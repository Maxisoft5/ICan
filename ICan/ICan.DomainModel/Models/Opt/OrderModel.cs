using ICan.Common.Domain;
using ICan.Common.Utils;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace ICan.Common.Models.Opt
{
	public class OrderModel
	{
		[Display(Name = "Номер заказа")]
		public Guid OrderId { get; set; }
		[Display(Name = "Клиент")]
		public string ClientId { get; set; }

		[Display(Name = "Клиент")]
		public ApplicationUser Client { get; set; }

		[Display(Name = "Статус")]
		public int OrderStatusId { get; set; }
		[Display(Name = "Статус")]
		public string OrderStatus { get; set; }

		[Display(Name = "Дата заказа")]
		[DataType(DataType.Date)]
		public DateTime OrderDate { get; set; }

		[Display(Name = "Дата выполнения заказа")]
		[DataType(DataType.Date)]
		public DateTime? DoneDate { get; set; }

		[Display(Name = "Дата сборки заказа")]
		[DataType(DataType.DateTime)]
		public DateTime? AssemblyDate { get; set; }

		[Display(Name = "Общая сумма, руб")]
		public double TotalSum { get; set; }

		[Display(Name = "Сумма, руб")]
		public double DiscountedSum { get; set; }

		[Display(Name = "Сумма, руб")]
		public string DiscountedSumFormatted => DiscountedSum.ToString("N2", CultureInfo.CreateSpecificCulture("ru-RU"));

		[Display(Name = "Вес заказа, кг")]
		public double TotalWeight { get; set; }

		[Display(Name = "Количество тетрадей")]
		public int? NoteBookAmount => OrderProducts?.Sum(q => q.ActingAmount * q.Amount);

		[Display(Name = "Трекномер")]
		[StringLength(20, ErrorMessage = Const.ValidationMessages.MaxLengthExceeded)]
		public string TrackNo { get; set; }

		[Display(Name = "Номер заказа")]
		public int ShortOrderId { get; set; }

		public int? EventId { get; set; }
	
		[Display(Name = "Магазин клиента")]
		public int? ShopId { get; set; }

		[Display(Name = "Размер скидки по акции,%")]
		public double? EventDiscountPercent { get; set; }

		[Display(Name = "Размер индивидуальной скидки,%")]
		public long? PersonalDiscountId { get; set; }

		[Display(Name = "Размер индивидуальной скидки,%")]
		public double? PersonalDiscountPercent { get; set; }

		[Display(Name = "Общая скидка,%")]
		public double TotalDiscountedPercent => (EventDiscountPercent ?? 0) + (PersonalDiscountPercent ?? 0) + (OrderSizeDiscountPercent ?? 0);

		[Display(Name = "Размер скидки в зависимости от суммы заказа,%")]
		public double? OrderSizeDiscountPercent { get; set; }

		[Display(Name = "Магазин клиента")]
		public string ShopName { get; set; }

		[JsonIgnore]
		public List<OrderProductModel> OrderProducts { get; set; }
	 	[JsonIgnore]
		public List<OptOrderpayment> OrderPayments { get; set; }

		public IList<IFormFile> UploadOrderPhotos { get; set; }

	 
		[Display(Name = "Фотографии заказа")]
		public List<OrderPhotoModel> Photos { get; set; }

		[Display(Name = "Комментарий")]
		[DataType(DataType.MultilineText)]
		[MaxLength(2000)]
		public string Comment { get; set; }

		[Display(Name = "Адрес доставки")]
		public string ClientAddress { get; set; }
		
		[Display(Name = "Адрес ближайшего ПВЗ ПЭК")]
		public string DeliveryPointAddress { get; set; }

		[Display(Name = "Номер УПД")]
		public string UpdNum { get; set; }
	 
		[JsonIgnore]
		public ProductListModel ProductList { get; set; }

		[Display(Name = "Оплачен")]
		public bool IsPaid { get; set; }

		[Display(Name = "Статус оплаты")]
		public string IsPaidString => IsPaid ? "Оплачено" : "Не оплачено";

		public int PaymentsCount => OrderPayments?.Count ?? 0;
		public bool CurentUserCanEditStatus { get; set; }

		public double? PaidSum => OrderPayments?.Sum(t => t.Amount);

		[Display(Name = "Остаток")]
		public string RestSumFormatted => (DiscountedSum -
			(OrderPayments?.Sum(t => t.Amount)) ?? 0).ToString("N2", CultureInfo.CreateSpecificCulture("ru-RU"));

		[Display(Name = "Реквизиты")]
		public int? RequisitesId { get; set; }

		[Display(Name = "Реквизиты")]
		public string RequisitesOwner { get; set; }
		public string Promo { get; set; }

		//[Display(Name = "Скидка в зависимости от суммы заказа, %")]
		//public OptOrderSizeDiscount OrderSizeDiscount { get; set; }

		[Display(Name = "Тип клиента")]
		public int ClientType => Client?.ClientType ?? (int)Enums.ClientType.Unknown;

		[Display(Name = "Тип клиента")]
		public string ClientTypeName { get; set; }
	
		[Display(Name = "Номер заказа")]
		public string ShortOrderDisplayId =>
			ShortOrderId > 0 ? ShortOrderId.ToString() : Util.GetShortNum(OrderId);

		[Display(Name = "Дата заказа")]
		public string OrderDateDisplay =>
			OrderDate.ToShortDateString();
	}
}