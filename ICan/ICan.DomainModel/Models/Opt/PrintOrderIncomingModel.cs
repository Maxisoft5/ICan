using ICan.Common.Models.Enums;
using ICan.Common.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class PrintOrderIncomingModel
	{
		public long PrintOrderIncomingId { get; set; }
		public int PrintOrderId { get; set; }

		[Display(Name = "Тип прихода")]
		public PrintOrderIncomingType IncomingType { get; set; }
		public string IncomingTypeName => IncomingType.GetDisplayName();

		[Display(Name = "Комментарий")]
		public string Comment { get; set; }

		[DataType(DataType.DateTime)]
		[Display(Name = "Дата прихода")]
		public DateTime IncomingDate { get; set; }

		public List<PrintOrderIncomingItemModel> PrintOrderIncomingItems { get; set; }
	}
}
