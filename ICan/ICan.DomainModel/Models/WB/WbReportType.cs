using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.WB
{
	public enum WbReportType
	{
		None = 0,
		[Display(Name = "Заказы")]
		Orders = 1,
		[Display(Name = "Продажи")]
		Sales = 2
	}
}