namespace ICan.Common.Domain
{
	public class OptSemiproductProductRelation
	{
		public int SemiproductId { get; set; }
		public int ProductId { get; set; }

		public OptSemiproduct Semiproduct { get; set; }
		public OptProduct Product { get; set; }
	}
}