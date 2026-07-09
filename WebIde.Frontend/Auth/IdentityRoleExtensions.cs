using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace WebIde.Web.Auth;

/// <summary>
/// Helpers for deciding whether the current request may see admin/manager UI.
///
/// Admin is decided by the DB role regardless of which table signed the user in:
///  - GitHub users authenticate via the default cookie, so their role claim is
///    already on <c>HttpContext.User</c>.
///  - Identity (username/password) users sign into a *separate* cookie
///    (<see cref="IdentityConstants.ApplicationScheme"/>) which is not the active
///    principal, so we authenticate that scheme explicitly as a fallback.
/// The result is cached per-request in <see cref="HttpContext.Items"/>.
/// </summary>
public static class IdentityRoleExtensions
{
    public static async Task<bool> IsIdentityInRoleAsync(this HttpContext http, params string[] roles)
    {
        // GitHub (default cookie) principal is already on HttpContext.User.
        if (http.User?.Identity?.IsAuthenticated == true && roles.Any(http.User.IsInRole))
            return true;

        // Fall back to the Identity (username/password) cookie.
        var result = await http.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (!result.Succeeded || result.Principal is null) return false;
        return roles.Any(result.Principal.IsInRole);
    }

    /// <summary>True when signed into Identity as Admin or Manager (problem/content admin).</summary>
    public static async Task<bool> IsIdentityManagerAsync(this HttpContext http)
    {
        const string key = "webide:isManager";
        if (http.Items.TryGetValue(key, out var cached) && cached is bool b) return b;
        var result = await http.IsIdentityInRoleAsync("Admin", "Manager");
        http.Items[key] = result;
        return result;
    }

    /// <summary>True when signed into Identity as Admin.</summary>
    public static async Task<bool> IsIdentityAdminAsync(this HttpContext http)
    {
        const string key = "webide:isAdmin";
        if (http.Items.TryGetValue(key, out var cached) && cached is bool b) return b;
        var result = await http.IsIdentityInRoleAsync("Admin");
        http.Items[key] = result;
        return result;
    }
}
