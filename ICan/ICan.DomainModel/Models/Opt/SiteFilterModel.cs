using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ICan.Common.Models.Opt
{
	public class SiteFilterModel
	{
		public int SiteFilterId { get; set; }

		[Display(Name="Название")]
		[Required]
		public string Name { get; set; }
		[Display(Name = "Сайт")]
		public string SiteName { get; set; }
	 
		public bool IsInternal { get; set; }

		public ICollection<SiteFilterProductModel> Products { get; set; }

		[Display(Name = "Товары")]
		public IEnumerable<string> ProductNames => 
			Products?.OrderBy(siteFilterProd=> siteFilterProd.Order)
			.Select(product => product.Product?.DisplayName);
	}
}
