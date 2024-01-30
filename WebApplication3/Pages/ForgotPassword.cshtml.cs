using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Mail;
using System.Net.Mime;
using System.Net;
using System.Text;
using WebApplication3.Model;
using WebApplication3.ViewModels;

namespace WebApplication3.Pages
{
    public class ForgotPasswordModel : PageModel
    {
        [BindProperty]
        public ForgotPassword FPModel { get; set; }

        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
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

                var currentUser = await userManager.FindByEmailAsync(FPModel.Email);
                if (currentUser != null)
                {
                    var ProtectedEmail = protector.Protect(FPModel.Email);
                    var token = await userManager.GeneratePasswordResetTokenAsync(currentUser);
                    var ProtectedToken = protector.Protect(token);

                    var smtpClient = new SmtpClient("smtp.ethereal.email")
                    {
                        Port = 587,
                        Credentials = new NetworkCredential("rose.swift@ethereal.email", "UN1wPX1jXm2Vw1bEGe"),
                        EnableSsl = true,
                    };

                    var attachment = Attachment.CreateAttachmentFromString(System.Text.Json.JsonSerializer.Serialize(new
                    {
                        Message = "Hello World!"
                    }), "helloworld.json", Encoding.UTF8, MediaTypeNames.Application.Json);

                    var message = new MailMessage("fromtest@test.com", "sendertest@test.com")
                    {
                        Subject = "Reset password email",
                        Body = $"<p>Click on this link to verify your account <a href=https://localhost:7175/ResetPassword?email={ProtectedEmail}&token={ProtectedToken} target=\"_blank\">https://localhost:7175/ResetPassword?email={ProtectedEmail}&token={ProtectedToken}</a></p>",
                        IsBodyHtml = true,
                    };

                    message.Attachments.Add(attachment);

                    try
                    {
                        smtpClient.Send(message);
                        TempData["SuccessMessage"] = "Password reset email sent successfully.";
                    }
                    catch (SmtpException ex)
                    {
                        TempData["ErrorMessage"] = $"Failed to send Password reset email: {ex.Message}";
                        Console.WriteLine(ex.ToString());
                    }

                    TempData["Message"] = "A Password reset email was sent to you.";
                    return RedirectToPage("");
                }
                else
                {
                    TempData["ErrorMessage"] = "Email address not found.";
                    return RedirectToPage();
                }
            }
            return Page();
        }
    }
}