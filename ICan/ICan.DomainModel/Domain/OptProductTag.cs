namespace ICan.Common.Domain
{
	public class OptProductTag
	{
		public int ProductTagId { get; set; }
		public int TagId { get; set; }
		public int ProductId { get; set; }

		public OptProduct Product { get; set; }
		public OptTag Tag { get; set; }
	}
}
