using System.ComponentModel.DataAnnotations;

namespace IdentityAPI.Models
{
	public class ForgotPassword
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; }
	}
}
