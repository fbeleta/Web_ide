using System.Collections.Concurrent;
using System.Text.Json;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebIde.Model;
using WebIde.Worker.Models;

namespace WebIde.Worker.Services;

public class SandboxRunResult(int exitCode, string stdout, string stderr)
{
    public int    ExitCode { get; } = exitCode;
    public string Stdout   { get; } = stdout;
    public string Stderr   { get; } = stderr;
}

public class SandboxOrchestrator(
    DockerClient docker,
    IOptions<WorkerOptions>  workerOpts,
    IOptions<SandboxOptions> sandboxOpts,
    ILogger<SandboxOrchestrator> logger)
{
    private readonly SemaphoreSlim _slots =
        new(workerOpts.Value.MaxConcurrentSandboxes, workerOpts.Value.MaxConcurrentSandboxes);

    private readonly ConcurrentDictionary<int, byte> _active = new();

    // Built once: ["no-new-privileges", "seccomp=<json content>"] — the Docker
    // Engine API requires the seccomp profile *content*, not a file path.
    private readonly string[] _securityOpts = BuildSecurityOpts(sandboxOpts.Value, logger);

    public IReadOnlyCollection<int> ActiveSubmissionIds => (IReadOnlyCollection<int>)_active.Keys;

    private static string[] BuildSecurityOpts(SandboxOptions opts, ILogger logger)
    {
        var path = opts.SeccompProfilePath;
        if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
        {
            var json = File.ReadAllText(path);
            return new[] { "no-new-privileges", $"seccomp={json}" };
        }

        logger.LogWarning(
            "Seccomp profile not found at '{Path}'; falling back to Docker's default profile",
            path);
        return new[] { "no-new-privileges" };
    }

    public async Task<SandboxRunResult> RunAsync(SubmissionJob job, Problem problem, IList<TestCase> testCases, CancellationToken ct)
    {
        await _slots.WaitAsync(ct);
        _active[job.SubmissionId] = 0;
        var srcDir = Path.Combine("/tmp/webide-src", job.SubmissionId.ToString());

        try
        {
            return await RunContainerAsync(job, problem, testCases, srcDir, ct);
        }
        finally
        {
            _active.TryRemove(job.SubmissionId, out _);
            _slots.Release();
            if (Directory.Exists(srcDir))
                Directory.Delete(srcDir, recursive: true);
        }
    }

    private async Task<SandboxRunResult> RunContainerAsync(
        SubmissionJob job, Problem problem, IList<TestCase> testCases,
        string srcDir, CancellationToken ct)
    {
        Directory.CreateDirectory(srcDir);
        var (image, ext) = MapLanguage(job.Language, sandboxOpts.Value);
        File.WriteAllText(Path.Combine(srcDir, $"solution.{ext}"), job.SourceCode);
        File.WriteAllText(Path.Combine(srcDir, "cases.json"), BuildCasesJson(problem, testCases));

        var memBytes = (long)workerOpts.Value.SandboxMemMb * 1024 * 1024;
        var createParams = new CreateContainerParameters
        {
            Image = image,
            Cmd   = new[] { $"/code/solution.{ext}", "/code/cases.json" },
            User  = "nobody:nogroup",
            AttachStdout = true,
            AttachStderr = true,
            HostConfig = new HostConfig
            {
                NetworkMode    = "none",
                ReadonlyRootfs = true,
                // exec is required: the C/C++ wrapper compiles to /tmp/a.out and
                // executes it. Docker mounts --tmpfs noexec by default, which would
                // make every C/C++ submission fail with "Permission denied".
                Tmpfs = new Dictionary<string, string> { ["/tmp"] = "size=64m,mode=1777,exec" },
                Memory      = memBytes,
                MemorySwap  = memBytes,
                NanoCPUs    = (long)(workerOpts.Value.SandboxCpus * 1_000_000_000),
                PidsLimit   = 64,
                SecurityOpt = _securityOpts,
                CapDrop     = new[] { "ALL" },
                Ulimits     = new[] { new Ulimit { Name = "fsize", Soft = 67108864, Hard = 67108864 } },
                Mounts      = new List<Mount>
                {
                    new() { Type = "bind", Source = srcDir, Target = "/code", ReadOnly = true }
                },
                AutoRemove = false,
            },
        };

        var created = await docker.Containers.CreateContainerAsync(createParams, ct);
        var id = created.ID;
        logger.LogDebug("Container {Id} created for submission {Sub}", id[..12], job.SubmissionId);

        // Attach before start so we cannot miss any output
        using var attachStream = await docker.Containers.AttachContainerAsync(id, false,
            new ContainerAttachParameters { Stdout = true, Stderr = true, Stream = true }, ct);

        await docker.Containers.StartContainerAsync(id, null, ct);

        // Hard timeout: per-case limit × cases + 30 s overhead
        var hardMs = (long)job.TimeLimitMs * Math.Max(testCases.Count, 1) + 30_000;
        using var hardCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        hardCts.CancelAfter(TimeSpan.FromMilliseconds(hardMs));

        string stdout, stderr;
        try
        {
            (stdout, stderr) = await attachStream.ReadOutputToEndAsync(hardCts.Token);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            // Hard timeout — force-remove the container and return timeout sentinel
            logger.LogWarning("Hard timeout hit for submission {Sub}", job.SubmissionId);
            await ForceRemoveAsync(id);
            return new SandboxRunResult(124, "", "hard timeout");
        }

        var waitResp = await docker.Containers.WaitContainerAsync(id, ct);
        await ForceRemoveAsync(id);

        return new SandboxRunResult((int)waitResp.StatusCode, stdout, stderr);
    }

    private async Task ForceRemoveAsync(string containerId)
    {
        try
        {
            await docker.Containers.RemoveContainerAsync(
                containerId,
                new ContainerRemoveParameters { Force = true },
                CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not remove container {Id}", containerId[..12]);
        }
    }

    private static string BuildCasesJson(Problem problem, IList<TestCase> testCases)
    {
        var obj = new
        {
            timeLimitMs   = problem.TimeLimitMs,
            floatTolerance = problem.FloatTolerance,
            cases = testCases.OrderBy(tc => tc.OrderIndex).Select(tc => new
            {
                id       = tc.Id,
                stdin    = tc.InputArgs,
                expected = tc.ExpectedOutput,
                points   = tc.Points,
            }),
        };
        return JsonSerializer.Serialize(obj);
    }

    private static (string image, string ext) MapLanguage(string language, SandboxOptions opts) =>
        language switch
        {
            "python"     => (opts.PythonImage, "py"),
            "cpp"        => (opts.GccImage,    "cpp"),
            "c"          => (opts.GccImage,    "c"),
            "javascript" => (opts.NodeImage,   "js"),
            _            => throw new ArgumentException($"Unsupported language: {language}"),
        };
}
