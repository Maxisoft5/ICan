using System;
using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class PrintOrderPaymentModel
	{
		public int PrintOrderPaymentId { get; set; }
		
		public int PrintOrderId { get; set; }
		
		[Display(Name ="Дата платежа")]
		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		[DataType(DataType.Date)]
		public DateTime Date { get; set; }
		public string DisplayDate => Date.ToString("dd.MM.yyyy");

		[Display(Name = "Сумма")]
		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		public decimal Amount { get; set; }
	
		public string DisplayAmount => Amount.ToString("N2");
	}
}
