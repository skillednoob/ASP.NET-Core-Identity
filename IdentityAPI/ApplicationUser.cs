using Microsoft.AspNetCore.Identity;

namespace IdentityAPI
{
	public class ApplicationUser : IdentityUser
	{
		public string City { get; set; }
	}
}
