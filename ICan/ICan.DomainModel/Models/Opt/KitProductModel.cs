using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class KitProductModel
	{
		public int KitProductId { get; set; }
		public int ProductId { get; set; }
		public int MainProductId { get; set; }

		[Display(Name = "Название")]
		public string ProductName { get; set; }
		public bool ProductEnabled { get; set; }
	}
}
