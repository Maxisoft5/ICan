using ICan.Common.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ICan.Common.Models.Opt
{
	public class SemiproductWarehouseModel
	{
		public int SemiproductWarehouseId { get; set; }

		[Display(Name = "Дата обновления")]
		public DateTime Date { get; set; }

		[Display(Name = "Действие")]
		public int WarehouseActionTypeId { get; set; }

		[Display(Name = "Действие")]
		public string WarehouseActionTypeName { get; set; }	
		
		[Display(Name = "Склад")]
		public int WarehouseTypeId { get; set; }

		[Display(Name = "Склад")]
		public string WarehouseTypeName { get; set; }	
	
		[Display(Name = "Комментарий")]
		public string Comment { get; set; }
		public IEnumerable<SemiproductWarehouseItemModel> SemiproductWarehouseItems { get; set; }
		public IEnumerable<SemiproductWarehouseFullModel> SemiproductWarehouseFullItems { get; set; }
		private IEnumerable<SemiproductWarehouseFullModel> SemiproductWarehouseItemsWithData =>
			SemiproductWarehouseFullItems?.Where(sProd =>
			sProd.BlockAmount.HasValue ||
			sProd.StickersAmount.HasValue ||
			sProd.CoversAmount.HasValue ||
			sProd.BoxFrontAmount.HasValue ||
			sProd.BoxBackAmount.HasValue ||
			sProd.CursorAmount.HasValue);

		public IEnumerable<SemiproductWarehouseFullModel> DetailsDisplayItems =>
			WarehouseActionTypeId == (int)WarehouseActionType.SingleInventory
			? SemiproductWarehouseItemsWithData
			: SemiproductWarehouseFullItems;

		public IEnumerable<SelectListItem> WarehouseTypes { get; set; }
	}
}
