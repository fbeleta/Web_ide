using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebIde.DAL;

namespace WebIde.Web.Areas.Identity.Pages.Account;

// Anonymous, or the global authenticated-by-default posture challenges this page
// itself — and since the cookie LoginPath points here, that produces an infinite
// /Identity/Account/Login?ReturnUrl=… redirect loop (nginx 502).
[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly SignInManager<AppUser> _signInManager;

    public LoginModel(SignInManager<AppUser> signInManager)
    {
        _signInManager = signInManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public IList<AuthenticationScheme> ExternalProviders { get; set; } = [];

    public class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = "";
    }

    public async Task<IActionResult> OnGetAsync()
    {
        // Already signed in as an Identity user — no reason to show the login form.
        // Authenticate the Identity scheme explicitly: the default request principal
        // comes from the GitHub cookie, so IsSignedIn(User) wouldn't see it.
        var identityAuth = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (identityAuth.Succeeded)
            return LocalRedirect(Url.Content("~/"));

        ExternalProviders = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        ExternalProviders = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (!ModelState.IsValid) return Page();

        var result = await _signInManager.PasswordSignInAsync(
            Input.Email, Input.Password, isPersistent: false, lockoutOnFailure: false);

        if (result.Succeeded)
            return LocalRedirect(returnUrl);

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return Page();
    }

    public IActionResult OnPostExternalLogin(string provider, string? returnUrl = null)
    {
        var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
        var properties  = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return new ChallengeResult(provider, properties);
    }
}
