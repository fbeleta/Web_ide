namespace WebIde.Web.Controllers.Api;

internal static class ApiAuthSchemes
{
    // IdentityConstants.ApplicationScheme = "Identity.Application"
    // Cannot be used directly in attribute arguments (not a const), so we define it here.
    public const string Identity = "Identity.Application";

    // Schemes accepted on /api endpoints: the Identity browser cookie AND the
    // Personal Access Token bearer scheme, so both interactive users and headless
    // clients (VS Code extension, MCP server) can authenticate.
    public const string Api = "Identity.Application,PersonalAccessToken";
}
