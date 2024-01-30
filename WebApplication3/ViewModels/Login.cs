using System.ComponentModel.DataAnnotations;

namespace WebApplication3.ViewModels
{
	public class Login
	{
		[Required(ErrorMessage = "Please enter your email ")]
		[DataType(DataType.EmailAddress)]
		[RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,}$", ErrorMessage = "Please enter a valid email.")]
		public string Email { get; set; }
		[Required(ErrorMessage = "Please enter your password ")]
		[DataType(DataType.Password)]
		[RegularExpression(@"^[^<>]*$")]
		public string Password { get; set; }
		public bool RememberMe { get; set; }
	}
}
