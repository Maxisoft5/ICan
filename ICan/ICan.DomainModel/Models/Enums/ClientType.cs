using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Enums
{
	public enum ClientType
	{
		[Display(Name = "Неизвестно")]
		Unknown,
		[Display(Name = "Mагазин")]
		Shop,
		[Display(Name = "СП")]
		JointPurchase
	}

	public static class ClientTypeEx
	{
		public static string GetName(this ClientType clientType)
		{

			switch (clientType)
			{
				case ClientType.Unknown:
					return "Неизвестно";
				case ClientType.Shop:
					return "Mагазин";
				case ClientType.JointPurchase:
					return "СП";
				default:
					return "Не задано";
			}
		}

		public static ClientType GetClientTypeByName(this string clientType)
		{
			switch (clientType)
			{
				case "Mагазин":
					return ClientType.Shop;
				case "Совместная покупка":
					return ClientType.JointPurchase;
				default:
					return ClientType.Unknown;
			}
		}
	}
}