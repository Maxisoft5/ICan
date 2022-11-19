using ICan.Common.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class PaperOrderIncomingModel
	{
		public int PaperOrderIncomingId { get; set; }
		public int PaperOrderId { get; set; }
		[DisplayName("Кол-во")]
		[Range(0, int.MaxValue)]
		public int Amount { get; set; }
		[DisplayName("Дата прихода")]
		public DateTime IncomingDate { get; set; }
		[Required]
		public WarehouseType WarehouseTypeId { get; set; }
		public string WarehouseTypeName { get; set; }
		[DisplayName("Склад")]
		public IEnumerable<SelectListItem> WarehouseTypes { get; set; }
	}
}
