using ICan.Common.Domain;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace ICan.Common.Models.Opt
{
	public class ProductListModel
	{
		public Dictionary<OptProductseries, List<ProductModel>> ProductGroups { get; set; }

		public double MinOrderSizeSum =>
			OrderSizeDiscounts != null && OrderSizeDiscounts.Any() ? OrderSizeDiscounts.Min(t => t.From) : int.MaxValue;

		public EventModel ActiveEvent { get; set; }

		public string ActiveEventAnnouncement
		{
			get
			{
				if (ActiveEvent == null)
					return null;
				if (ActiveEvent.EndDate.HasValue)
					return string.Format(Const.EventTitle,
						ActiveEvent.EndDate.Value.ToShortDateString(), ActiveEvent.DiscountPercent);
				else
					return string.Format(Const.EndlessEventTitle, ActiveEvent.DiscountPercent);
			}
		}

		public SelectList Clients { get; set; }

		public bool ReadOnly { get; set; } = false;

		public string Promo { get; set; }

		public IEnumerable<OptOrderSizeDiscount> OrderSizeDiscounts { get; set; }
	}
}
