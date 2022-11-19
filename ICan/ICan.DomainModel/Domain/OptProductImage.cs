namespace ICan.Common.Domain
{
	public class OptProductImage
	{
		public int ProductImageId { get; set; }

		public int ProductId { get; set; }
		 
		public int Order { get; set; }

		public string FileName { get; set; }
		
		public string UserFileName { get; set; }

		public int ImageType { get; set; }

		public OptProduct Product { get; set; }
	}
}
