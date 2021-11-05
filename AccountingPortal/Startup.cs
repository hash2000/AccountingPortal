using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using LibProfiles.Context;
using LibProfiles.Mapper;
using LibProfiles.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AccountingPortal
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            var propertiesPath = Path.Combine(env.ContentRootPath, "Properties");
            var builder = new ConfigurationBuilder()
               .SetBasePath(propertiesPath)
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
               .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }
        public ILifetimeScope AutofacContainer { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            var constr = Configuration.GetConnectionString("DbConnectionString");

            services.AddDbContext<ProfilesContext>(options =>
            {
                options.UseNpgsql(constr);
            });

            services.AddCors();
            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(n =>
                {
                    n.RequireHttpsMetadata = false;
                    n.TokenValidationParameters = new TokenValidationParameters
                    {
                        // укзывает, будет ли валидироваться издатель при валидации токена
                        ValidateIssuer = true,
                        // строка, представляющая издателя
                        ValidIssuer = AuthService.ISSUER,

                        // будет ли валидироваться потребитель токена
                        ValidateAudience = true,
                        // установка потребителя токена
                        ValidAudience = AuthService.AUDIENCE,
                        // будет ли валидироваться время существования
                        ValidateLifetime = true,

                        // установка ключа безопасности
                        IssuerSigningKey = AuthService.GetSymmetricSecurityKey(),
                        // валидация ключа безопасности
                        ValidateIssuerSigningKey = true,
                    };
                });

            //services.AddRazorPages();
            //services.AddControllersWithViews();

            services
                .AddControllers()
                .AddControllersAsServices()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                });
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.Register<AutoMapper.IConfigurationProvider>(ctx => new MapperConfiguration(cfg => cfg
                  .AddProfile(new MappingProfile()))
            ).SingleInstance();

            builder.Register<IMapper>(ctx =>
            {
                var scope = ctx.Resolve<ILifetimeScope>();
                var provider = ctx.Resolve<AutoMapper.IConfigurationProvider>();
                return new Mapper(provider, scope.Resolve);
            })
            .PropertiesAutowired();

            builder.RegisterAssemblyTypes(typeof(AuthService).Assembly)
               .Where(t => t.Name.EndsWith("Service"))
               .PropertiesAutowired();

            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
              .Where(t => t.Name.EndsWith("Controller"))
              .PropertiesAutowired();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            this.AutofacContainer = app.ApplicationServices.GetAutofacRoot();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
            }

            //app.UseHttpsRedirection();
            //app.UseStaticFiles();

            app.UseRouting();
            app.UseCors(options =>
            {
                options.AllowAnyOrigin();
                options.AllowAnyHeader();
                options.AllowAnyMethod();
            });

            app.UseAuthorization();
            app.UseAuthentication();

            app.UseEndpoints(endpoint =>
            {
                endpoint.MapDefaultControllerRoute();
            });
        }
    }
}
