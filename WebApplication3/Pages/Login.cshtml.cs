using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using WebApplication3.Model;
using WebApplication3.ViewModels;

namespace WebApplication3.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public Login LModel { get; set; }

        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IHttpContextAccessor contxt;

        public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IHttpContextAccessor contxt)
        {
            this.signInManager = signInManager;
            this.userManager = userManager; // Inject UserManager
            this.contxt = contxt;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(LModel.Email, LModel.Password,
                    LModel.RememberMe, lockoutOnFailure: true);
                var currentUser = await userManager.FindByEmailAsync(LModel.Email);
                if (result.Succeeded)
                {

                    if (currentUser != null)
                    {
                        contxt.HttpContext.Session.SetString("LoggedIn", "True");
                        contxt.HttpContext.Session.SetString("Full Name", currentUser.FullName.ToString());
                        contxt.HttpContext.Session.SetString("Credit Card", currentUser.CreditCard.ToString());
                        contxt.HttpContext.Session.SetString("Gender", currentUser.Gender.ToString());
                        contxt.HttpContext.Session.SetString("Mobile Number", currentUser.MobileNo.ToString());
                        contxt.HttpContext.Session.SetString("Delivery Address", currentUser.DeliveryAddress.ToString());
                        contxt.HttpContext.Session.SetString("About Me", currentUser.AboutMe.ToString());
                        contxt.HttpContext.Session.SetString("Photo Path", currentUser.PhotoPath.ToString());
                        string authToken = Guid.NewGuid().ToString();

                        contxt.HttpContext.Session.SetString("AuthToken", authToken);

                        contxt.HttpContext.Response.Cookies.Append("AuthToken", authToken, new CookieOptions
                        {
                            HttpOnly = true,
                            Expires = DateTimeOffset.UtcNow.AddDays(3)
                        });
                        return RedirectToPage("Index");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Unable to retrieve user information.";
                        return RedirectToPage();
                    }
                }
                if (result.IsLockedOut)
                {
                    TempData["ErrorMessage"] = "Account is locked out due to 3 failed login attempts. Please try again in later.";
                    return RedirectToPage();
                }
                else
                {
                    TempData["ErrorMessage"] = "Incorrect username or password. Please try again.";
                    return RedirectToPage();
                }
            }
            return Page();
        }
    }
}
