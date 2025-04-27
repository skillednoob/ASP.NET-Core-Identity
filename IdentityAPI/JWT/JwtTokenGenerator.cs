using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityAPI.JWT
{
	public class JwtTokenGenerator
	{
		private readonly IConfiguration _configuration;
		private readonly UserManager<ApplicationUser> _userManager;

		public JwtTokenGenerator(IConfiguration configuration,UserManager<ApplicationUser> userManager)
		{
			_configuration = configuration;
			_userManager = userManager;
		}

		public async Task<string> GenerateJwtToken(ApplicationUser user)
		{
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(ClaimTypes.Name, user.UserName),
				new Claim(ClaimTypes.Email, user.Email)
				// Add any other relevant claims
			};
			// Fetch roles from UserManager  TO GET HIS ROLE START
			var roles = await _userManager.GetRolesAsync(user);

			// Add each role as a claim
			foreach (var role in roles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role));
			}
			//END

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
			var expiry = DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["JWT:TokenLifetimeInMinutes"]));

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(claims),
				Expires = expiry,
				SigningCredentials = creds,
				Issuer = _configuration["JWT:ValidIssuer"],
				Audience = _configuration["JWT:ValidAudience"]
			};

			var tokenHandler = new JwtSecurityTokenHandler();
			var token = tokenHandler.CreateToken(tokenDescriptor);

			return tokenHandler.WriteToken(token);
		}
	}
}
