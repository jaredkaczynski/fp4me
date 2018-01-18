using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using fp4me.Web.Data;
using fp4me.Web.Models;
using fp4me.Web.Services;
using fp4me.Web.Controllers;
using Disney;
using Helpers.Services;

namespace fp4me.Web
{
    public class Startup
    {
        public IHostingEnvironment CurrentHostingEnvironment { get; set; }
        
        public Startup(IHostingEnvironment hostingEnvironment)
        {
            CurrentHostingEnvironment = hostingEnvironment;

            var builder = new ConfigurationBuilder()
                .SetBasePath(hostingEnvironment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{hostingEnvironment.EnvironmentName}.json", optional: true)
                //.AddJsonFile($"customSettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            if (hostingEnvironment.IsDevelopment())
            {
                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DataContext>(options =>
                options.UseMySql(Configuration.GetConnectionString("Default")));

            services.AddApplicationInsightsTelemetry(Configuration);

            services.Configure<TwilioSettings>(options => Configuration.GetSection("Twilio").Bind(options));
            services.Configure<DisneyAPISettings>(options => Configuration.GetSection("DisneyAPI").Bind(options));
            services.Configure<AppSettings>(options => Configuration.GetSection("fp4me").Bind(options));
            services.Configure<EmailSenderSettings>(options => Configuration.GetSection("EmailSender").Bind(options));

            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<DataContext, DataContext>();
            services.AddTransient<FacadeContext, FacadeContext>();

            if (CurrentHostingEnvironment.IsProduction())
            {
                services.AddTransient<ITextSender, TwilioTextSender>();
                services.AddTransient<IDisneyAPI, DisneyAPI>();
            }
            else
            {
                services.AddTransient<ITextSender, DoNothingTextSender>();
                services.AddTransient<IDisneyAPI, DisneyAPITester>();
            }
                
            services.AddScoped<IViewRenderService, ViewRenderServiceUsingHttpContext>();

            services.AddMvc().AddJsonOptions(options => {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            }); ;

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseSimpleUserIdentity();
            app.UseUserTimezoneOffset();

            app.UseMvc(routes =>
            {

                routes.MapRoute(
                    name: "authenticate",
                    template: "a/{accessToken}",
                    defaults: new { controller = nameof(UserController).Replace("Controller", ""), action = nameof(UserController.Authenticate) }
                    );

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Home}/{id?}");

            });
        }
    }
}
