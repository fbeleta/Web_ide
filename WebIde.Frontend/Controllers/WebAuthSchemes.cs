namespace WebIde.Web.Controllers;

internal static class WebAuthSchemes
{
    // Browser cookie schemes that may satisfy an admin/manager MVC page:
    //   "Cookies"              — the default cookie the GitHub OAuth flow signs into
    //   "Identity.Application" — the ASP.NET Core Identity (username/password) cookie
    // Both carry a ClaimTypes.Role derived from the DB role, so admin is decided by
    // the DomainUsers/Identity role regardless of which table the user signed in from.
    public const string Cookies = "Cookies,Identity.Application";
}
