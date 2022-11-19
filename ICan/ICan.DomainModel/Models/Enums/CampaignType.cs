using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Enums
{
	public enum CampaignType
	{
		[Display(Name = "-")]
		None,
		[Display(Name = "Mагазин")]
		Shop,
		[Display(Name = "СП")]
		JointPurchase,
		[Display(Name = "Тест")]
		Test
	}
}