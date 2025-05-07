using System.ComponentModel.DataAnnotations;

namespace IdentityAPI.Models
{
	public class ChangePassword
	{
		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Current Password")]
		public string CurrentPassword { get; set; }

		[Required]
		[DataType(DataType.Password)]
		public string NewPassword { get; set; }

		[Required]
		[Compare("NewPassword", ErrorMessage = "The new password and confirm password do not match")]
		public string ConfirmPassword { get; set; }
	}

}
