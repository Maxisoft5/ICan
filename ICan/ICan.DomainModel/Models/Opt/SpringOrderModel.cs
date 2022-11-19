using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class SpringOrderModel
	{
		public int SpringOrderId { get; set; }
		[Required]
		[Display(Name = "Дата заказа")]
		public DateTime OrderDate { get; set; }
		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		[Display(Name = "Поставщик")]
		public string Provider { get; set; }
		[Display(Name = "Стоимость")]
		public decimal Cost { get; set; }
		[Display(Name = "Номер счёта")]
		public string InvoiceNumber { get; set; }
		[Display(Name = "Номер УПД")]
		public string UPDNumber { get; set; }
	
		[Required]
		[Range(1, Const.MaxSpoolValue, ErrorMessage = Const.ValidationMessages.MinMaxRangeExceeded)]
		[Display(Name = "Количество бобин")]
		public int SpoolCount { get; set; }
	
		[Display(Name = "Собран")]
		public bool IsAssembled { get; set; }
		
		[Required]
		[Display(Name = "Пружина")]
		public int SpringId { get; set; }
		[Display(Name = "Пружина")]
		public string SpringName => Spring?.SpringName;
		public SpringModel Spring { get; set; }
		public IEnumerable<SpringOrderIncomingModel> SpringOrderIncomings { get; set; }
		public IEnumerable<PaymentModel> Payments { get; set; }
	}
}
