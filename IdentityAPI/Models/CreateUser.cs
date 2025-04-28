namespace IdentityAPI.Models
{
	public class CreateUser
	{
		public string Email { get; set; }
		public string Password { get; set; }
		public string City { get; set; }
		public string Role { get; set; }
	}
}
