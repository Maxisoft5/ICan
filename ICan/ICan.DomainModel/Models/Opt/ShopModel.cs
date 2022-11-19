using ICan.Common.Models.Opt.Report;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ICan.Common.Models.Opt
{
	public class ShopModel
	{
		public int ShopId { get; set; }

		[Display(Name = "Название магазина")]
		public string Name { get; set; }

		[Display(Name = "ИНН")]
		public string INN { get; set; }

		[Display(Name = "URL магазина")]
		public string ShopUrl { get; set; }

		[Display(Name = "Грузополучатель")]
		public string Consignee { get; set; }

		[Display(Name = "Маркетплейс")]
		public bool IsMarketPlace { get; set; }

		[Display(Name = "Не резидент")]
		public bool NonResident { get; set; }

		[Display(Name = "Игнорировать УПД")]
		public bool IgnoreInWarehouseCalc { get; set; }

		[Display(Name = "Используется")]
		public bool Enabled { get; set; } = true;

		[Display(Name = "Названия юр лиц")]
		public List<ShopNameModel> ShopNames { get; set; }

		[Display(Name = "Отсрочка оплаты, дни")]
		public short? Postponement { get; set; }

		[Display(Name = "Скан договора")]
		public string ScanFileName { get; set; }

		public string ClientId { get; set; }

		public string MimeType { get; set; }

		public bool CanDeleteShop => ShopId > Enum.GetValues(typeof(ShopType)).Cast<int>().Max();
	}
}
