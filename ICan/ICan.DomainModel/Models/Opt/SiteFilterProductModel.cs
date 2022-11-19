using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class SiteFilterProductModel
	{
		public int SiteFilterProductId { get; set; }

		public int SiteFilterId { get; set; }
		[Display(Name = "Тетрадь")]
		public int ProductId { get; set; }

		[Display(Name = "Порядок показа на сайте")]
		public int? Order { get; set; }

		public ProductModel Product { get; set; }
		public IEnumerable<SelectListItem> AvailableProducts { get; set; }
	}
}
