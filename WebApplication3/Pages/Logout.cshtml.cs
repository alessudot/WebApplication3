using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication3.Model;

namespace WebApplication3.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IHttpContextAccessor contxt;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, IHttpContextAccessor contxt)
        {
            this.signInManager = signInManager;
            this.contxt = contxt;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrEmpty(contxt.HttpContext.Session.GetString("LoggedIn")) &&
                string.IsNullOrEmpty(contxt.HttpContext.Session.GetString("AuthToken")))
            {
                await ClearSessionAsync();
                return RedirectToPage("Index");
            }
            else
            {
                return Page();
            }
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await signInManager.SignOutAsync();
            await ClearSessionAsync();
            return RedirectToPage("Index");
        }

        public IActionResult OnPostDontLogoutAsync()
        {
            return RedirectToPage("Index");
        }

        private async Task ClearSessionAsync()
        {
            contxt.HttpContext.Session.Clear();

            if (Request.Cookies.ContainsKey("ASP.NET_SessionId"))
            {
                Response.Cookies.Delete("ASP.NET_SessionId");
            }

            if (Request.Cookies.ContainsKey("AuthToken"))
            {
                Response.Cookies.Delete("AuthToken");
            }
        }
    }
}
