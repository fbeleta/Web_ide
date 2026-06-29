namespace WebIde.Worker.Models;

public class WorkerOptions
{
    public int MaxConcurrentSandboxes { get; set; } = 2;
    public int SandboxMemMb { get; set; } = 512;
    public double SandboxCpus { get; set; } = 0.9;
}

public class SandboxOptions
{
    public string GccDigest { get; set; } = "";
    public string PythonDigest { get; set; } = "";
    public string NodeDigest { get; set; } = "";

    // Path (inside the worker container) to the custom seccomp profile.
    // The Docker Engine API expects the profile JSON *content*, not a path
    // (only the docker CLI resolves file paths), so the orchestrator reads
    // this file and inlines its contents. If the file is missing (e.g. local
    // `dotnet run` outside the container), the default Docker profile is used.
    public string SeccompProfilePath { get; set; } = "/sandbox/seccomp-profile.json";

    public string GccImage    => Resolve("gcc",    GccDigest);
    public string PythonImage => Resolve("python", PythonDigest);
    public string NodeImage   => Resolve("node",   NodeDigest);

    private static string Resolve(string lang, string digest) =>
        string.IsNullOrEmpty(digest)
            ? $"webide-sandbox-{lang}"
            : digest.StartsWith("sha256:", StringComparison.Ordinal)
                ? $"ghcr.io/fbeleta/web_ide/sandbox-{lang}@{digest}"
                : digest;
}
