using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.AccountViewModels
{
	public class ResetPasswordViewModel
	{
		//  [Required]
		[EmailAddress]
		public string Email { get; set; }

		[Required]
		//[EmailAddress]
		public string UserId { get; set; }

		[Required]
		[StringLength(100, ErrorMessage = "Пароль {0} должен быть не меньше {2}  и не больше {1} символов в дину.",
			MinimumLength = 6)]
		[Display(Name = "Пароль")]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Подтверждение пароля")]
		[Compare("Password", ErrorMessage = "Пароль и подтверждение пароля должны совпадать.")]
		public string ConfirmPassword { get; set; }

		public string Code { get; set; }
	}
}
