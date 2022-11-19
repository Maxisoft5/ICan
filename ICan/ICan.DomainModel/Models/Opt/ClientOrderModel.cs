using System;

namespace ICan.Common.Models.Opt
{
	public class ClientOrderModel
	{
		public ShortOrderProductModel[] OrderItems { get; set; }
		public string ClientId { get; set; }
		public string Address { get; set; }
		public string PvzAddress { get; set; }
		public string OrderId { get; set; } = Guid.Empty.ToString();
	}
}
