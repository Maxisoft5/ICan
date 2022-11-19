using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.AccountViewModels
{
	public class EmployeeModel
	{
		public string Id { get; set; }
		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		[Display(Name = "Имя")]
		public string FirstName { get; set; }

		[Display(Name = "Фамилия")]
		public string LastName { get; set; }

		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		[EmailAddress]
		[Display(Name = "Email")]
		public string Email { get; set; }

		[Phone]
		[Display(Name = "Мобильный телефон")]
		public string Phone { get; set; }

		[Display(Name = "Роль")]
		public List<RoleDescription> Roles { get; set; } = new List<RoleDescription>();

		public string[] CheckedRoles { get; set; } = new string[] { };

		[Display(Name = "Дата регистрации")]
		[DataType(DataType.Date)]
		public DateTime DateRegistration { get; set; }
	}
}
