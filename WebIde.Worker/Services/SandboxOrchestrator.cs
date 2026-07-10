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

    // Built once: ["no-new-privileges", "apparmor=unconfined", "seccomp=<json>"].
    // The Docker Engine API requires the seccomp profile *content*, not a file path.
    private readonly string[] _securityOpts = BuildSecurityOpts(sandboxOpts.Value, logger);

    public IReadOnlyCollection<int> ActiveSubmissionIds => (IReadOnlyCollection<int>)_active.Keys;

    private static string[] BuildSecurityOpts(SandboxOptions opts, ILogger logger)
    {
        // apparmor=unconfined: the host's docker-default AppArmor profile denies exec of the
        // compiled binary on the /work bind mount (EACCES), which broke every C/C++ run and
        // made Python/JS crash to InternalError when their helpers couldn't exec. The sandbox
        // is still hard-isolated by seccomp + cap-drop ALL + read-only rootfs + network none +
        // no-new-privileges + pids/memory limits, so dropping the redundant AppArmor layer is
        // an acceptable trade to restore execution.
        var path = opts.SeccompProfilePath;
        if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
        {
            var json = File.ReadAllText(path);
            return new[] { "no-new-privileges", "apparmor=unconfined", $"seccomp={json}" };
        }

        logger.LogWarning(
            "Seccomp profile not found at '{Path}'; falling back to Docker's default profile",
            path);
        return new[] { "no-new-privileges", "apparmor=unconfined" };
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
        // Writable, exec-capable dir for the compiled binary. It must be a bind mount
        // (host root fs is exec) because Docker.DotNet can't produce an exec tmpfs — the
        // Tmpfs "exec" option is ignored via the API, so /tmp is always noexec. Kept
        // separate from /code so a running solution can't tamper with cases.json.
        var workDir = Path.Combine(srcDir, "work");
        Directory.CreateDirectory(workDir);
        // The sandbox runs as nobody (uid 65534) but the worker created workDir as its own
        // uid; make it world-writable so the compiler can write /work/a.out. We can't chown
        // to 65534 (the worker is non-root). The dir is per-submission and destroyed after.
        if (OperatingSystem.IsLinux())
            File.SetUnixFileMode(workDir,
                UnixFileMode.UserRead  | UnixFileMode.UserWrite  | UnixFileMode.UserExecute  |
                UnixFileMode.GroupRead | UnixFileMode.GroupWrite | UnixFileMode.GroupExecute |
                UnixFileMode.OtherRead | UnixFileMode.OtherWrite | UnixFileMode.OtherExecute);
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
                // tmpfs for the wrapper's stdin/stdout/stderr temp files (write-only, no
                // exec needed). The compiled binary runs from the /work bind mount below,
                // because this tmpfs is always noexec (Docker.DotNet ignores the exec option).
                Tmpfs = new Dictionary<string, string> { ["/tmp"] = "size=64m,mode=1777" },
                Memory      = memBytes,
                MemorySwap  = memBytes,
                NanoCPUs    = (long)(workerOpts.Value.SandboxCpus * 1_000_000_000),
                PidsLimit   = 64,
                SecurityOpt = _securityOpts,
                CapDrop     = new[] { "ALL" },
                Ulimits     = new[] { new Ulimit { Name = "fsize", Soft = 67108864, Hard = 67108864 } },
                Mounts      = new List<Mount>
                {
                    new() { Type = "bind", Source = srcDir,  Target = "/code", ReadOnly = true  },
                    new() { Type = "bind", Source = workDir, Target = "/work", ReadOnly = false },
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
