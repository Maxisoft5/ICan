using ICan.Common.Domain;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class WarehouseCreateModel
	{
		public Dictionary<OptProductseries, List<ProductModel>> Products { get; set; }

		[Display(Name = "Действие")]
		public int WarehouseActionTypeId { get; set; }

		[Display(Name = "Дата обновления")]
		[DataType(DataType.DateTime)]
		public DateTime DateAdd { get; set; }

		[Display(Name = "Действие")]
		public IEnumerable<SelectListItem> WarehouseActionTypes { get; set; }
	}
}
