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
            //var smtpClient = new SmtpClient("smtp.ethereal.email")
            //{
            //    Port = 587,
            //    Credentials = new NetworkCredential("rose.swift@ethereal.email", "UN1wPX1jXm2Vw1bEGe"),
            //    EnableSsl = true,
            //};

            //var attachment = Attachment.CreateAttachmentFromString(JsonSerializer.Serialize(new
            //{
            //    Message = "Hello World!"
            //}), "helloworld.json", Encoding.UTF8, MediaTypeNames.Application.Json);

            //var message = new MailMessage("fromtest@test.com", "sendertest@test.com")
            //{
            //    Subject = "Test Email! Hello World!",
            //    Body = "<p>Test Email</p><b>Hello World!</b>",
            //    IsBodyHtml = true,
            //};

            //message.Attachments.Add(attachment);

            //try
            //{
            //    smtpClient.Send(message);
            //    TempData["SuccessMessage"] = "Test email sent successfully.";
            //}
            //catch (SmtpException ex)
            //{
            //    TempData["ErrorMessage"] = $"Failed to send test email: {ex.Message}";
            //    Console.WriteLine(ex.ToString());
            //}
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