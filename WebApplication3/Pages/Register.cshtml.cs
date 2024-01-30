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

        //public async Task<IActionResult> OnPostAsync()
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
                    ModelState.AddModelError("", "reCAPTCHA verification failed.");
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
                        contxt.HttpContext.Session.SetString("Email", protector.Protect(RModel.Email));
                        contxt.HttpContext.Session.SetString("Password", protector.Protect(RModel.Password));
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
                            Secure = true,
                            Expires = DateTimeOffset.UtcNow.AddDays(3)
                        });

                        result = await userManager.AddToRoleAsync(user, "User");
                        await signInManager.SignInAsync(user, true);
                        return RedirectToPage("Index");
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
