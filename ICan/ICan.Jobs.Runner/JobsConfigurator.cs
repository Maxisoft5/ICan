using Hangfire;
using ICan.Jobs.OneC;
using ICan.Jobs.WB;
using Microsoft.Extensions.Configuration;
using ICan.Common;
using ICan.Jobs.Marketplaces;
using ICan.Common.Models.Enums;
using ICan.Jobs.Ozon;
using ICan.Jobs.ClearWhJournal;

namespace ICan.Jobs.Runner
{
	public static class JobsConfigurator
	{
		private static void DeleteExistingJobs()
		{
			var monitor = JobStorage.Current.GetMonitoringApi();

			var jobsProcessing = monitor.ProcessingJobs(0, int.MaxValue);
			foreach (var j in jobsProcessing)
			{
				BackgroundJob.Delete(j.Key);
			}

			var jobsScheduled = monitor.ScheduledJobs(0, int.MaxValue);
			foreach (var j in jobsScheduled)
			{
				BackgroundJob.Delete(j.Key);
			}

			var jobsEnqueued = monitor.EnqueuedJobs("default", 0, int.MaxValue);
			foreach (var j in jobsEnqueued)
			{
				BackgroundJob.Delete(j.Key);
			}
		}

		public static void ConfigureJobs(IConfiguration configuration)
		{
			DeleteExistingJobs();

			var oneCTimetable = configuration.GetValue<string>("Settings:Jobs:1C:Timetable")?.ToString();
			RecurringJob.AddOrUpdate<OneCImportJob>(Const.Import1CJobName, x => x.Import(), oneCTimetable, queue: "default");

			var wbParsePriceTimetble = configuration.GetValue<string>("Settings:Jobs:WbParsePrices:Timetable")?.ToString();
			RecurringJob.AddOrUpdate<WbParsePricesJob>(
					Const.JobName.WbMarketplaceParse,
					x => x.GetMarketInfo(),
					wbParsePriceTimetble,
					queue: "default");

			var ozonParsePriceTimetble = configuration.GetValue<string>("Settings:Jobs:OzonParsePrices:Timetable")?.ToString();
			RecurringJob.AddOrUpdate<OzonParsePricesJob>(
				Const.JobName.OzonMarketplaceParse,
				x => x.GetMarketInfo(),
				ozonParsePriceTimetble,
				queue: "default");

			var clearWhJournalNotebooks = configuration.GetValue<string>("Settings:Jobs:ClearWhJournal:Timetable")?.ToString();
			RecurringJob.AddOrUpdate<ClearWhJournalJob>(
				"ClearWhJournalNotebooks",
				x => x.ClearWhJournal(WhJournalObjectType.Notebook),
				clearWhJournalNotebooks,
				queue: "default");

			var timetable = configuration.GetValue<string>("Settings:Jobs:OzonAPIGetPrices:Timetable")?.ToString();
			RecurringJob.AddOrUpdate<OzonApiPriceImportJob>(
				Const.JobName.OzonApiPriceImportJob,
				x => x.Import(),
				timetable,
				queue: "default");


			timetable = configuration.GetValue<string>("Settings:Jobs:WbSales:Timetable")?.ToString();
			RecurringJob.AddOrUpdate<WbSalesImportJob>(
				Const.JobName.WbSalesApiImportJob,
				x => x.Import(),
				timetable,
				queue: "default");

			timetable = configuration.GetValue<string>("Settings:Jobs:WbOrders:Timetable")?.ToString();
			RecurringJob.AddOrUpdate<WbOrdersImportJob>(
				Const.JobName.WbOrdersApiImportJob,
				x => x.Import(),
				timetable,
				queue: "default");
		}
	}
}
