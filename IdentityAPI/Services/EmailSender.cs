using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;

namespace IdentityAPI.Services
{
	public class EmailSender:IdentityAPI.IServices.IEmailSender
	{
		private readonly SmtpSettings _smtpSettings;

		public EmailSender(IOptions<SmtpSettings> smtpSettings)
		{
			_smtpSettings = smtpSettings.Value;
		}

		public async Task SendEmailAsync(string email, string subject, string message)
		{
			try
			{
				using (var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port))
				{
					client.EnableSsl = _smtpSettings.EnableSsl;
					client.Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password);

					var mailMessage = new MailMessage
					{
						From = new MailAddress(_smtpSettings.Username),
						Subject = subject,
						Body = message,
						IsBodyHtml = true // Set to false for plain text emails
					};
					mailMessage.To.Add(email);

					await client.SendMailAsync(mailMessage);
				}
			}
			catch (Exception ex)
			{
			 
				Console.WriteLine($"Error sending email to {email}: {ex.Message}");
				 
				throw;
			}
		}
	}

	public class SmtpSettings
	{
		public string Host { get; set; }
		public int Port { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public bool EnableSsl { get; set; }
	}
}
