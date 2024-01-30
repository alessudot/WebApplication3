using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication3.Model;

namespace WebApplication3.Pages
{
    public class VerificationModel : PageModel
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public VerificationModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }
        public void OnGet()
        {
        }
        public async Task<IActionResult> OnPostAsync(string EmailAccount, string ResetToken)

        {
            var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
            var protector = dataProtectionProvider.CreateProtector("MySecretKey");

            {
                var UnprotectedEmail = protector.Unprotect(EmailAccount);
                var UnprotectedToken = protector.Unprotect(ResetToken);
                var user = await userManager.FindByEmailAsync(UnprotectedEmail);

                if (user != null)
                {
                    if (user.EmailConfirmed == false)
                    {
                        var result = await userManager.ConfirmEmailAsync(user, UnprotectedToken);

						if (result.Succeeded)
                        {
                            TempData["SuccessMessage"] = "Account was successfully activated.";
                            return RedirectToPage("Login");
                        }

                        else
                        {
                            TempData["ErrorMessage"] = "There was an error in activating your account.";
                            return RedirectToPage("/Login");
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Account is already activated.";
                        return RedirectToPage("/Login");
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid verfication link.";
                    return RedirectToPage("/Login");
                }
            }
            return Page();
        }
    }
}