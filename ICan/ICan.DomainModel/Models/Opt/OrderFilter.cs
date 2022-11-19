using Newtonsoft.Json;

namespace ICan.Common.Models.Opt
{
	public class OrderFilter
	{
		public string ShortOrderDisplayId { get; set; }
		public string OrderDateDisplay { get; set; }
		public string Client { get; set; }
		public string ClientTypeName { get; set; }

		[JsonProperty("client.phoneNumber")]
		public string ClientPhoneNumber { get; set; }
		public string OrderStatus { get; set; }
		public string PersonalDiscountPercent { get; set; }
		public string RequisitesOwner { get; set; }
		public bool? IsPaid { get; set; }
		public string Comment { get; set; }
	}
}
