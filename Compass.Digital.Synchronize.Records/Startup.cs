using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compass.Digital.Core;
using Compass.Digital.GateKeeper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;

namespace Compass.Digital.Synchronize.Records
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IOptions<AppSettings> appSettings)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddCors(options =>
            //{
            //    options.AddPolicy("AllowedOrigins", policy => policy.WithOrigins("https://localhost:44391"));
            //});

            services.AddMvc(options =>
            {
                //options.Filters.Add(new ValidateModelStateAttribute());
                options.Filters.Add(new RequestValidationAttribute());
                //options.Filters.Add(new ValidateIPAddressAttribute());
            }).AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddApiVersioning(o =>
            {
                o.ReportApiVersions = true;
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(2, 0);
                o.ApiVersionReader = new HeaderApiVersionReader("x-apiVersion");
                //o.Conventions.Controller<ValuesV1Controller>().HasApiVersion(new ApiVersion(1, 0));
            });

            services.AddMemoryCache();
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.AddSingleton(typeof(ILog<>), typeof(CompassLog<>));
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            var key = Encoding.ASCII.GetBytes(Configuration.GetSection("AppSettings")["JwtSecret"]);
            var encKey = Encoding.ASCII.GetBytes(Configuration.GetSection("AppSettings")["JwtSecret"]);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    TokenDecryptionKey = new SymmetricSecurityKey(encKey),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

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

            //app.UseCors("AllowedOrigins");
            app.UseMiddleware<LoggerMiddleware>();
            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseMvcWithDefaultRoute();
            app.UseStaticFiles();
            if (env.IsDevelopment())
            {
                app.UseStaticFiles(new StaticFileOptions()
                {
                    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot")),
                    RequestPath = new PathString("/libs")
                });

            }
            else
            {
                var webRoot = env.WebRootPath;
                app.UseStaticFiles(new StaticFileOptions()
                {
                    FileProvider = new PhysicalFileProvider(Path.Combine("", @"C://inetpub/wwwroot/lib")),
                });
            }

        }
    }
}
