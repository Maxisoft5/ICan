using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ICan.Common.Models.Opt
{
	public class OrderProductModel
	{
		[Display(Name = "Количество")]
		public int Amount { get; set; }
		public int Productid { get; set; }
		[Display(Name = "Продукт")]
		public string Product { get; set; }
		[Display(Name = "Цена за единицу, руб")]
		public double Price { get; set; }

		[Display(Name = "Цена с учётом скидки, руб")]
		public double DiscountedPrice { get; set; }

		[Display(Name = "Вес, кг")]
		public double Weight { get; set; }

		public int ProductPriceId { get; set; }

		public Guid OrderId { get; set; }

		public int ProductKindId { get; set; }

		public bool IsKit { get; set; }

		public bool IsNoteBook => ProductKindId == 1;

		public IEnumerable<KitProductModel> KitProducts { get; set; }

		public int ActingAmount
		{
			get
			{
				if (IsNoteBook)
				{
					if (!IsKit || (KitProducts == null) || !KitProducts.Any())
						return 1;

					var kitSize = KitProducts.Where(kitP => kitP.ProductEnabled).Count();
					return kitSize;
				}
				else
				{
					return 0;
				}

			}
		}

	}
}