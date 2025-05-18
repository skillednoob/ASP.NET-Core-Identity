using IdentityAPI.Models;
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
		private readonly ApplicationDbContext _context;

		public JwtTokenGenerator(IConfiguration configuration,UserManager<ApplicationUser> userManager, ApplicationDbContext context)
		{
			_configuration = configuration;
			_userManager = userManager;
			_context = context;
		}

		//public async Task<string> GenerateJwtToken(ApplicationUser user)
		//{
		//	var claims = new List<Claim>
		//	{
		//		new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
		//		new Claim(ClaimTypes.Name, user.UserName),
		//		new Claim(ClaimTypes.Email, user.Email)
		//		// Add any other relevant claims
		//	};
		//	// Fetch roles from UserManager  TO GET HIS ROLE START
		//	var roles = await _userManager.GetRolesAsync(user);

		//	// Add each role as a claim
		//	foreach (var role in roles)
		//	{
		//		claims.Add(new Claim(ClaimTypes.Role, role));
		//	}
		//	//END

		//	// 2. Get all user claims from AspNetUserClaims table
		//	var userClaims = await _userManager.GetClaimsAsync(user);  // for customer claims
		//	claims.AddRange(userClaims);  // Add all custom claims to the token

		//	var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
		//	var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
		//	var expiry = DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["JWT:TokenLifetimeInMinutes"]));

		//	var tokenDescriptor = new SecurityTokenDescriptor
		//	{
		//		Subject = new ClaimsIdentity(claims),
		//		Expires = expiry,
		//		SigningCredentials = creds,
		//		Issuer = _configuration["JWT:ValidIssuer"],
		//		Audience = _configuration["JWT:ValidAudience"]
		//	};

		//	var tokenHandler = new JwtSecurityTokenHandler();
		//	var token = tokenHandler.CreateToken(tokenDescriptor);

		//	return tokenHandler.WriteToken(token);
		//}

		public async Task<AuthResult> GenerateJwtToken(ApplicationUser user, int refreshTokenExpiryMinutes = 43200)
		{
			var claims = new List<Claim>
	{
		new Claim(ClaimTypes.NameIdentifier, user.Id),
		new Claim(ClaimTypes.Name, user.UserName),
		new Claim(ClaimTypes.Email, user.Email)
	};

			var roles = await _userManager.GetRolesAsync(user);
			foreach (var role in roles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role));
			}

			var userClaims = await _userManager.GetClaimsAsync(user);
			claims.AddRange(userClaims);

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
			var accessTokenExpiry = DateTime.UtcNow.AddMinutes(20); // short expiry
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(claims),
				Expires = accessTokenExpiry,
				SigningCredentials = creds,
				Issuer = _configuration["JWT:ValidIssuer"],
				Audience = _configuration["JWT:ValidAudience"]
			};

			var tokenHandler = new JwtSecurityTokenHandler();
			var token = tokenHandler.CreateToken(tokenDescriptor);
			var accessToken = tokenHandler.WriteToken(token);

			// ✅ Generate refresh token
			var refreshToken = new RefreshToken
			{
				Token = Guid.NewGuid().ToString(),
				UserId = user.Id,
				Created = DateTime.UtcNow,
				Expires = DateTime.UtcNow.AddMinutes(refreshTokenExpiryMinutes)
			};

			// ✅ Save refresh token to DB
			_context.RefreshTokens.Add(refreshToken);
			await _context.SaveChangesAsync();

			return new AuthResult
			{
				AccessToken = accessToken,
				AccessTokenExpiry = accessTokenExpiry,
				RefreshToken = refreshToken.Token,
				RefreshTokenExpiry = refreshToken.Expires
			};
		}

	}
}
