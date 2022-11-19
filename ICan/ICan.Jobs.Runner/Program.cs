using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using System;

namespace ICan.Jobs.Runner
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
			var nLogFileName = environment.Equals("Development") ? "nlog.config" : $"nlog.{environment}.config";

			var logger =NLogBuilder.ConfigureNLog(nLogFileName).GetCurrentClassLogger();

			try
			{
				CreateHostBuilder(args).Build().Run();
			}
			catch (Exception ex)
			{
				//NLog: catch setup errors
				logger.Error(ex, "Stopped program because of exception");
				throw;
			}
			finally
			{
				// Ensure to flush and stop public timers/threads before application-exit (Avoid segmentation fault on Linux)
				NLog.LogManager.Shutdown();
			}
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.ConfigureKestrel(serverOptions =>
						serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromHours(24))
					 .UseStartup<Startup>();
				})
				.ConfigureAppConfiguration((ctx, builder) =>
				{
					builder.AddJsonFile("appsettings.json", false, true);
					builder.AddJsonFile($"appsettings.{Environment.MachineName}.json", true, true);
					builder.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true, true);
				})
				.ConfigureLogging(logging =>
				{
					logging.ClearProviders();
					logging.SetMinimumLevel(LogLevel.Warning);
					//logging.AddConsole();
				})
				.UseNLog();
	}
}
