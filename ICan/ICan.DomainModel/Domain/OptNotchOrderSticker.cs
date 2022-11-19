namespace ICan.Common.Domain
{
	public class OptNotchOrderSticker
	{
		public int NotchOrderStickerId { get; set; }
		public int NotchOrderId { get; set; }
		public int SemiproductId { get; set; }

		public bool IsAssembled { get; set; }

		public OptNotchOrder NotchOrder { get; set; }
		public OptSemiproduct Semiproduct { get; set; }
	}
}
