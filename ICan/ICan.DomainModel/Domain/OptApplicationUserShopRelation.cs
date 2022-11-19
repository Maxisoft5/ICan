using ICan.Common.Models;

namespace ICan.Common.Domain
{
	public class OptApplicationUserShopRelation
	{
		public string UserId { get; set; }
		public int ShopId { get; set; }
		public ApplicationUser User { get; set; }
		public OptShop Shop { get; set; }
	}
}
