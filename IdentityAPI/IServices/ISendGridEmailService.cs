namespace IdentityAPI.IServices
{
	public interface ISendGridEmailService
	{
		public Task SendEmailAsync(string toEmail, string subject, string bodyHtml);
	}
}
