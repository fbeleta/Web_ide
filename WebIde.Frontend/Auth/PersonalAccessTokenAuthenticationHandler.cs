using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebIde.DAL;
using WebIde.Web.Services;

namespace WebIde.Web.Auth;

// Authenticates /api requests carrying "Authorization: Bearer webide_pat_...".
// Resolves the token to its owning domain User and issues a principal whose
// NameIdentifier is the domain user's int id — so SubmissionApiController
// attributes API submissions to the right person (fixing the synthetic userId-0
// fallback) and ExecutionHub can read webide:userId.
public sealed class PersonalAccessTokenAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    WebIdeDbContext db)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "PersonalAccessToken";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Only act on our own Bearer tokens; let other schemes (cookie) handle the
        // rest so browser requests are unaffected.
        string? header = Request.Headers.Authorization;
        if (string.IsNullOrEmpty(header) || !header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.NoResult();

        var raw = header["Bearer ".Length..].Trim();
        if (!ApiTokenService.LooksLikeToken(raw))
            return AuthenticateResult.NoResult();

        var hash = ApiTokenService.Hash(raw);
        var now  = DateTime.UtcNow;

        var token = await db.ApiTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == hash);

        if (token is null || token.RevokedAt != null || (token.ExpiresAt != null && token.ExpiresAt <= now))
            return AuthenticateResult.Fail("Invalid or expired token.");

        if (token.User is null || token.User.DeletedAt != null)
            return AuthenticateResult.Fail("Token owner no longer exists.");

        // Best-effort last-used stamp; never fail auth if the write throws.
        try
        {
            token.LastUsedAt = now;
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to update ApiToken.LastUsedAt for token {Id}.", token.Id);
        }

        var user   = token.User;
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name,           user.Username),
            new(ClaimTypes.Email,          user.Email),
            new("webide:userId",           user.Id.ToString()),
            new("webide:displayName",      user.DisplayName),
        };

        var identity  = new ClaimsIdentity(claims, SchemeName);
        // Map the domain role onto the role names [Authorize(Roles=...)] checks
        // ("Admin", "Manager") — same mapping used by the GitHub OAuth flow.
        DomainRoleClaims.AddRoleClaim(identity, user.Role);
        var principal = new ClaimsPrincipal(identity);
        return AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName));
    }
}
