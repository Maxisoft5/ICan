using ICan.Common.Models.Enums;
using ICan.Common.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ICan.Common.Models.Opt
{
	public class AssemblyModel
	{
		public int AssemblyId { get; set; }

		[DataType(DataType.DateTime)]
		[Display(Name = "Дата")]
		[Required]
		public DateTime Date { get; set; }

		[Display(Name = "Продукт")]
		[Required]
		public int ProductId { get; set; }

		[Display(Name = "Количество")]
		[Range(1, int.MaxValue, ErrorMessage = Const.ValidationMessages.PositiveFieldMessage)]
		public int Amount { get; set; }

		[Display(Name = "Продукт")]
		public string ProductDisplayName => Product?.DisplayName;

		public ProductModel Product { get; set; }

		[Display(Name = "Дата приёма на склад")]
		[DataType(DataType.Date)]
		public DateTime? WarehouseDateAdd { get; set; }

		[Display(Name = "Тип сборки")]
		public AssemblyType AssemblyType { get; set; }

		[Display(Name = "Тип сборки")]
		public string AssemblyTypeName => AssemblyType.GetDisplayName();
		public int? WarehouseId { get; set; }
		public bool CanEdit => !WarehouseId.HasValue;

		public List<AssemblySemiproductModel> AssemblySemiproducts { get; set; }

		public bool ConsiderNotch => AssemblySemiproducts != null && AssemblySemiproducts.Any(asmr => asmr.NotchOrderId.HasValue);

		public Dictionary<SemiproductModel, IEnumerable<KeyValuePair<int, string>>> SemiproductsWithPrintOrders { get; set; }
		public int ProductSeriesId { get; set; }
	}
}
