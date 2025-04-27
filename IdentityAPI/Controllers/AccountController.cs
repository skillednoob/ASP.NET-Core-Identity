using IdentityAPI.JWT;
using IdentityAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AccountController : ControllerBase
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly JwtTokenGenerator _jwtTokenGenerator; // Inject the token generator

		public AccountController(UserManager<ApplicationUser> userManager, JwtTokenGenerator jwtTokenGenerator)
        {
				_userManager = userManager;
			_jwtTokenGenerator = jwtTokenGenerator;
        }


		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] Register model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
			var result = await _userManager.CreateAsync(user, model.Password);

			if (result.Succeeded)
			{
				// Optionally, you can assign roles to the user here
				// await _userManager.AddToRoleAsync(user, "RegularUser");

				// Generate JWT token using the injected service
				var token = _jwtTokenGenerator.GenerateJwtToken(user);
				return Ok(new { Token = token });
			}

			return BadRequest(result.Errors);
		}

		[HttpPost("Login")]
		public async Task<IActionResult> Login([FromBody] Login model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var user=await _userManager.FindByEmailAsync(model.Email);
			if (user == null)
			{
				return Unauthorized(new { message = "user does not exsist" });
			}
			var isPasswordValid=await _userManager.CheckPasswordAsync(user, model.Password);
			if (!isPasswordValid)
			{
				return Unauthorized(new { message = "wrong password" });
			}
			var token=_jwtTokenGenerator.GenerateJwtToken(user);
			return Ok(new { Token = token });
		}

	}
}
