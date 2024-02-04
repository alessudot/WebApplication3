using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Mail;
using System.Net.Mime;
using System.Net;
using System.Text;
using WebApplication3.Model;
using System.Text.Json;

namespace WebApplication3.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IHttpContextAccessor contxt;
        private readonly SignInManager<ApplicationUser> signInManager;



        public IndexModel(ILogger<IndexModel> logger, IHttpContextAccessor httpContextAccessor, SignInManager<ApplicationUser> signInManager)
        {
            _logger = logger;
            contxt = httpContextAccessor;
            this.signInManager = signInManager;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await signInManager.SignOutAsync();
            contxt.HttpContext.Session.Clear();

            if (Request.Cookies.ContainsKey("ASP.NET_SessionId"))
            {
                Response.Cookies.Delete("ASP.NET_SessionId");
            }

            if (Request.Cookies.ContainsKey("AuthToken"))
            {
                Response.Cookies.Delete("AuthToken");
            }

            return RedirectToPage("Login");
        }

    }
}