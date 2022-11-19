using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Domain
{
	public partial class OptFormat
	{
		public OptFormat()
		{
			Semiproducts = new HashSet<OptSemiproduct>();
		}

		public int FormatId { get; set; }

		[Display(Name = "Название")]
		public string Name { get; set; }
		[Display(Name = "Описание")]
		public string Description { get; set; }

		public ICollection<OptSemiproduct> Semiproducts { get; set; }
		public ICollection<OptPaperOrder> PaperOrders { get; set; }
	}
}
