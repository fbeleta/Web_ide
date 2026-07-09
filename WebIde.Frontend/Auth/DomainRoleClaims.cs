using System.Security.Claims;
using WebIde.Model.Enums;

namespace WebIde.Web.Auth;

/// <summary>
/// Single source of truth for turning a <see cref="UserRole"/> stored on a
/// DomainUsers row into the role name used by <c>[Authorize(Roles = ...)]</c>.
///
/// Used by every scheme that authenticates a domain user — GitHub OAuth
/// (<c>OnCreatingTicket</c>) and Personal Access Tokens — so that "admin in the
/// DB is admin regardless of which table/scheme signed you in".
/// </summary>
public static class DomainRoleClaims
{
    /// <summary>Maps the domain role to an authorization role name, or null for Student.</summary>
    public static string? RoleName(UserRole role) => role switch
    {
        UserRole.Admin      => "Admin",
        UserRole.Instructor => "Manager",
        _                   => null,
    };

    /// <summary>Adds a <see cref="ClaimTypes.Role"/> claim to the identity when the role grants one.</summary>
    public static void AddRoleClaim(ClaimsIdentity identity, UserRole role)
    {
        var name = RoleName(role);
        if (name is not null)
            identity.AddClaim(new Claim(ClaimTypes.Role, name));
    }
}
