using SendGrid.Helpers.Mail;
using SendGrid;
using IdentityAPI.IServices;

namespace IdentityAPI.Services
{
	public class SendGridEmailService: ISendGridEmailService
	{
		private readonly IConfiguration _config;

		public SendGridEmailService(IConfiguration config)
		{
			_config = config;
		}

		public async Task SendEmailAsync(string toEmail, string subject, string bodyHtml)
		{
			var apiKey = _config["SendGrid:ApiKey"];
			var client = new SendGridClient(apiKey);

			var from = new EmailAddress(_config["SendGrid:SenderEmail"], _config["SendGrid:SenderName"]);
			var to = new EmailAddress(toEmail);

			var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent: null, htmlContent: bodyHtml);

			var response = await client.SendEmailAsync(msg);

			if ((int)response.StatusCode >= 400)
			{
				throw new Exception($"SendGrid Error: {response.StatusCode}");
			}
		}
	}
}
