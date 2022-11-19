using Hangfire;
using Hangfire.Dashboard;
using Hangfire.MySql;
using ICan.Business;
using ICan.Business.Services;
using ICan.Common;
using ICan.Common.Models;
using ICan.Data.Configuration;
using ICan.Data.Context;
using ICan.DomainModel.Jobs;
using ICan.Jobs.OneC;
using ICan.Jobs.Runner.Extensions;
using ICan.Jobs.WB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ICan.Jobs.Runner
{
    public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton(Configuration);
			services.AddControllers();
			services.AddDbContext<ApplicationDbContext>(options =>
				options.UseMySql(Configuration.GetConnectionString("MySQLConnection"),
				ServerVersion.AutoDetect(Configuration.GetConnectionString("MySQLConnection")))
				//.EnableSensitiveDataLogging()
				);
			services.AddIdentity<ApplicationUser, IdentityRole>()
				.AddEntityFrameworkStores<ApplicationDbContext>()
				.AddDefaultTokenProviders();

			services.AddHttpClient();
			services.ConfigureApplicationCookie(options => options.LoginPath = "/Auth");
			services.AddAuthentication()
				.AddCookie(options =>
				{
					options.LoginPath = "/Auth";
					options.ExpireTimeSpan = TimeSpan.FromDays(30);
				})
				.AddJwtBearer(options =>
				{
					options.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuerSigningKey = true,
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Settings:TokenKey"])),
						ValidateAudience = false,
						ValidateIssuer = false,
						RequireExpirationTime = true,
						ValidateLifetime = true
					};
				});

			string folder = Configuration.GetValue<string>("Settings:KeyFolder")?.ToString();
			if (!string.IsNullOrEmpty(folder))
				services.AddDataProtection()
			  .PersistKeysToFileSystem(new DirectoryInfo(folder));
			services.AddTransient<IEmailSender, EmailSender>();
			services.AddHttpClient();
			services.AddScoped<UnisenderClient>();
			services.AddScoped<IEmailService, UnisenderService>();
			ManagerRegistrator.Register(services);
			services.ConfigureDal(Configuration, null);
			services.AddAutoMapper(typeof(MapperProfile));
			services.AddRazorPages();
			AddJobs(services);
			GetOptionsFromConfig(services);
			ConfigureHangFire(services);
			services.AddScoped<JwtGeneratorService>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}
			app.UseHttpsRedirection();
			app.UseStaticFiles();
			var path = Path.Combine(env.ContentRootPath, ".well-known/acme-challenge/");
			if (Directory.Exists(path))
			{
				app.UseStaticFiles(new StaticFileOptions
				{
					FileProvider = new PhysicalFileProvider(
					path),
					RequestPath = "/.well-known/acme-challenge",
					ServeUnknownFileTypes = true,
					DefaultContentType = "text/plain"
				});
			}
			app.UseRouting();
			app.UseAuthentication();
			app.UseAuthorization();

			app.UseHangfireDashboard("/hangfire", new DashboardOptions
			{
				Authorization = new List<IDashboardAuthorizationFilter> { { new HangfireAuthorizationFilter() } },
			});

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "auth",
					pattern: "Auth/{controller=Auth}/{action=Index}/{id?}");

				endpoints.MapControllerRoute(
					name:"default",
					pattern:"{controller}=Auth/{action}=Login/{id?}"
				);

				endpoints.MapControllers();
			});

			JobsConfigurator.ConfigureJobs(Configuration);
		}

		private void ConfigureHangFire(IServiceCollection services)
		{
			//Add Hangfire services.
			services.AddHangfire(configuration => configuration
			   .UseNLogLogProvider()
			   .UseStorage(new MySqlStorage(Configuration.GetConnectionString("MySQLConnection"), new MySqlStorageOptions
			   {
				   TransactionIsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
				   PrepareSchemaIfNecessary = true,
				   TablesPrefix = "Hangfire_",
				   DashboardJobListLimit = 100
			   })));

			services.AddHangfireServer(options =>
			{
				options.Queues = new[] { "default" };
				options.WorkerCount = 1;
			});
		}

		private void GetOptionsFromConfig(IServiceCollection services)
		{
			try
			{
				var opts = new WbCheckWarehouseSetting();
				var config = Configuration.GetSection("Settings:Jobs:WbCheckWarehouse");
				config.Bind(opts);
				services.AddSingleton(opts);
			}
			catch (Exception ex)
			{

			}
		}

		private void AddJobs(IServiceCollection services)
		{
			services.AddScoped<OneCImportJob, OneCImportJob>();
			services.AddScoped<WbOrdersImportJob, WbOrdersImportJob>();
			services.AddScoped<WbSalesImportJob, WbSalesImportJob>();
		}
	}
}
