using ICan.Business.Managers;
using ICan.Business.Services;
using ICan.Common.Domain;
using ICan.Data.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ICan.Jobs.WB
{
	public class WbSalesImportJob : BaseWBImportiJob<OptWbSale>
	{
		public WbSalesImportJob(ILogger<BaseWBImportiJob<OptWbSale>> logger, IConfiguration configuration, IEmailSender emailSender, ApplicationDbContext context, ProductManager productManager) : base(logger, configuration, emailSender, context, productManager)
		{
			Url = configuration["Settings:WB:SalesApiUrl"];
			DataTypeStr = "Продажи";
		}
	}
}