using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.ManageViewModels
{
	public class ChangePasswordViewModel
	{
		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Старый пароль")]
		public string OldPassword { get; set; }

		[Required]
		[StringLength(100, ErrorMessage = "Пароль должен быть как мининимум {0} символов длиной.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "Новый пароль")]
		public string NewPassword { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Подтвердите новый пароль")]
		[Compare("NewPassword", ErrorMessage = "Пароль и подтвердждение не совпадают")]
		public string ConfirmPassword { get; set; }

		public string StatusMessage { get; set; }
	}
}