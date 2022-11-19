using ICan.Business;
using ICan.Business.Managers;
using ICan.Business.Services;
using ICan.Common;
using ICan.Common.Models;
using ICan.Common.Utils;
using ICan.Data.Configuration;
using ICan.Data.Context;
using ICan.DomainModel.Jobs;
using ICan.Extensions;
using ICan.Jobs.OneC;
using ICan.Jobs.WB;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace ICan
{
    public class Startup
    {
        public IWebHostEnvironment _environment;
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            _environment = environment;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCaching();
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            if (_environment.IsDevelopment())
            {
                services.AddRazorPages()
                    .AddRazorRuntimeCompilation();
            }
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(Configuration.GetConnectionString("MySQLConnection"),
                ServerVersion.AutoDetect(Configuration.GetConnectionString("MySQLConnection")))
                //.EnableSensitiveDataLogging()
                );

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddAntiforgery();


            AddJobs(services);
            //ConfigureBus(services);
            GetOptionsFromConfig(services);

            services.AddScoped<SetRoleAttribute, SetRoleAttribute>();
            services.AddScoped<ReportCriteriaService, ReportCriteriaService>();
            services.AddScoped<ReportParseService, ReportParseService>();
            services.AddScoped<JwtGeneratorService>();

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;

                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(120);
                //options.Lockout.MaxFailedAccessAttempts = 10;

                // User settings
                options.User.RequireUniqueEmail = true;
            });

            services
                .ConfigureApplicationCookie(options => options.LoginPath = "/Account/Login");

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
            });

            string folder = Configuration.GetValue<string>("Settings:KeyFolder")?.ToString();
            if (!string.IsNullOrEmpty(folder))
                services.AddDataProtection()
              .PersistKeysToFileSystem(new DirectoryInfo(folder));
            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddHttpClient();
            services.AddScoped<UnisenderClient>();
            services.AddScoped<IEmailService, UnisenderService>();
            services.ConfigureDal(Configuration, null);
            ManagerRegistrator.Register(services);

            //CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            //CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            services.AddAutoMapper(typeof(MapperProfile));
            SetCulture(services);
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.Configure<CloudConfiguration>(Configuration.GetSection("Cloud"));
            services.AddControllersWithViews(conf => conf.EnableEndpointRouting = true)
            .AddNewtonsoftJson(
                    options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                )
                .AddViewLocalization()
                .AddDataAnnotationsLocalization();
            services.AddOptions();
            services.Configure<UnisenderSettings>(Configuration.GetSection("Unisender"));
            services.AddHttpClient("unisender", option => 
            {
                option.BaseAddress = new Uri(Configuration["Unisender:Endpoint"]);
            });

        }

        private void SetCulture(IServiceCollection services)
        {
            var sp = services.BuildServiceProvider();

            // This will succeed.
            var siteManager = sp.GetService<SiteManager>();
            var currentSiteId = int.Parse(Configuration["Settings:CurrentSiteId"]);
            var locale = siteManager.GetLocale(currentSiteId);

            var cultureInfo = new CultureInfo(locale);

            cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
            cultureInfo.NumberFormat.CurrencyDecimalSeparator = ".";
            var supportedCultures = new List<CultureInfo>
            {
                cultureInfo,
				//CultureInfo.CurrentCulture
			};

            // Configure the Localization middleware

            services.Configure<RequestLocalizationOptions>(
             opts =>
             {
                 opts.DefaultRequestCulture = new RequestCulture(cultureInfo);
                 opts.SupportedCultures = supportedCultures;
                 opts.SupportedUICultures = supportedCultures;
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
            services.AddScoped<CheckWarehouseJob, CheckWarehouseJob>();
            services.AddScoped<SmsSenderJob, SmsSenderJob>();
        }

        private void ConfigureBus(IServiceCollection services)
        {
            try
            {
                var rmqConnectionString = Configuration["Settings:RabbitMQ:ConnectionString"];
                if (string.IsNullOrWhiteSpace(rmqConnectionString))
                    return;

                var factory = new ConnectionFactory { Uri = new Uri(rmqConnectionString) };

                var connection = factory.CreateConnection();
                var channel = connection.CreateModel();
                services.AddSingleton(channel);
            }
            catch (Exception ex)
            {
                //
            }
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment() || env.EnvironmentName == "Docker")
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            const string cacheMaxAge = "604800/7*30";
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Append(
                         "Cache-Control", $"public, max-age={cacheMaxAge}");
                },
            });
            var path = Path.Combine(env.ContentRootPath, ".well-known/acme-challenge/");
            if (Directory.Exists(path))
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(
                    path),
                    RequestPath = "/.well-known/acme-challenge"
                });
            }

            app.UseRouting();

            var options = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(options.Value);

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseResponseCaching();
            app.Use(async (context, next) =>
            {
                await next();
                var responseStatusCode = context.Response.StatusCode;
                if (responseStatusCode == StatusCodes.Status403Forbidden || responseStatusCode == StatusCodes.Status401Unauthorized)
                {
                    context.Request.Path = "/Account/AccessDenied";
                    await next();
                }
            });

            UpdateDatabase(app, logger);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "siteHomeFilter",
                    pattern: "category/{filter?}",
                    defaults: new { controller = "SiteHome", action = "Index" });

                endpoints.MapControllerRoute(
                    name: "opt",
                    pattern: "opt/{controller=Product}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "Default",
                    pattern: "{controller=SiteHome}/{action=Index}/{id?}");

                endpoints.MapFallback(context =>
                {
                    context.Response.Redirect("/Sitehome/Error");
                    return Task.CompletedTask;
                });
            });
        }


        private static void UpdateDatabase(IApplicationBuilder app, ILogger<Startup> logger)
        {
            using (var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>())
                {
                    try
                    {
                        context.Database.Migrate();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "update database error");
                    }
                }
            }
        }
    }
}
