using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public partial class OptPaper
	{
		public OptPaper()
		{
			SemiproductPapers = new HashSet<OptSemiproductPaper>();
			PaperOrders = new HashSet<OptPaperOrder>();
		}

		public int PaperId { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }

		public int? TypeOfPaperId { get; set; }

		public int? Width { get; set; }

		public int? Length { get; set; }

		public OptTypeOfPaper TypeOfPaper { get; set; }

		public ICollection<OptSemiproductPaper> SemiproductPapers { get; set; }
		public ICollection<OptPaperOrder> PaperOrders { get; set; }
	}
}
