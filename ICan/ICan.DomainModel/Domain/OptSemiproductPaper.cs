namespace ICan.Common.Domain
{
	public partial class OptSemiproductPaper
	{
		public int SemiproductPaperId { get; set; }
		public int PaperId { get; set; }
		public int SemiproductId { get; set; }
		public OptPaper Paper { get; set; }
		public OptSemiproduct Semiproduct { get; set; }
	}
}
