namespace IdentityAPI.Models
{
	public class AuthResult
	{
		public string AccessToken { get; set; }
		public string RefreshToken { get; set; }
		public DateTime AccessTokenExpiry { get; set; }
		public DateTime RefreshTokenExpiry { get; set; }
	}
}
