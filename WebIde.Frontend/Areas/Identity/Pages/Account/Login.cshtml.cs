using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebIde.DAL;
using WebIde.Web.Repositories;

namespace WebIde.Web.Areas.Identity.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserRepository _userRepository;

    public LoginModel(SignInManager<AppUser> signInManager, UserRepository userRepository)
    {
        _signInManager = signInManager;
        _userRepository = userRepository;
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

    public async Task OnGetAsync()
    {
        ExternalProviders = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        ExternalProviders = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (!ModelState.IsValid) return Page();

        var result = await _signInManager.PasswordSignInAsync(
            Input.Email, Input.Password, isPersistent: false, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            // Identity signs into Identity.Application only. The site keys off the
            // default "Cookies" scheme + webide:* claims (like GitHub login), so we
            // also establish that identity here — otherwise the navbar/submissions
            // never see the user as logged in.
            var appUser     = await _signInManager.UserManager.FindByEmailAsync(Input.Email);
            var username    = appUser?.UserName ?? Input.Email;
            var displayName = username;
            var domainUser  = await _userRepository.UpsertLocalUserAsync(Input.Email, username, displayName);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, domainUser.Id.ToString()),
                new(ClaimTypes.Name,           domainUser.DisplayName),
                new("webide:userId",           domainUser.Id.ToString()),
                new("webide:displayName",      domainUser.DisplayName),
                new("webide:avatarUrl",        domainUser.AvatarUrl ?? ""),
            };
            var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return LocalRedirect(returnUrl);
        }

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
