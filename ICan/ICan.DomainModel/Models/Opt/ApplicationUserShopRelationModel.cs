using ICan.Common.Models.ManageViewModels;

namespace ICan.Common.Models.Opt
{
	public class ApplicationUserShopRelationModel
	{
		public string UserId { get; set; }
		public int ShopId { get; set; }
		public ClientModel User { get; set; }
		public ShopModel Shop { get; set; }
	}
}
