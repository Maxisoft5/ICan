using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class SemiproductTypeModel
	{
		public int SemiproductTypeId { get; set; }

		[Display(Name = "Название")]
		public string Name { get; set; }

		public ICollection<SemiproductModel> Semiproducts { get; set; }
	}
}
