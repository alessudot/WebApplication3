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

        public RegisterModel(UserManager<ApplicationUser> userManager,
							 SignInManager<ApplicationUser> signInManager,
							 IWebHostEnvironment environment,
                             IHttpContextAccessor contxt,
                             RoleManager<IdentityRole> roleManager)
		{
			this.userManager = userManager;
			this.signInManager = signInManager;
			_environment = environment;
            this.contxt = contxt;
            this.roleManager = roleManager;
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

                var user = new ApplicationUser()
                {
                    FullName = protector.Protect(RModel.FullName),
                    CreditCard = protector.Protect(RModel.CreditCard),
                    Gender = protector.Protect(RModel.Gender),
                    MobileNo = protector.Protect(RModel.MobileNo),
                    DeliveryAddress = protector.Protect(RModel.DeliveryAddress),
                    AboutMe = protector.Protect(RModel.AboutMe),
                    UserName = RModel.Email,
                    Email = RModel.Email,
                };

                IdentityRole AdminRole = await roleManager.FindByIdAsync("Admin");
                if (AdminRole == null)
                {
                    IdentityResult result2 = await roleManager.CreateAsync(new IdentityRole("Admin"));
                    if (!result2.Succeeded)
                    {
                        ModelState.AddModelError("", "Create role admin failed");
                    }
                }

                IdentityRole UserRole = await roleManager.FindByIdAsync("User");
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

                        result = await userManager.AddToRoleAsync(user, "User");
                        await signInManager.SignInAsync(user, false);
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

    }
}
