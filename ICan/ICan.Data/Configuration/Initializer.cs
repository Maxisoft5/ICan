using ICan.Common.Repositories;
using ICan.Data.Context;
using ICan.Data.Repositories;
using ICan.Data.Repostories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ICan.Data.Configuration
{
	public static class Initializer
	{
		public static IServiceCollection ConfigureDal(this IServiceCollection services, IConfiguration configuration, ILogger<int> logger)
		{
			//SetSettings(services, configuration, logger);
			AddDependenciesToContainer(services);

			return services;
		}

		private static void SetSettings(IServiceCollection services, IConfiguration configuration, ILogger<int> logger)
		{
			//var connecionString = configuration.GetConnectionString("UTGDatabase");
			//logger.LogInformation(connecionString);
			//services.AddDbContext<UtgContext>(options =>
			//{
			//	options.UseNpgsql(connecionString);
			//});
			services.AddDbContext<ApplicationDbContext>(options =>
			options.UseMySql(configuration.GetConnectionString("MySQLConnection"),
				ServerVersion.AutoDetect(configuration.GetConnectionString("MySQLConnection")))
			//.EnableSensitiveDataLogging()
			);
		}

		private static void AddDependenciesToContainer(IServiceCollection services)
		{
			services.AddScoped<IPriceRepository, PriceRepository>();
			services.AddScoped<IProductRepository, ProductRepository>();
			services.AddScoped<IWarehouseRepository, WarehouseRepository>();
			services.AddScoped<ISiteRepository, SiteRepository>();
			services.AddScoped<ISiteFilterRepository, SiteFilterRepository>();
			services.AddScoped<IPaperOrderRepository, PaperOrderRepository>();
			services.AddScoped<IPrintOrderRepository, PrintOrderRepository>();
			services.AddScoped<ISemiproductRepository, SemiproductRepository>();
			services.AddScoped<IWbReportRepository, WbReportRepository>();
			services.AddScoped<IShopRepository, ShopRepository>();
			services.AddScoped<ISpringRepository, SpringRepository>();
			services.AddScoped<INumberOfTurnsRepository, NumberOfTurnsRepository>();
			services.AddScoped<ISpringOrderRepository, SpringOrderRepository>();
			services.AddScoped<ISpringOrderIncomingRepository, SpringOrderIncomingRepository>();
			services.AddScoped<IPaymentRepository, PaymentRepository>();
			services.AddScoped<IMovePaperRepository, MovePaperRepository>();
			services.AddScoped<IPrintOrderPaperRepository, PrintOrderPaperRepository>();
			services.AddScoped<IWarehouseJournalRepository, WarehouseJournalRepository>();
			services.AddScoped<IOrderRepository, OrderRepository>();
			services.AddScoped<IAssemblyRepository, AssemblyRepository>();
			services.AddScoped<IDiscountRepository, DiscountRepository>();
			services.AddScoped<ICampaignRepository, CampaingRepository>();
			services.AddScoped<IImageRepository, ImageRepository>();
			services.AddScoped<IMaterialRepository, MaterialRepository>();
			services.AddScoped<IUserRepository, UserRepository>();
			services.AddScoped<INotchOrderRepository, NotchOrderRepository>();
		}
	}
}
