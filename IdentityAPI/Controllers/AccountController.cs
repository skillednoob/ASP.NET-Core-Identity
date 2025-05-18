using IdentityAPI.IServices;
using IdentityAPI.JWT;
using IdentityAPI.Models;
using IdentityAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace IdentityAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	//[Authorize]
	public class AccountController : ControllerBase
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly JwtTokenGenerator _jwtTokenGenerator; // Inject the token generator
		private readonly ILogger<AccountController> _logger;
		private readonly IEmailSender _emailSender;
		private readonly ISendGridEmailService _emailService;
		private readonly ApplicationDbContext _context;

		public AccountController(UserManager<ApplicationUser> userManager, JwtTokenGenerator jwtTokenGenerator, ILogger<AccountController> logger,IEmailSender emailSender, ISendGridEmailService emailService, ApplicationDbContext context)
		{
			_userManager = userManager;
			_jwtTokenGenerator = jwtTokenGenerator;
			_logger = logger;
			_emailSender = emailSender;
			_emailService = emailService;
			_context = context;
		}


		[HttpPost("register")]
		[AllowAnonymous]
		public async Task<IActionResult> Register([FromBody] Register model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var user = new ApplicationUser { UserName = model.Email, Email = model.Email,City=model.City };
			var result = await _userManager.CreateAsync(user, model.Password);

			if (result.Succeeded)
			{
				// Optionally, you can assign roles to the user here
				// await _userManager.AddToRoleAsync(user, "RegularUser");

				// Generate JWT token using the injected service
				//var token = _jwtTokenGenerator.GenerateJwtToken(user);
				//return Ok(new { Token = token });
				// ✅ Generate email confirmation token
				var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

				// ✅ Build confirmation URL
				var confirmationLink = Url.Action(
					nameof(ConfirmEmail),              // action
					"Account",                            // controller name
					new { userId = user.Id, token = token },
					Request.Scheme);                   // generate full URL with http/https

				// ✅ Send the email - replace with real email sending Simulate sending email (in real app, send via SMTP/sendgrid/etc)
				//Console.WriteLine($"Confirm your email using this link: {confirmationLink}");

				//return Ok(new { message = "User registered successfully. Please confirm your email." });
				var subject = "Confirm your email";
				var message = $"Please confirm your email address by clicking the following link: <a href='{confirmationLink}'>Confirm Email</a>";

				// ✅ Use the injected email sender service to send the email
				await _emailSender.SendEmailAsync(user.Email, subject, message);

				return Ok(new { message = "User registered successfully. Please check your email to confirm your account." });
			}

			return BadRequest(result.Errors);
		}

		[HttpGet("confirmemail")]
		[AllowAnonymous]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<IActionResult> ConfirmEmail(string userId, string token)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
				return BadRequest("Invalid User ID");

			var result = await _userManager.ConfirmEmailAsync(user, token);
			if (result.Succeeded)
				return Ok("Email confirmed successfully!");

			return BadRequest("Email confirmation failed");
		}


		[HttpPost("Login")]
		[AllowAnonymous]
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

			// 🔐 Enforce email confirmation
			if (!user.EmailConfirmed)
			{
				return Unauthorized(new { message = "Email not confirmed." });
			}

			var isPasswordValid=await _userManager.CheckPasswordAsync(user, model.Password);
			if (!isPasswordValid)
			{
				return Unauthorized(new { message = "wrong password" });
			}
			var token=_jwtTokenGenerator.GenerateJwtToken(user);
			return Ok(new { Token = token });
		}


		[HttpPost("resend-confirmation")]
		[AllowAnonymous]
		public async Task<IActionResult> ResendConfirmationEmail([FromBody] string email)
		{
			var user = await _userManager.FindByEmailAsync(email);
			if (user == null || user.EmailConfirmed)
				return BadRequest("Invalid request.");

			var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

			// Encode the token for safe URL use
			var encodedToken = System.Web.HttpUtility.UrlEncode(token);

			// Manually build the confirmation link
			var confirmationLink = $"{Request.Scheme}://{Request.Host}/api/account/confirmemail?userId={user.Id}&token={encodedToken}";

			_logger.LogWarning(confirmationLink); // Log for debugging

			//SENDGRID
			var emailBody = $"<p>Hi {email},</p><p>Please verify your email by clicking <a href='{confirmationLink}'>this link</a>.</p>";

			await _emailService.SendEmailAsync(user.Email, "Confirm your email", emailBody);

			return Ok("Resend link has been sent  successful. Please check your email to verify your account.");

			return Ok(new { message = "Confirmation link resent.", link = confirmationLink });
		}

		[HttpPost("Forgot-Password")]
		[AllowAnonymous]
		public async Task<IActionResult> ForgotPassword(ForgotPassword model)
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.FindByEmailAsync(model.Email);
				if(user!=null && await _userManager.IsEmailConfirmedAsync(user))
				{
					var token = await _userManager.GeneratePasswordResetTokenAsync(user);

					var PasswordResetLink= $"{Request.Scheme}://{Request.Host}/api/account/ResetPassword?userId={user.Id}&token={token}";  

					_logger.LogWarning(PasswordResetLink);

					return Ok(new { message = "PasswordReset link resent.", link = PasswordResetLink });

				}
				return Ok(new { message = "Invalid emailId or Email not confirmed" });
			}
			return BadRequest(ModelState);
		}

		[HttpPost("ResetPassword")]
		[AllowAnonymous]
		public async Task<IActionResult> ResetPassword([FromBody] ResetPassword model)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user == null)
				return Ok(new { message = "Password reset successful." }); // Security: Don't reveal user existence

			var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

			if (result.Succeeded)
				return Ok(new { message = "Password reset successful!" });

			// Handle errors
			foreach (var error in result.Errors)
				ModelState.AddModelError(string.Empty, error.Description);

			return BadRequest(ModelState);
		}

		[HttpPost("ChangePassword")]
		public async Task<IActionResult> ChangePassword(ChangePassword model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return Unauthorized(new { message = "Invalid user." });
			}

			var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
			if (result.Succeeded)
			{
				return Ok(new { message = "Password changed successfully." });
			}

			var errors = result.Errors.Select(e => e.Description);
			return BadRequest(new { errors = errors });
		}


		[HttpPost("refresh")]
		[AllowAnonymous]
		public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
		{
			if (string.IsNullOrEmpty(request.RefreshToken))
				return BadRequest(new { message = "Refresh token is required" });

			// Get the token from DB
			var storedToken = await _context.RefreshTokens
				.Include(rt => rt.User)
				.FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

			if (storedToken == null || storedToken.IsUsed || storedToken.IsRevoked)
				return Unauthorized(new { message = "Invalid refresh token" });

			if (storedToken.Expires < DateTime.UtcNow)
				return Unauthorized(new { message = "Refresh token expired" });

			// ✅ Mark old token as used
			storedToken.IsUsed = true;
			storedToken.IsRevoked = true;
			_context.RefreshTokens.Update(storedToken);
			await _context.SaveChangesAsync();

			// ✅ Generate new tokens
			var authResponse = await _jwtTokenGenerator.GenerateJwtToken(storedToken.User);

			return Ok(authResponse);
		}


	}
}
