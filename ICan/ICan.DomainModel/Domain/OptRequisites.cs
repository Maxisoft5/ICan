using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace ICan.Common.Domain
{
	public partial class OptRequisites
	{
		public OptRequisites()
		{
			OptOrders = new HashSet<OptOrder>();
		}

		public int RequisitesId { get; set; }

		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		[Display(Name = "Владалец карточки")]
		//  [StringLength(100, ErrorMessage  = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]

		public string Owner { get; set; }

		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		// [StringLength(100, ErrorMessage  = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]

		[Display(Name = "Реквизиты")]
		[StringLength(2000, MinimumLength = 6, ErrorMessage = Const.ValidationMessages.MinMaxLengthViolation)]
		[DataType(DataType.Html)]
		public string RequisitesText { get; set; }

		[Display(Name = "Последнее использование в заказе")]
		public DateTime? LastUsed { get; set; }

		[Display(Name = "Предназначено для")]
		public int ClientType { get; set; }

		[JsonIgnore]
		public ICollection<OptOrder> OptOrders { get; set; }
	}
}
