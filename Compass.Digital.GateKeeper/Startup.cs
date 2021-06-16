using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NLog;
using System.IO;
using Compass.Digital.Core;
namespace Compass.Digital.GateKeeper
{
    public class Startup
    {

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            LogManager.LoadConfiguration(System.String.Concat(env.ContentRootPath, "\\NLog.config"));
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowedOrigins", policy => policy.WithOrigins("https://localhost:44391"));
            });

            services.AddMemoryCache();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddApiVersioning(o =>
            {
                o.ReportApiVersions = true;
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(1, 0);
                o.ApiVersionReader = new HeaderApiVersionReader("x-apiVersion");
            });

            //services.AddHealthChecks()
            //    .AddCheck<HttpHealthCheck>("http")
            //    .AddCheck<SqlServerHealthCheck>("sql");
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.AddSingleton(typeof(ILog<>), typeof(CompassLog<>));
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            //app.UseMiddleware<ThrottlingMiddleware>();
            app.UseCors("AllowedOrigins");
            //app.ConfigureExceptionHandler(logger);
            //env.ConfigureNLog("nlog.config");
            app.UseHttpsRedirection();
            app.UseMvc();
            
        }
    }
}
