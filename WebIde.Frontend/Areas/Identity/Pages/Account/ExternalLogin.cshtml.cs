using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebIde.DAL;

namespace WebIde.Web.Areas.Identity.Pages.Account;

public class ExternalLoginModel : PageModel
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;

    public ExternalLoginModel(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
    {
        _signInManager = signInManager;
        _userManager   = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string ProviderDisplayName { get; set; } = "";
    public string? StatusMessage { get; set; }

    public class InputModel
    {
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [Required]
        [StringLength(11, MinimumLength = 11)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "OIB smije sadržavati samo brojeve.")]
        [Display(Name = "OIB")]
        public string OIB { get; set; } = "";

        [Required]
        [StringLength(13, MinimumLength = 13)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "JMBG smije sadržavati samo brojeve.")]
        [Display(Name = "JMBG")]
        public string JMBG { get; set; } = "";
    }

    public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl = null, string? remoteError = null)
    {
        returnUrl ??= Url.Content("~/");

        if (remoteError != null)
        {
            StatusMessage = $"Error from external provider: {remoteError}";
            return RedirectToPage("./Login");
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null) return RedirectToPage("./Login");

        // Try to sign in with the external login — succeeds if user already linked
        var result = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

        if (result.Succeeded) return LocalRedirect(returnUrl);

        // First time — show registration form
        ProviderDisplayName = info.ProviderDisplayName ?? info.LoginProvider;
        var email = info.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
        Input = new InputModel { Email = email };
        return Page();
    }

    public async Task<IActionResult> OnPostConfirmationAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            StatusMessage = "Error loading external login information.";
            return RedirectToPage("./Login");
        }

        ProviderDisplayName = info.ProviderDisplayName ?? info.LoginProvider;

        if (!ModelState.IsValid) return Page();

        var user = new AppUser
        {
            UserName = Input.Email,
            Email    = Input.Email,
            OIB      = Input.OIB,
            JMBG     = Input.JMBG,
        };

        var createResult = await _userManager.CreateAsync(user);
        if (createResult.Succeeded)
        {
            await _userManager.AddLoginAsync(user, info);
            await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
            return LocalRedirect(returnUrl);
        }

        foreach (var error in createResult.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return Page();
    }
}
