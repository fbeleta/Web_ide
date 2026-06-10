using System.Text.Json.Serialization;

namespace WebIde.Worker.Models;

// Matches the JSON object each sandbox wrapper emits per test case.
public record SandboxCaseResult(
    [property: JsonPropertyName("id")]      int    Id,
    [property: JsonPropertyName("verdict")] string Verdict,
    [property: JsonPropertyName("wallMs")]  int    WallMs,
    [property: JsonPropertyName("peakKb")]  int    PeakKb,
    [property: JsonPropertyName("stdout")]  string Stdout,
    [property: JsonPropertyName("stderr")]  string Stderr);
