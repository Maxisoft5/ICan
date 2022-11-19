using System;

namespace ICan.Common.Models.Opt
{
	public class OrderPhotoModel
	{
		public int OrderPhotoId { get; set; }

		public Guid OrderId { get; set; }

		public DateTime PhotoDate { get; set; }
		public byte[] Photo { get; set; }


		public string Base64Photo =>
			Photo != null ? string.Format("data:image/gif;base64,{0}", Convert.ToBase64String(Photo)) : null;

	}
}
