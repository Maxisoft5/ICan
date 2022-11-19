using ICan.Common.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.AccountViewModels
{
	public class ManualRegisterViewModel
	{
		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		[Display(Name = "Имя")]
		public string FirstName { get; set; }

		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		[Display(Name = "Фамилия")]
		public string LastName { get; set; }

		[Phone(ErrorMessage= Const.ValidationMessages.InvalidValue)]
		[Display(Name = "Мобильный телефон")]
		public string Phone { get; set; }

		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		[EmailAddress]
		[Display(Name = "Email")]
		public string Email { get; set; }

		[Display(Name = "Тип клиента")]
		public ClientType ClientType { get; set; }
	}
}
