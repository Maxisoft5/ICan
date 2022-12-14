using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.AccountViewModels
{
	public class ForgotPasswordViewModel
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; }
	}
}
