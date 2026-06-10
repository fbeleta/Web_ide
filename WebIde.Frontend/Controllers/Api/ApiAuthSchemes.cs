namespace WebIde.Web.Controllers.Api;

internal static class ApiAuthSchemes
{
    // IdentityConstants.ApplicationScheme = "Identity.Application"
    // Cannot be used directly in attribute arguments (not a const), so we define it here.
    public const string Identity = "Identity.Application";
}
