using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogAPI.CustomFormatters;
using CatalogAPI.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;

namespace CatalogAPI
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
            services.AddScoped<CatalogContext>();

            services.AddCors(c=>
            {
                //c.AddDefaultPolicy(x => x.AllowAnyOrigin()
                //.AllowAnyMethod()
                //.AllowAnyHeader());

                c.AddPolicy("AllowPartners", x =>
                {
                    x.WithOrigins("http://microsoft.com", "http://synergetics.com")
                    .WithMethods("GET", "POST")
                    .AllowAnyHeader(); 
                });
                c.AddPolicy("AllowAll", x =>
                {
                    x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info
                {
                    Title="Catalog API",
                    Description="Catalog management API methods for Eshop application",
                    Version="1.0",
                    Contact=new Contact
                    {
                        Name="Sonu Sathyadas",
                        Email="sonusathyadas@hotmail.com",
                        Url="https://github.com/sonusathyadas"
                    }
                });
            });

            services.AddAuthentication(c =>
                {
                    c.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    c.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(c =>
                {
                    c.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience=true,
                        ValidateIssuer= true,
                        ValidateLifetime=true,
                        ValidateIssuerSigningKey=true,
                        ValidIssuer=Configuration.GetValue<string>("Jwt:issuer"),
                        ValidAudience=Configuration.GetValue<string>("Jwt:audience"),
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetValue<string>("Jwt:secret")))
                    };
                });                

            services.AddMvc(options=>
                {
                    options.OutputFormatters.Add(new CsvOutputFormatter());
                })
                .AddXmlDataContractSerializerFormatters()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseCors("AllowAll");
            
            app.UseSwagger(); // http://localhost:5000/swagger/v1/swagger.json

            if (env.IsDevelopment())
            {
                app.UseSwaggerUI(config =>
                {
                    config.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog API");
                    config.RoutePrefix = "";
                });
            }
            
            app.UseFileServer(new FileServerOptions()
            {
                RequestPath="/images",
                FileProvider=new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Images")),
                EnableDirectoryBrowsing=true
            });

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
