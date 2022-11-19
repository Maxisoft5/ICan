using ICan.Common.Models.Enums;
using ICan.Common.Models.Opt;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace ICan.Common.Models.ManageViewModels
{
	public class ClientModel
	{
		[Display(Name = "Логин")]
		public string Username { get; set; }

		[Display(Name = "Имя")]
		public string FirstName { get; set; }
		[Display(Name = "Фамилия")]
		public string LastName { get; set; }
		[Display(Name = "Отчество")]
		public string PatronymicName { get; set; }

		public bool IsEmailConfirmed { get; set; }

		[Required(ErrorMessage =Const.ValidationMessages.RequiredFieldMessage)]
		[EmailAddress(ErrorMessage = Const.ValidationMessages.InvalidValue)]
		public string Email { get; set; }

		[Phone(ErrorMessage = Const.ValidationMessages.InvalidValue)]
		[Display(Name = "Телефон")]
		public string PhoneNumber { get; set; }

		public string StatusMessage { get; set; }

		public string Id { get; set; }

		[Display(Name = "Клиент")]
		public bool IsClient { get; set; }

		[Display(Name = "Фамилия и Имя")]
		public string FullName => $"{LastName} {FirstName}";

		[Display(Name = "Представляет магазин")]
		public IEnumerable<int?> ShopIds { get; set; }

		public IEnumerable<ApplicationUserShopRelationModel> ApplicationUserShopRelations { get; set; }

		[Display(Name = "Представляет магазин")]
		public string ShopName { get; set; }

		[Display(Name = "Тип клиента")]
		public ClientType ClientType { get; set; }

		[Display(Name = "Дата регистрации")]
		[DataType(DataType.Date)]
		public DateTime DateRegistration { get; set; }
	}
}
