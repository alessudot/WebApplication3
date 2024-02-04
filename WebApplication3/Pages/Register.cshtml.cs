using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NanoidDotNet;
using WebApplication3.Model;
using WebApplication3.ViewModels;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Text.Encodings.Web;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Net.Mime;
using System.Net;
using System.Text.Json;
using System.Text;
using static System.Net.WebRequestMethods;


namespace WebApplication3.Pages
{
    public class RegisterModel : PageModel

    {

        [BindProperty]
        public Register RModel { get; set; }

		private readonly IWebHostEnvironment _environment;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IHttpContextAccessor contxt;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly HtmlEncoder _htmlEncoder;


        public RegisterModel(UserManager<ApplicationUser> userManager,
							 SignInManager<ApplicationUser> signInManager,
							 IWebHostEnvironment environment,
                             IHttpContextAccessor contxt,
                             RoleManager<IdentityRole> roleManager,
                             HtmlEncoder htmlEncoder)
		{
			this.userManager = userManager;
			this.signInManager = signInManager;
			_environment = environment;
            this.contxt = contxt;
            this.roleManager = roleManager;
            _htmlEncoder = htmlEncoder;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync(string recaptchaToken)

        {
            if (ModelState.IsValid)
            {
                var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
                var protector = dataProtectionProvider.CreateProtector("MySecretKey");

                HttpClient httpClient = new HttpClient();
                var response = await httpClient.GetAsync($"https://www.google.com/recaptcha/api/siteverify?secret=6LfD614pAAAAAIoEpA0PDfdwj0LkbP8t0K1n6CE2&response={recaptchaToken}");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                var recaptchaResult = JsonConvert.DeserializeObject<RecaptchaResponse>(responseBody);

                if (!recaptchaResult.Success)
                {
                    TempData["ErrorMessage"] = "reCAPTCHA verification failed.";
                    return RedirectToPage();
                }

                var user = new ApplicationUser()
                {
                    FullName = _htmlEncoder.Encode(protector.Protect(RModel.FullName)),
                    CreditCard = _htmlEncoder.Encode(protector.Protect(RModel.CreditCard)),
                    Gender = _htmlEncoder.Encode(protector.Protect(RModel.Gender)),
                    MobileNo = _htmlEncoder.Encode(protector.Protect(RModel.MobileNo)),
                    DeliveryAddress = _htmlEncoder.Encode(protector.Protect(RModel.DeliveryAddress)),
                    AboutMe = _htmlEncoder.Encode(protector.Protect(RModel.AboutMe)),
                    PreviousPasswords = _htmlEncoder.Encode(protector.Protect(RModel.Password) + "|" + protector.Protect("")),
                    LastPasswordChange = DateTime.Now,
                    UserName = RModel.Email,
                    Email = RModel.Email,
                };

                IdentityRole AdminRole = await roleManager.FindByNameAsync("Admin");
                if (AdminRole == null)
                {
                    IdentityResult result2 = await roleManager.CreateAsync(new IdentityRole("Admin"));
                    if (!result2.Succeeded)
                    {
                        ModelState.AddModelError("", "Create role admin failed");
                    }
                }

                IdentityRole UserRole = await roleManager.FindByNameAsync("User");
                if (UserRole == null)
                {
                    IdentityResult result2 = await roleManager.CreateAsync(new IdentityRole("User"));
                    if (!result2.Succeeded)
                    {
                        ModelState.AddModelError("", "Create role user failed");
                    }
                }



                if (RModel.Photo == null || RModel.Photo.Length == 0)
                {
                    ModelState.AddModelError("", "Please select a photo.");
                    return Page();
                }

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                if (RModel.Photo != null)
                {
                    var id = Nanoid.Generate(size: 10);
                    var filename = id + Path.GetExtension(RModel.Photo.FileName);
                    var filePath = Path.Combine(uploadsFolder, filename);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    RModel.Photo.CopyTo(fileStream);
                    user.PhotoPath = "/uploads/" + filename;
                }

				var result = await userManager.CreateAsync(user, RModel.Password);
                var currentUser = await userManager.FindByEmailAsync(RModel.Email);

                if (result.Succeeded)
                {
                    if (currentUser != null)
                    {
                        await userManager.AddToRoleAsync(user, "User");

                        var ProtectedEmail = protector.Protect(RModel.Email);
                        var token = await userManager.GenerateEmailConfirmationTokenAsync(currentUser);
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
                            Subject = "Verification email",
                            Body = $"<p>Click on this link to verify your account <a href=https://localhost:7175/Verification?email={ProtectedEmail}&token={ProtectedToken} target=\"_blank\">https://localhost:7175/Verification?email={ProtectedEmail}&token={ProtectedToken}</a></p>",
                            IsBodyHtml = true,
                        };

                        message.Attachments.Add(attachment);

                        try
                        {
                            smtpClient.Send(message);
                            TempData["SuccessMessage"] = "Verification email sent successfully.";
                        }
                        catch (SmtpException ex)
                        {
                            TempData["ErrorMessage"] = $"Failed to send verification email: {ex.Message}";
                            Console.WriteLine(ex.ToString());
                        }

                        TempData["Message"] = "Please verify your email address. A verification email was sent to you.";
                        return RedirectToPage("Login");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Unable to retrieve user information.";
                        return RedirectToPage();
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return Page();
        }

        public class RecaptchaResponse
        {
            public bool Success { get; set; }
        }

    }
}
