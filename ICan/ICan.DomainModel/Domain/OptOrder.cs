using ICan.Common.Models;
using System;
using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public class OptOrder
	{
		public OptOrder()
		{
			OptOrderpayments = new HashSet<OptOrderpayment>();
			OptOrderproducts = new HashSet<OptOrderproduct>();
		}

		public Guid OrderId { get; set; }

		public int ShortOrderId { get; set; }
		public int? ShopId{ get; set; }

		public int OrderStatusId { get; set; }
		public int? RequisitesId { get; set; }

		public DateTime OrderDate { get; set; }
		public DateTime? DoneDate { get; set; }
		public DateTime? AssemblyDate { get; set; }

		public string ClientId { get; set; }
		public ApplicationUser Client { get; set; }

		public string ClientAddress { get; set; }
		public string DeliveryPointAddress { get; set; }

		public OptOrderstatus OrderStatus { get; set; }
		public OptRequisites Requisites { get; set; }
		public ICollection<OptOrderpayment> OptOrderpayments { get; set; }
		public ICollection<OptOrderproduct> OptOrderproducts { get; set; }

		public double TotalSum { get; set; }
		public double DiscountedSum { get; set; }

		public float? EventDiscountPercent { get; set; }

		public double? PersonalDiscountPercent { get; set; }
		public double? OrderSizeDiscountPercent { get; set; }

		public double TotalWeight { get; set; }

		public string TrackNo { get; set; }
		public string Comment { get; set; }
		public string UpdNum { get; set; }

		public int? EventId { get; set; }
		public long? PersonalDiscountId { get; set; }


		public OptShop Shop { get; set; }
		public OptDiscount Discount { get; set; }
		public OptEvent Event { get; set; }

		public ICollection<OptOrderPhoto> Photos { get; set; }

		public bool IsPaid { get; set; }
	}
}
