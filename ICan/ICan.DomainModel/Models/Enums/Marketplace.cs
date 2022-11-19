using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Enums
{
	public enum Marketplace
	{
		None = 0,
		[Display(Name = "Wildberries")]
		WB =1 ,
		[Display(Name = "ЯндексМаркет")]
		YandexMarket = 2,
		[Display(Name = "Озон")]
		Ozon =3 ,
		[Display(Name = "Amazon UK")]
		AmazonUK = 4
	}
}