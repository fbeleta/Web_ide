using System.ComponentModel.DataAnnotations;

namespace WebIde.Model;

// Personal Access Token — lets headless clients (VS Code extension, MCP server)
// authenticate against /api without a browser cookie. Bound to a domain User so
// submissions attribute to the right person. The raw token is shown once at
// creation; only its SHA-256 hash is stored.
public class ApiToken
{
    [Key]
    public int Id { get; set; }

    // Owning domain user (Submission.UserId etc. reference this same key).
    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;

    // Human label so users can tell tokens apart ("laptop vscode", "mcp").
    [Required, MaxLength(100)]
    public string Name { get; set; } = "";

    // Displayed, non-secret prefix (e.g. "webide_pat_ab12cd34") for the token list.
    [Required, MaxLength(32)]
    public string Prefix { get; set; } = "";

    // SHA-256 (hex) of the full raw token. Never store the raw token.
    [Required, MaxLength(64)]
    public string TokenHash { get; set; } = "";

    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}
