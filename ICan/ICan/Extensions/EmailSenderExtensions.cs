using System.Text.Encodings.Web;

namespace ICan.Business.Services
{
	public static class EmailSenderExtensions
	{
		public static void SendEmailConfirmation(this IEmailSender emailSender, string email, string link)
		{
			emailSender.SendEmail(email, "Подтверждение почты",
			   $"Пожалуйста, завершите регистрацию, перейдя по <a href='{HtmlEncoder.Default.Encode(link)}'>ссылке</a>");
		}
	}
}

