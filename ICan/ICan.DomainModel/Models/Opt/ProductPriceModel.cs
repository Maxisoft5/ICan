using ICan.Common.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class ProductpriceModel
	{
		public int ProductPriceId { get; set; }

		public int ProductId { get; set; }
		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		[Display(Name = "Цена")]
		[Range(0.01, 999999)]
		public double Price { get; set; }

		[Display(Name = "Актуальна с")]
		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		public DateTime? DateStart { get; set; }

		[Display(Name = "Актуальна по")]
		public DateTime? DateEnd { get; set; }

		public OptProduct Product { get; set; }
		public ICollection<OptOrderproduct> OptOrderproduct { get; set; }

		public ProductpriceModel()
		{
		}

		public ProductpriceModel(OptProductprice optPrice)
		{
			ProductId = optPrice.ProductId;
			Price = optPrice.Price;
			DateStart = optPrice.DateStart;
			DateEnd = optPrice.DateEnd;
			ProductPriceId = optPrice.ProductPriceId;
		}
	}
}
