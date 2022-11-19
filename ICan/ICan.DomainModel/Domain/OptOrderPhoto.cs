using System;


namespace ICan.Common.Domain
{
	public partial class OptOrderPhoto
	{
		public int OrderPhotoId { get; set; }

		public Guid OrderId { get; set; }

		public DateTime PhotoDate { get; set; }
		public string PhotoPath { get; set; }

		public OptOrder Order { get; set; }
	}
}
