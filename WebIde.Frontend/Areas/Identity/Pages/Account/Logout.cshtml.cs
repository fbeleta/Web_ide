using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebIde.DAL;

namespace WebIde.Web.Areas.Identity.Pages.Account;

public class LogoutModel : PageModel
{
    private readonly SignInManager<AppUser> _signInManager;

    public LogoutModel(SignInManager<AppUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public async Task<IActionResult> OnPost(string? returnUrl = null)
    {
        await _signInManager.SignOutAsync();
        return returnUrl != null ? LocalRedirect(returnUrl) : RedirectToPage();
    }
}
