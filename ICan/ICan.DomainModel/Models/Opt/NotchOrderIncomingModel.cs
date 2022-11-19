using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class NotchOrderIncomingModel
	{
		public int NotchOrderIncomingId { get; set; }
		[Display(Name = "Дата прихода")]
		public DateTime IncomingDate { get; set; }
		public int NotchOrderId { get; set; }
		public List<NotchOrderIncomingItemModel> IncomingItems { get; set; } = new List<NotchOrderIncomingItemModel>();
		public NotchOrderModel NotchOrder { get; set; }
	}
}
