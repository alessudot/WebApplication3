using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using WebApplication3.Model;
using WebApplication3.ViewModels;
using System.Text.Encodings.Web;

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
                var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
                var protector = dataProtectionProvider.CreateProtector("MySecretKey");

                var result = await signInManager.PasswordSignInAsync(LModel.Email, LModel.Password,
                    LModel.RememberMe, true);
                var currentUser = await userManager.FindByEmailAsync(LModel.Email);
                if (result.Succeeded)
                {
                    currentUser.LastLogin = DateTime.Now;
                    await userManager.UpdateAsync(currentUser);
                    if (currentUser != null)
                    {
                        contxt.HttpContext.Session.SetString("LoggedIn", "True");
                        contxt.HttpContext.Session.SetString("Email", protector.Protect(LModel.Email));
                        contxt.HttpContext.Session.SetString("Password", protector.Protect(LModel.Password));
                        contxt.HttpContext.Session.SetString("Full Name", currentUser.FullName.ToString());
                        contxt.HttpContext.Session.SetString("Credit Card", currentUser.CreditCard.ToString());
                        contxt.HttpContext.Session.SetString("Gender", currentUser.Gender.ToString());
                        contxt.HttpContext.Session.SetString("Mobile Number", currentUser.MobileNo.ToString());
                        contxt.HttpContext.Session.SetString("Delivery Address", currentUser.DeliveryAddress.ToString());
                        contxt.HttpContext.Session.SetString("About Me", currentUser.AboutMe.ToString());
                        contxt.HttpContext.Session.SetString("Photo Path", currentUser.PhotoPath.ToString());
                        contxt.HttpContext.Session.SetString("Last Login", currentUser.LastLogin.ToString());
                        contxt.HttpContext.Session.SetString("Previous Passwords", currentUser.PreviousPasswords.ToString());
						contxt.HttpContext.Session.SetString("Last Password Change", currentUser.LastPasswordChange.ToString());
						string authToken = Guid.NewGuid().ToString();

                        contxt.HttpContext.Session.SetString("AuthToken", authToken);

                        contxt.HttpContext.Response.Cookies.Append("AuthToken", authToken, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
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
                else if (result.IsLockedOut)
                {
                    TempData["ErrorMessage"] = "Account is locked out due to 3 failed login attempts. Please try again in later.";
                    return RedirectToPage();
                }
                else if (currentUser.TwoFactorEnabled != true || currentUser.EmailConfirmed != true)
                {
                    TempData["ErrorMessage"] = "Your account is not activated, please check your email for an activation link.";
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
