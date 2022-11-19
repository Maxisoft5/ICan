using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ICan.Common.Models.Opt
{
	public class NotchOrderModel
	{
		public int NotchOrderId { get; set; }
		[Display(Name = "Номер заказа")]
		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		public string NotchOrderNumber { get; set; }

		[Display(Name = "Дата заказа")]
		[DataType(DataType.DateTime)]
		public DateTime OrderDate { get; set; }

		public bool IsUsed { get; set; }

		[Display(Name = "Стоимость заказа")]
		public decimal? OrderSum { get; set; }

		[Display(Name = "Дата отправки")]
		public DateTime? ShipmentDate { get; set; }

		[Display(Name = "Дата отправки")]
		public string ShipmentDateFormatted => ShipmentDate?.ToShortDateString();

		[Display(Name = "Стоимость отправки")]
		public decimal? ShipmentSum { get; set; }
		public bool CanBeUpdated => NotchOrderIncomings == null || !NotchOrderIncomings.Any();

		public List<NotchOrderItemModel> NotchOrderItems { get; set; }
		public List<NotchOrderStickerModel> NotchOrderStickers { get; set; }

		[Display(Name = "Состав заказа")]
		public string NotchOrderItemsDisplay
		{
			get
			{
				if (NotchOrderItems != null && NotchOrderItems.Any())
				{
					var printOrders = NotchOrderItems.Select(item => item.PrintOrder);

					if (printOrders.Any(x => x == null))
						return null;

					var semiproducts = printOrders?.SelectMany(x => x.PrintOrderSemiproducts).Select(x => x.SemiProduct);

					if (NotchOrderStickers != null && NotchOrderStickers.Any())
					{
						semiproducts = NotchOrderStickers.Select(x => x.Semiproduct);
					}

					return string.Join(", <br>", printOrders?.Select(x => x.DisplayName).Concat(semiproducts.Select(s => s.DisplayName)));
				}

				return null;
			}
		}

		public List<NotchOrderIncomingModel> NotchOrderIncomings { get; set; }

		public string GetDisplayForAssembly()
		{
			return $"{OrderDate.ToShortDateString()} {NotchOrderNumber}";
		}
	}
}
