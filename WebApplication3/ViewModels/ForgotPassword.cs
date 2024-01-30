using System.ComponentModel.DataAnnotations;

namespace WebApplication3.ViewModels
{
    public class ForgotPassword
    {
        [Required(ErrorMessage = "Please enter your email ")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,}$", ErrorMessage = "Please enter a valid email.")]
        public string Email { get; set; }
    }
}