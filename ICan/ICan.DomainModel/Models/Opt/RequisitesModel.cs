using ICan.Common.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class RequisitesModel
	{
		public int RequisitesId { get; set; }

		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		[Display(Name = "Владалец карточки")]

		public string Owner { get; set; }

		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		[Display(Name = "Реквизиты")]
		[StringLength(2000, MinimumLength = 6, ErrorMessage = Const.ValidationMessages.MinMaxLengthViolation)]
		[DataType(DataType.Html)]
		public string RequisitesText { get; set; }

		[Display(Name = "Последнее использование в заказе")]
		public DateTime? LastUsed { get; set; }

		[Display(Name = "Предназначено для")]
		public ClientType ClientType { get; set; }

	}
}
