using System;
using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class UpdPaymentModel
	{
		public int UpdPaymentId { get; set; }

		public int ShopId { get; set; }

		[Display(Name = "Дата подтверждения")]
		[DataType(DataType.Date)]
		public DateTime Date { get; set; }

		[Display(Name = "Дата УПД")]
		[DataType(DataType.Date)]
		public DateTime ReportDate { get; set; }
		
		[Display(Name = "Номер УПД")]
		public string UpdNumber { get; set; }

		public string Comment { get; set; }

		[Display(Name="Магазин")]
		public string ShopName { get; set; }
		public decimal UpdTotalSum { get; set; }

		[Display(Name = "Нет УПД")]
		public bool IsUnbound { get; set; } = false;
	}
}
