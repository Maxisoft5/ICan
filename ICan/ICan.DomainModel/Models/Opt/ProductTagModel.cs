using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class ProductTagModel
	{
		public int ProductTagId { get; set; }
		[Display(Name="Тег")]
		public int TagId { get; set; }
		public int ProductId { get; set; }

		public string TagName { get; set; }
	}
}
