using ICan.Common.Domain;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ICan.Common.Models.Opt
{
	public class CalcDetails
	{
		[Display(Name = "Инвентаризация")]
		public int Inventory { get; set; }

		[Display(Name = "Приход")]
		public int Arrived { get; set; }

		[Display(Name = "Участвует в сборках комплектов")]
		public int KitAssembly { get; set; }

		[Display(Name = "Отгрузка по УПД")]
		public int UPD { get; set; }

		[Display(Name = "Маркетинг")]
		public int Marketing { get; set; }

		[Display(Name = "Возврат")]
		public int Return { get; set; }

		[Display(Name = "Отгрузка через оптовый модуль")]
		public int OrderProduct { get; set; }

		public int ProductId { get; set; }

		[Display(Name = "Название")]
		public string Name { get; set; }
		[Display(Name = "Результат")]
		public int Current =>
				  Inventory + Arrived - UPD - Marketing + Return - OrderProduct - KitAssembly;

		public IEnumerable<OptReport> UPDItems { get; set; }
		public IOrderedEnumerable<OptOrderproduct> OrderItems { get; set; }
	}
}