using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Threading.RateLimiting;
using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using WebIde.DAL;
using WebIde.Web.Hubs;
using WebIde.Web.Repositories;
using WebIde.Web.Services;

var builder = WebApplication.CreateBuilder(args);
var config  = builder.Configuration;

// ── Redis ─────────────────────────────────────────────────────────────────────
// Lazy singleton — Connect() is deferred until first DI resolution so that
// test factories can swap in a mock before any connection is attempted.
var redisConnectionString = config["Redis:ConnectionString"] ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(redisConnectionString));

// ── DataProtection — persisted to Redis so deploys don't log everyone out ─────
// Uses a custom IXmlRepository that resolves IConnectionMultiplexer from DI,
// keeping the lazy pattern so tests can inject a mock without triggering Connect().
builder.Services.AddDataProtection().SetApplicationName("WebIde");
builder.Services.AddSingleton<IConfigureOptions<KeyManagementOptions>>(sp =>
    new ConfigureOptions<KeyManagementOptions>(opts =>
        opts.XmlRepository = new RedisDataProtectionRepository(
            sp.GetRequiredService<IConnectionMultiplexer>())));

// ── MVC + SignalR + Razor Pages ───────────────────────────────────────────────
builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter()));
builder.Services.AddRazorPages();
builder.Services.AddSignalR().AddStackExchangeRedis(redisConnectionString);

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<WebIdeDbContext>(options =>
    options.UseNpgsql(
        config.GetConnectionString("WebIdeDb"),
        o => o.MigrationsAssembly("WebIde.DAL")));

// ── Repositories ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<ProblemRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<OrganizationRepository>();
builder.Services.AddScoped<ProblemSetRepository>();
builder.Services.AddScoped<SubmissionRepository>();
builder.Services.AddScoped<TagRepository>();
builder.Services.AddScoped<TestCaseRepository>();
builder.Services.AddScoped<ExecutionResultRepository>();

// ── Authentication — Cookie + GitHub OAuth ─────────────────────────────────────
// AddAuthentication configures the default scheme (GitHub cookie).
// We use AddIdentityCore (not AddDefaultIdentity) so Identity does NOT override
// the DefaultScheme — that would break the existing GitHub OAuth flow.
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme          = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GitHubAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.HttpOnly    = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite    = SameSiteMode.Lax;
    options.ExpireTimeSpan     = TimeSpan.FromDays(14);
    options.SlidingExpiration  = true;
    options.LoginPath          = "/auth/github/login";
    options.LogoutPath         = "/auth/logout";
})
.AddGitHub(options =>
{
    options.ClientId     = config["GitHub:ClientId"] ?? "";
    options.ClientSecret = config["GitHub:ClientSecret"] ?? "";
    options.CallbackPath = "/auth/github/callback";
    options.Scope.Add("user:email");

    options.Events.OnCreatingTicket = async ctx =>
    {
        var githubId    = ctx.Principal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var username    = ctx.Principal.FindFirstValue(ClaimTypes.Name) ?? githubId;
        var displayName = ctx.Principal.FindFirstValue("urn:github:name")
                       ?? ctx.Principal.FindFirstValue(ClaimTypes.Name)
                       ?? username;
        var email       = ctx.Principal.FindFirstValue(ClaimTypes.Email) ?? "";
        var avatarUrl   = ctx.Principal.FindFirstValue("urn:github:avatar_url") ?? "";

        await using var scope   = ctx.HttpContext.RequestServices.CreateAsyncScope();
        var userRepo            = scope.ServiceProvider.GetRequiredService<UserRepository>();
        var user                = await userRepo.UpsertGitHubUserAsync(githubId, username, displayName, email, avatarUrl);

        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim("webide:userId",      user.Id.ToString()));
        identity.AddClaim(new Claim("webide:avatarUrl",   user.AvatarUrl ?? ""));
        identity.AddClaim(new Claim("webide:displayName", user.DisplayName));
        ctx.Principal!.AddIdentity(identity);
    };
})
// ── Identity cookies (ApplicationScheme, ExternalScheme, 2FA) ─────────────────
// These three are required by SignInManager. They coexist with the GitHub
// cookie above because they use different cookie names and scheme keys.
.AddCookie(IdentityConstants.ApplicationScheme, options =>
{
    options.Cookie.HttpOnly    = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite    = SameSiteMode.Lax;
    options.ExpireTimeSpan     = TimeSpan.FromDays(14);
    options.SlidingExpiration  = true;
    options.LoginPath          = "/Identity/Account/Login";
    options.LogoutPath         = "/Identity/Account/Logout";
})
.AddCookie(IdentityConstants.ExternalScheme)
.AddCookie(IdentityConstants.TwoFactorUserIdScheme);
// Google OAuth disabled — add Google:ClientId and Google:ClientSecret to enable
// .AddGoogle(options =>
// {
//     options.ClientId     = config["Google:ClientId"] ?? "";
//     options.ClientSecret = config["Google:ClientSecret"] ?? "";
//     options.SignInScheme  = IdentityConstants.ExternalScheme;
// });


// ── Identity — services only (no AddAuthentication override) ──────────────────
builder.Services.AddIdentityCore<AppUser>(options =>
{
    options.Password.RequireDigit           = true;
    options.Password.RequiredLength         = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase       = false;
    options.SignIn.RequireConfirmedAccount  = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<WebIdeDbContext>()
.AddSignInManager()
.AddDefaultTokenProviders();

// ── Rate limiter (inner defense — nginx has outer limits in nginx.conf) ───────
builder.Services.AddRateLimiter(o =>
{
    o.AddFixedWindowLimiter("submission", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window      = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit  = 0;
    });
    o.AddTokenBucketLimiter("auth", opt =>
    {
        opt.TokenLimit          = 10;
        opt.TokensPerPeriod     = 10;
        opt.ReplenishmentPeriod = TimeSpan.FromMinutes(1);
    });
    o.AddConcurrencyLimiter("hub", opt =>
    {
        opt.PermitLimit = 100;
        opt.QueueLimit  = 0;
    });
    o.RejectionStatusCode = 429;
});

// ── Antiforgery ───────────────────────────────────────────────────────────────
builder.Services.AddAntiforgery();

// ── Health checks ─────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddNpgSql(
        config.GetConnectionString("WebIdeDb")!,
        name: "postgres",
        tags: ["ready"])
    .AddRedis(
        redisConnectionString,
        name: "redis",
        tags: ["ready"])
    .AddCheck<WorkerHealthCheck>(
        "worker",
        tags: ["ready"]);

// ── Build ─────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Seed roles ────────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var role in new[] { "Admin", "Manager" })
        if (!await roleMgr.RoleExistsAsync(role))
            await roleMgr.CreateAsync(new IdentityRole(role));
}

// ── ForwardedHeaders — must be first so scheme/IP are correct for OAuth ───────
// Trust X-Forwarded-* from the nginx container on the Docker bridge network.
// Docker Compose assigns 172.16.0.0/12 by default; 172.0.0.0/8 covers all
// common Docker bridge ranges without opening up to the public internet.
var fwdOpts = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
};
fwdOpts.KnownIPNetworks.Clear();
fwdOpts.KnownProxies.Clear();
// Accept forwarded headers only from RFC-1918 private ranges — covers any
// Docker bridge subnet (172.x.x.x / 10.x.x.x) while refusing spoofing
// from public IPs if the port were ever accidentally exposed.
fwdOpts.KnownIPNetworks.Add(new System.Net.IPNetwork(IPAddress.Parse("172.0.0.0"), 8));
fwdOpts.KnownIPNetworks.Add(new System.Net.IPNetwork(IPAddress.Parse("10.0.0.0"), 8));
fwdOpts.KnownIPNetworks.Add(new System.Net.IPNetwork(IPAddress.Parse("127.0.0.0"), 8));
app.UseForwardedHeaders(fwdOpts);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

var supportedCultures = new[] { new CultureInfo("hr"), new CultureInfo("en-US") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("hr"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.UseRateLimiter();
app.UseAuthentication();    // MUST be before Authorization
app.UseAuthorization();

app.MapStaticAssets();

app.MapHub<ExecutionHub>("/hubs/execution").RequireRateLimiting("hub");
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = c => c.Tags.Contains("ready"),
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

app.Run();

// Required for WebApplicationFactory<Program> in test project
public partial class Program { }
