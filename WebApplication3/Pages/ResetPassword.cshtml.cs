using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NanoidDotNet;
using Newtonsoft.Json;
using static WebApplication3.Pages.RegisterModel;
using System.Text.Encodings.Web;
using WebApplication3.Model;
using WebApplication3.ViewModels;

namespace WebApplication3.Pages
{
    public class ResetPasswordModel : PageModel
    {

        [BindProperty]
        public ResetPassword RPModel { get; set; }

        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public ResetPasswordModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync(string EmailAccount, string ResetToken)

        {
            {
                var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
                var protector = dataProtectionProvider.CreateProtector("MySecretKey");

                var UnprotectedEmail = protector.Unprotect(EmailAccount);

                var UnprotectedToken = protector.Unprotect(ResetToken);

                var user = await userManager.FindByEmailAsync(UnprotectedEmail);

                var previousPasswordsArray = user.PreviousPasswords?.Split('|');
                if (user != null)
                {
                    if (RPModel.Password != protector.Unprotect(previousPasswordsArray[0]) && RPModel.Password != protector.Unprotect(previousPasswordsArray[1]))
                    {

                        var result = await userManager.ResetPasswordAsync(user, UnprotectedToken, RPModel.Password);

                        var NewPreviousPasswords = $"{protector.Protect(RPModel.Password)}|{previousPasswordsArray[0]}";

                        user.PreviousPasswords = NewPreviousPasswords;

                        user.LastPasswordChange = DateTime.Now;

                        await userManager.UpdateAsync(user);

                        if (result.Succeeded)
                        {
                            TempData["SuccessMessage"] = "Password was successfully reset.";
                            return RedirectToPage("Login");
                        }

                        else
                        {
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError("", error.Description);
                            }
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Password cannot be the same as previous 2 passwords.";
                        return RedirectToPage("/ResetPassword", new { email = EmailAccount });
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToPage("/ResetPassword", new { email = EmailAccount });
                }
            }
            return Page();
        }
    }
}