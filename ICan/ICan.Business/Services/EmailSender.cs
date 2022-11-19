using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace ICan.Business.Services
{
	// This class is used by the application to send email for account confirmation and password reset.
	// For more details see https://go.microsoft.com/fwlink/?LinkID=532713
	public class EmailSender : IEmailSender
	{
		private readonly ILogger _logger;

		private string Host { get; }
		private string NoReply { get; }
		private string Pass { get; }

		private readonly IConfiguration _configuration;
		private readonly int Port;
		private readonly bool EnableSsl;

		public EmailSender(IConfiguration configuration,

			ILogger<EmailSender> logger)
		{
			_configuration = configuration;

			_logger = logger;
			Host = _configuration["Settings:Mail:Host"];
			NoReply = _configuration["Settings:Mail:NoReply"];
			Pass = _configuration["Settings:Mail:Pass"];
			int.TryParse(_configuration["Settings:Mail:Port"] ?? "", out Port);
			bool.TryParse(_configuration["Settings:Mail:EnableSsl"] ?? "", out EnableSsl);
		}

		public void SendEmail(string email, string subject, string message, IEnumerable<EmailAttachment> attachments)
		{
			if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(message))
				return;
			var client = new SmtpClient(Host)
			{
				UseDefaultCredentials = false,
				Credentials = new NetworkCredential(NoReply, Pass)
			};
			client.Port = Port;
			client.EnableSsl = EnableSsl;

			var mailMessage = new MailMessage
			{
				From = new MailAddress(NoReply)
			};
			mailMessage.To.Add(email);
			mailMessage.Subject = subject;
			mailMessage.Body = message;
			mailMessage.IsBodyHtml = true;
			List<MemoryStream> streams = new List<MemoryStream>();
			if (attachments != null && attachments.Any())
			{
				attachments.ToList().ForEach(attachment =>
				{
					MemoryStream m = new MemoryStream(attachment.Data);
					mailMessage.Attachments.Add(new System.Net.Mail.Attachment(m, attachment.Name));
					streams.Add(m);
				});
			}
			try
			{
				client.Send(mailMessage);
				_logger.LogWarning($"message to {email} about \"{subject}\"  with body {message} is sent");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex , $"Не удалось отправить письмо {email}");
			}
			finally
			{
				foreach (MemoryStream stream in streams)
				{
					stream.Dispose();
				}
			}
		}
	}
}
