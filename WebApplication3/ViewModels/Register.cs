using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using WebApplication3.Model;

namespace WebApplication3.ViewModels
{
    public class Register
    {

        [Required(ErrorMessage = "Your full name is required.")]
        [DataType(DataType.Text)]
        [RegularExpression(@"^(?=.*a-zA-Z)$", ErrorMessage = "Your full name can only include letters.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "A Credit Card Number is required.")]
        [DataType(DataType.CreditCard)]
        [RegularExpression(@"^(?=.*\d).{16}$", ErrorMessage = "Credit card number must consist of 16 digits.")]
        public string CreditCard { get; set; }

        [Required(ErrorMessage = "Please select your Gender")]
        [DataType(DataType.Text)]
        public string Gender { get; set; }

        [Required(ErrorMessage = "A phone number is required.")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^(?=.*\d)$", ErrorMessage = "Phone number can only include numbers.")]
        public string MobileNo { get; set; }

        [Required(ErrorMessage = "A Delivery Address is required.")]
        [DataType(DataType.Text)]
        [RegularExpression(@"^(?=.*\w)$", ErrorMessage = "Delivery address can only include letters and numbers.")]
        public string DeliveryAddress { get; set; }

        [Required(ErrorMessage = "Email address is required.")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,}$", ErrorMessage = "Please enter a valid email.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d<>]).{12,}$", ErrorMessage = "Password must be at least 12 characters long and contain at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Password and confirmation password does not match")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d<>]).{12,}$")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "A photo is required.")]
        [DataType(DataType.Upload)]
        [FileValidation(ErrorMessage = "Only .JPEG files are accepted")]
        public IFormFile? Photo { get; set; }


        [Required]
        [DataType(DataType.Text)]
        [RegularExpression(@"^(?=.*[^<>])$", ErrorMessage = "About me cannot include '<' and '>'.")]
        public string AboutMe { get; set; }
	}
}
