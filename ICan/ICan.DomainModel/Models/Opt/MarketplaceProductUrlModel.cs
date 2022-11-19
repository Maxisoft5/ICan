using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class MarketplaceProductUrlModel
	{
		public int MarketplaceProductUrlId { get; set; }
		public int MarketplaceProductId { get; set; }
		[Display(Name = "Ссылка")]
		public string Url { get; set; }	
	  
	}
}
