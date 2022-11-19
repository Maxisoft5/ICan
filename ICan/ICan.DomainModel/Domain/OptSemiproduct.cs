using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public partial class OptSemiproduct
	{
		public int SemiproductId { get; set; }

		public int ProductId { get; set; }

		public int SemiproductTypeId { get; set; }

		public int? FormatId { get; set; }

		public int StripNumber { get; set; }

		public string Description { get; set; }

		public bool HaveWDVarnish { get; set; }

		public bool HaveStochastics { get; set; }

		public int? CutLength { get; set; }

		public string Name { get; set; }
		public bool IsUniversal { get; set; }

		public int? BlockTypeId { get; set; }

		public OptBlockType BlockType { get; set; }

		public OptFormat Format { get; set; }

		public OptProduct Product { get; set; }

		public OptSemiproductType SemiproductType { get; set; }

		public ICollection<OptSemiproductWarehouseItem> SemiproductWarehouseItems { get; set; }

		public ICollection<OptPrintOrderSemiproduct> PrintOrderSemiproducts { get; set; }

		public ICollection<OptSemiproductPaper> SemiproductPapers { get; set; }
		
		public ICollection<OptNotchOrderIncomingItem> NotchOrderIncomingItems { get; set; }
		public ICollection<OptSemiproductProductRelation> RelatedProducts { get; set; }
		public ICollection<OptNotchOrderSticker> NotchOrderStickers { get; set; }
	}
}
