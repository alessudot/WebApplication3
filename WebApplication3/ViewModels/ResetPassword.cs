using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using WebApplication3.Model;

namespace WebApplication3.ViewModels
{
	public class ResetPassword
	{
		[Required(ErrorMessage = "Password is required.")]
		[DataType(DataType.Password)]
		[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d<>]).{12,}$", ErrorMessage = "Password must be at least 12 characters long and contain at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
		public string Password { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[Compare(nameof(Password), ErrorMessage = "Password and confirmation password does not match")]
		[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d<>]).{12,}$")]
		public string ConfirmPassword { get; set; }
	}
}
