using System.Collections.Generic;

namespace ICan.Business.Services
{
	public interface IEmailSender
	{
		void SendEmail(string email, string subject, string message, IEnumerable<EmailAttachment> attachments = null);
	}
}
