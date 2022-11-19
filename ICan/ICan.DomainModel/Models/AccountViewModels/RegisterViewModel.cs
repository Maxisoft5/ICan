using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.AccountViewModels
{
	public class RegisterViewModel
	{
		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		[Display(Name = "Имя")]
		public string FirstName { get; set; }

		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		[Display(Name = "Фамилия")]
		public string LastName { get; set; }
		
		[Phone(ErrorMessage = Const.ValidationMessages.InvalidValue)]
		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		[Display(Name = "Мобильный телефон")]
		public string Phone { get; set; }

		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		[EmailAddress]
		[Display(Name = "Email")]
		public string Email { get; set; }

		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		[StringLength(15, ErrorMessage = "{0} должен быть не меньше {2} и не больше {1} символов в длину",
			MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "Пароль")]
		public string Password { get; set; }

		[DataType(DataType.Password)]
		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		[Display(Name = "Подтверждение пароля")]
		[Compare("Password", ErrorMessage = "Пароль и подтверждение пароля должны совпадать")]
		[StringLength(15, ErrorMessage = "{0} должен быть не меньше {2} и не больше {1} символов в длину")]
		public string ConfirmPassword { get; set; }
	}
}
