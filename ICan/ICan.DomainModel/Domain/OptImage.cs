namespace ICan.Common.Domain
{
	public class OptImage
	{
		public int ImageId { get; set; }

		public int Order { get; set; }

		public string FileName { get; set; }
		
		public string UserFileName { get; set; }

		public int ImageTypeId { get; set; }
		public int ObjectTypeId { get; set; }
		public int ObjectId { get; set; }
	}
}
