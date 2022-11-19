using ICan.Common.Domain;
using ICan.Business.Managers;
using Microsoft.Extensions.DependencyInjection;

namespace ICan.Business.Services
{
	public static class ManagerRegistrator
	{
		public static void Register(IServiceCollection services)
		{

			services.AddScoped<ProductManager, ProductManager>();
			services.AddScoped<EventManager, EventManager>();
			services.AddScoped<OrderManager, OrderManager>();
			services.AddScoped<PriceManager, PriceManager>();
			services.AddScoped<WarehouseManager, WarehouseManager>();
			services.AddScoped<NotchOrderManager, NotchOrderManager>();

			services.AddScoped<ReportManager, ReportManager>();
			services.AddScoped<OrderMailManager, OrderMailManager>();
			services.AddScoped<SemiproductWarehouseManager, SemiproductWarehouseManager>();
			services.AddScoped<AssemblyManager, AssemblyManager>();
			services.AddScoped<PrintOrderManager, PrintOrderManager>();
			services.AddScoped<CalcManager, CalcManager>();
			services.AddScoped<GlobalSettingManager, GlobalSettingManager>();
			services.AddScoped<FormatManager, FormatManager>();
			services.AddScoped<CommonManager<OptPaper>, CommonManager<OptPaper>>();
			services.AddScoped<CommonManager<OptUtmStatistics>, CommonManager<OptUtmStatistics>>();
			services.AddScoped<CommonManager<OptProductseries>, CommonManager<OptProductseries>>();
			services.AddScoped<CommonManager<OptRequisites>, CommonManager<OptRequisites>>();
			services.AddScoped<CommonManager<OptWarehouseType>, CommonManager<OptWarehouseType>>();
			services.AddScoped<CommonManager<OptTag>, CommonManager<OptTag>>();
			services.AddScoped<CommonManager<OptSiteFilter>, CommonManager<OptSiteFilter>>();
			services.AddScoped<CommonManager<OptCountry>, CommonManager<OptCountry>>();
			services.AddScoped<CommonManager<OptGluePadIncoming>, CommonManager<OptGluePadIncoming>>();
			services.AddScoped<CommonManager<OptBlockType>, CommonManager<OptBlockType>>();
			services.AddScoped<WarehouseJournalManager, WarehouseJournalManager>();
			services.AddScoped<SemiproductManager, SemiproductManager>();
			services.AddScoped<PaperOrderManager, PaperOrderManager>();
			services.AddScoped<CounterPartyManager, CounterPartyManager>();
			services.AddScoped<ShopManager, ShopManager>();
			services.AddScoped<DiscountManager, DiscountManager>();
			services.AddScoped<UpdPaymentManager, UpdPaymentManager>();
			services.AddScoped<OrderSizeDiscountManager, OrderSizeDiscountManager>();
			services.AddScoped<WbManager, WbManager>();
			services.AddScoped<WbApiService, WbApiService>();
			services.AddScoped<TypeOfPaperManager, TypeOfPaperManager>();
			services.AddScoped<PaperWarehouseManager, PaperWarehouseManager>();
			services.AddScoped<AdminManager, AdminManager>();
			services.AddScoped<MarketplaceManager, MarketplaceManager>();
			services.AddScoped<SiteManager, SiteManager>();
			services.AddScoped<SiteFilterManager, SiteFilterManager>();
			services.AddScoped<GluePadWarehouseManager, GluePadWarehouseManager>();
			services.AddScoped<UniversalWarehouseManager, UniversalWarehouseManager>();
			services.AddScoped<SpringManager, SpringManager>();
			services.AddScoped<OzonApiManager, OzonApiManager>();
			services.AddScoped<SpringOrderManager, SpringOrderManager>();
			services.AddScoped<MovePaperManager, MovePaperManager>();
			//services.AddScoped<MailChimpService, MailChimpService>();
			services.AddScoped<HttpRequestSenderService, HttpRequestSenderService>();
			services.AddScoped<CampaignManager, CampaignManager>();
			services.AddScoped<S3FileManager, S3FileManager>();
			services.AddScoped<MaterialManager, MaterialManager>();
			services.AddScoped<ImageManager, ImageManager>();

			services.AddTransient<IEmailSender, EmailSender>();

			
			services.AddScoped<ReportCriteriaService, ReportCriteriaService>();
			services.AddScoped<ReportParseService, ReportParseService>();
		}
	}
}
