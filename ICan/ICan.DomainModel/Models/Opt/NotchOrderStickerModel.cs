namespace ICan.Common.Models.Opt
{
	public class NotchOrderStickerModel
	{
		public int NotchOrderStickerId { get; set; }

		public int NotchOrderId { get; set; }

		public int SemiproductId { get; set; }

		public bool IsAssembled { get; set; }

		public string SemiproductDisplayName { get; set; }
		public SemiproductModel Semiproduct { get; set; }
	}
}
