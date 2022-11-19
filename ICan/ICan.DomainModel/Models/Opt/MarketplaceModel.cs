using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class MarketplaceModel
	{
		public int MarketplaceId { get; set; }
		[Required]
		[Display(Name = "Название")]
		public string Name { get; set; }
		
		[Display(Name = "Картинка")]
		public string ImageName { get; set; }
		
		[Display(Name = "Ссылка")]
		public string Url { get; set; }
		
		public string ImagePath { get; set; }
	}
}
