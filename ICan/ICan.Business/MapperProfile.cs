using AutoMapper;
using ICan.Common.Models.Enums;
using ICan.Common.Domain;
using ICan.Common.Models.WB;
using System;
using System.Globalization;
using System.Linq;
using ICan.Common.Models.Opt;
using ICan.Common.Models.ManageViewModels;
using ICan.Common.Models;

namespace ICan.Business
{
	public class MapperProfile : Profile
	{
		public MapperProfile()
		{
			CreateMap<CampaignModel, OptCampaign>()
				.ForMember(dest => dest.CampaignType, src => src.MapFrom(opt => (int)opt.CampaignType));
			CreateMap<OptCampaign, CampaignModel>()
				.ForMember(dest => dest.CampaignType, src => src.MapFrom(opt => Enum.Parse(typeof(CampaignType), opt.CampaignType.ToString())));
			CreateMap<OptBlockType, BlockTypeModel>().ReverseMap();
			CreateMap<OptMovePaper, MovePaperModel>()
				.ForMember(dest => dest.PaperName, opt => opt.MapFrom(src => src.Paper != null ? src.Paper.Name : string.Empty))
				.ForMember(dest => dest.ReceiverName, opt => opt.MapFrom(src => src.ReceiverWarehouse != null ? src.ReceiverWarehouse.Name : string.Empty))
				.ForMember(dest => dest.SenderName, opt => opt.MapFrom(src => src.SenderWarehouse != null ? src.SenderWarehouse.Name : string.Empty));
			CreateMap<MovePaperModel, OptMovePaper>().ForMember(dest => dest.Paper, src => src.Ignore());
			CreateMap<OptPayment, PaymentModel>().ReverseMap();
			CreateMap<OptSpringOrderIncoming, SpringOrderIncomingModel>().ReverseMap();
			CreateMap<OptSpringOrder, SpringOrderModel>().ReverseMap();
			CreateMap<OptSpring, SpringModel>().ReverseMap();
			CreateMap<OptNumberOfTurns, NumberOfTurnsModel>().ReverseMap();
			CreateMap<OptApplicationUserShopRelation, ApplicationUserShopRelationModel>().ReverseMap();
			CreateMap<OptGluePadIncoming, GluePadIncomingModel>().ReverseMap();
			CreateMap<SemiproductModel, SemiproductShortModel>();
			CreateMap<IWBItemWithProductId, IWbItemModel>();

			CreateMap<OptTag, TagModel>().ReverseMap();
			CreateMap<OptSemiproductProductRelation, SemiproductProductRelationModel>().ReverseMap();
			CreateMap<OptProductTag, ProductTagModel>()
				.ForMember(dest => dest.TagName, src => src.MapFrom(raw => raw.Tag != null ? raw.Tag.TagName : string.Empty));
			CreateMap<ProductTagModel, OptProductTag>();
			CreateMap<OptProduct, SiteProductModel>().ReverseMap();
			CreateMap<OptProductImage, ProductImageModel>().ReverseMap();
			CreateMap<OptMarketplace, MarketplaceModel>().ReverseMap();
			CreateMap<OptTypeOfPaper, TypeOfPaperModel>().ReverseMap();
			CreateMap<OptProduct, ProductModel>().ReverseMap();
			CreateMap<OptPaperOrderIncoming, PaperOrderIncomingModel>()
				.ForMember(dest => dest.WarehouseTypeId, src => src.MapFrom(q => Enum.Parse(typeof(WarehouseType), q.WarehouseTypeId.ToString())));

			CreateMap<PaperOrderIncomingModel, OptPaperOrderIncoming>()
				.ForMember(dest => dest.WarehouseTypeId, src => src.MapFrom(q => (int)q.WarehouseTypeId));

			CreateMap<OptProductprice, ProductpriceModel>().ReverseMap();
			CreateMap<OptKitproduct, KitProductModel>()
				.ForMember(dest => dest.ProductEnabled, src =>
					src.MapFrom(from => (from.Product != null ? from.Product.Enabled : false)))
				.ForMember(dest => dest.ProductName, src =>
					src.MapFrom(from => (from.Product != null ? from.Product.Name : string.Empty)))
				.ReverseMap();
			CreateMap<OptSemiproduct, SemiproductModel>()
				.ForMember(dest => dest.SeriesName, src => src.MapFrom(t =>
						 (t.Product != null) && t.Product.ProductSeries != null ? t.Product.ProductSeries.Name : ""))
				.ForMember(dest => dest.ProductSeriesId, src => src.MapFrom(t =>
						 (t.Product != null) ? t.Product.ProductSeriesId : (int?)null))
				.ForMember(dest => dest.SemiproductTypeName, src =>
					src.MapFrom(from => (from.SemiproductType != null && !string.IsNullOrWhiteSpace(from.SemiproductType.Name)) ? from.SemiproductType.Name : ""))
				.ForMember(dest => dest.BlockTypeName, src =>
					src.MapFrom(from => from.BlockType != null ? from.BlockType.Name : null))
			.ReverseMap();

			CreateMap<OptProduct, ShortProductModel>().ReverseMap();
			CreateMap<OptPrintOrderPayment, PrintOrderPaymentModel>().ReverseMap();
			CreateMap<OptEvent, EventModel>().ReverseMap();
			CreateMap<OptOrderPhoto, OrderPhotoModel>().ReverseMap();
			CreateMap<OptProductseries, ProductSeriesModel>().ReverseMap();

			CreateMap<OptPaperOrder, PaperOrderModel>().ReverseMap();

			CreateMap<OptPrintOrderPaper, PrintOrderPaperModel>()
				.ForMember(dest => dest.PrintOrderInfo, src => src.MapFrom(t =>
						 (t.PrintOrder != null) ? $"Заказ на {t.PrintOrder.Printing} от {t.PrintOrder.OrderDate }" : ""))
					.ReverseMap();

			CreateMap<OptCounterparty, CounterpartyModel>().ReverseMap();
			CreateMap<OptAssemblySemiproduct, AssemblySemiproductModel>()
				.ForMember(dest => dest.SemiproductTypeId, src => src.MapFrom(db =>
					db.PrintOrderSemiproduct != null &&
					db.PrintOrderSemiproduct.SemiProduct != null ? db.PrintOrderSemiproduct.SemiProduct.SemiproductTypeId :
					db.NotchOrderId != null ? (int)SemiProductType.Stickers : (int)SemiProductType.None))
				.ReverseMap();
			CreateMap<OptOrder, OrderModel>()
				 .ForMember(dest => dest.OrderStatus, src => src.MapFrom(t =>
					 t.OrderStatus != null ? t.OrderStatus.Name : ""))

				 .ForMember(dest => dest.RequisitesOwner, src => src.MapFrom(t =>
					 t.Requisites != null ? t.Requisites.Owner : ""))
				 .ForMember(dest => dest.OrderPayments, src => src.MapFrom(t =>
					 t.OptOrderpayments ?? Enumerable.Empty<OptOrderpayment>()))
				.ForMember(dest => dest.ClientTypeName, src => src.MapFrom(t =>
					 t.Client != null
					 ? ((ClientType)t.Client.ClientType).GetName()
					 : string.Empty))

				.ReverseMap();

			CreateMap<OptReport, ReportModel>()
				.ForMember(dest => dest.ShopName, src => src.MapFrom(t =>
						 t.Shop != null ? t.Shop.Name : ""))
				.ForMember(dest => dest.ReportKindName, src => src.MapFrom(t =>
						 t.ReportKind != null ? t.ReportKind.Name : ""))
				.ForMember(dest => dest.Month, src => src.MapFrom(t =>
						 DateTimeFormatInfo.CurrentInfo.GetMonthName(t.ReportMonth)))
			.ReverseMap();

			CreateMap<MaterialModel, OptMaterial>();
				
			CreateMap<OptShop, ShopModel>().ReverseMap();
			CreateMap<OptPaper, PaperModel>().ReverseMap();
			CreateMap<OptPrintOrderIncoming, PrintOrderIncomingModel>()
				.ReverseMap();
			CreateMap<OptPrintOrderIncomingItem, PrintOrderIncomingItemModel>()
				.ReverseMap();

			CreateMap<OptPrintOrder, PrintOrderModel>()
			.ForMember(dest => dest.SemiProductTypeId, src => src.MapFrom(t =>
			  t.PrintOrderSemiproducts != null && t.PrintOrderSemiproducts.Any()
			  && t.PrintOrderSemiproducts.All(sProd => sProd.SemiProduct != null)
			  ? t.PrintOrderSemiproducts.OrderBy(sprod => sprod.SemiProduct.SemiproductTypeId).First().SemiProduct.SemiproductTypeId
			  : (int?)null
			 ))
			.ReverseMap();

			CreateMap<OptPrintOrderSemiproduct, PrintOrderSemiproductModel>()
			.ReverseMap();

			CreateMap<OptReportitem, ReportItemModel>()
			  .ForMember(dest => dest.ReportMonth, src => src.MapFrom(t =>
						 t.Report != null ? t.Report.ReportMonth : (int?)null))
			  .ForMember(dest => dest.ReportYear, src => src.MapFrom(t =>
						 t.Report != null ? t.Report.ReportYear : (int?)null))
			  .ForMember(dest => dest.ShopId, src => src.MapFrom(t =>
						 t.Report != null ? t.Report.ShopId : (int?)null))
			 .ForMember(dest => dest.ReportKindId, src => src.MapFrom(t =>
						 t.Report != null ? t.Report.ReportKindId : (int?)null))
				.ReverseMap();

			CreateMap<OptWarehouse, WarehouseModel>()
					.ReverseMap();

			CreateMap<OptWarehouseItem, WarehouseItemModel>()
				.ForMember(dest => dest.Product, src => src.MapFrom(t =>
						 t.Product != null
						 ? (t.Product.Country == null ? t.Product.Name : $"{t.Product.Country.Prefix} {t.Product.Name}")
						 : ""))
				.ReverseMap();

			CreateMap<OptAssembly, AssemblyModel>()
				.ReverseMap();

			CreateMap<OptKitproduct, KitProductModel>()
				.ReverseMap();

			CreateMap<OptFormat, FormatModel>()
				.ReverseMap();

			CreateMap<OptSemiproductType, SemiproductTypeModel>()
				.ReverseMap();

			CreateMap<OptSemiproductWarehouse, SemiproductWarehouseModel>()
				.ReverseMap();

			CreateMap<OptSemiproductWarehouseItem, SemiproductWarehouseItemModel>()
			.ReverseMap();

			CreateMap<OptShopName, ShopNameModel>()
				.ReverseMap();

			CreateMap<OptRequisites, RequisitesModel>()
				.ReverseMap();

			CreateMap<OptGlobalSetting, GlobalSettingModel>()
				.ReverseMap();

			CreateMap<OptWarehouseJournal, WarehouseJournalModel>()
				.ReverseMap();

			CreateMap<OptUpdPayment, UpdPaymentModel>()
				.ReverseMap();

			CreateMap<OptNotchOrder, NotchOrderModel>()
				.ForMember(dst => dst.OrderSum, src => src.MapFrom(db => db.OrderSum.HasValue ? Math.Round(db.OrderSum.Value, 2) : (decimal?)null))
				.ForMember(dst => dst.ShipmentSum, src => src.MapFrom(db => db.ShipmentSum.HasValue ? Math.Round(db.ShipmentSum.Value, 2) : (decimal?)null))
				.ReverseMap();
			CreateMap<OptNotchOrderItem, NotchOrderItemModel>().ReverseMap();
			CreateMap<OptNotchOrderSticker, NotchOrderStickerModel>().ReverseMap();
			CreateMap<OptNotchOrderIncoming, NotchOrderIncomingModel>().ReverseMap();
			CreateMap<OptNotchOrderIncomingItem, NotchOrderIncomingItemModel>().ReverseMap();

			CreateMap<OptDiscount, DiscountModel>().ReverseMap();

			CreateMap<OptWbSale, WbSaleModel>().ReverseMap();
			CreateMap<OptWbOrder, WbOrderModel>().ReverseMap();
			CreateMap<OptWbWarehouse, WbWarehouseModel>().ReverseMap();
			CreateMap<OptWarehouseType, WarehouseTypeModel>().ReverseMap();
			CreateMap<OptCountry, CountryModel>().ReverseMap();
			CreateMap<OptSiteFilter, SiteFilterModel>();
			CreateMap<SiteFilterModel, OptSiteFilter>()
				.ForMember(dest => dest.Site, src => src.Ignore());

			CreateMap<OptSiteFilterProduct, SiteFilterProductModel>().ReverseMap();
			CreateMap<ApplicationUser, ClientModel>().ReverseMap();

			CreateMap<OptMarketplaceProduct, MarketplaceProductModel>();
			CreateMap<OptMarketplaceProductUrl, MarketplaceProductUrlModel>();
			CreateMap<MarketplaceProductUrlModel, OptMarketplaceProductUrl>();
			CreateMap<MarketplaceProductModel, OptMarketplaceProduct>()
				.ForMember(dest => dest.Marketplace, src => src.Ignore());

			CreateMap<OptPaper, PaperWarehouseModel>()
				.ForMember(dest => dest.TypeOfPaperDisplayName,
				src => src.MapFrom(t => t.TypeOfPaper != null ? t.TypeOfPaper.Name : ""));

			CreateMap<ProductModel, SelectProductModel>();

			CreateMap<OptImage, ImageModel>()
				.ForMember(dest => dest.ImageType,
				src => src.MapFrom(db => db.ImageTypeId));

			CreateMap<MaterialImage, MaterialModel>()
				.ForMember(dest => dest.MaterialId, src => src.MapFrom(db => db.Material.MaterialId))
				.ForMember(dest => dest.Theme, src => src.MapFrom(db => db.Material.Theme))
				.ForMember(dest => dest.IsActive, src => src.MapFrom(db => db.Material.IsActive))
				.ForMember(dest => dest.Date, src => src.MapFrom(db => db.Material.Date))
				.ForMember(dest => dest.Content, src => src.MapFrom(db => db.Material.Content))
				.ForMember(dest => dest.Images, src => src.MapFrom(db => db.Images));
		}
	}
}
