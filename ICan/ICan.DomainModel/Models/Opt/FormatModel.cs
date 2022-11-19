using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class FormatModel
	{
		public long FormatId { get; set; }

		[Display(Name = "Название")]
		public string Name { get; set; }
		[Display(Name = "Описание")]
		public string Description { get; set; }

		public ICollection<SemiproductModel> Semiproducts { get; set; }
		public ICollection<PaperOrderModel> PaperOrders { get; set; }
	}
}
