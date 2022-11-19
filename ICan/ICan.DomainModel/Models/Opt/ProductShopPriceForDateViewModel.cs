using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	/// <summary>
	/// Класс для вывода цен на полученных в ходе конкретного парсинга
	/// - по конкретному продукту (строки) выводятся цены во всех магазинах (столбцы)
	/// </summary>
	public class ProductShopPriceForDateViewModel
	{
		[Display(Name = "Данные собраны ")]
		public DateTime ParsingDate { get; set; }

		public IEnumerable<ShopModel> Shops { get; set; }

		public IEnumerable<ProductShopPriceForDate> ProductShopPriceForDateList { get; set; }
	}

	/// <summary>
	/// Вспомогательный класс для вывода цен в магазинах для конкретного продукта
	/// </summary>
	public class ProductShopPriceForDate
	{
		public int ProductId { get; set; }

		[Display(Name = "Товар")]
		public string ProductName { get; set; }

		public Dictionary<int, PriceShopLink> PriceShopLinks { get; set; }

	}

	/// <summary>
	/// Вспомогательный класс для вывода цены и ссылки на продукт в интернет магазине
	/// </summary>
	public class PriceShopLink
	{
		public int ProductShopPriceId { get; set; }

		public double? Price { get; set; }

		public string Link { get; set; }

	}
}
