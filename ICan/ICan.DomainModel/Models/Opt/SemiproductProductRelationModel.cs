namespace ICan.Common.Models.Opt
{
	public class SemiproductProductRelationModel
	{
		public int SemiproductId { get; set; }
		public int ProductId { get; set; }

		public SemiproductModel Semiproduct { get; set; }
		public ProductModel Product { get; set; }
	}
}
