using AutoMapper;
using Jaxx.VideoDb.WebCore;
using Jaxx.WebApi.Shared;
using Jaxx.WebApi.Shared.Identity;
using Jaxx.WebApi.Shared.Identity.Services;
using Jaxx.WebApi.Shared.Infrastructure;
using Jaxx.WebApi.Shared.Models;
using Jaxx.WebGallery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NLog.Extensions.Logging;
using NSwag;
using NSwag.Generation.Processors.Security;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Jaxx.VideoDb.WebApi
{
    public class Startup
    {

        private const string TESTENV = "UnitTests";
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile("ClientSecrets.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var appSettings = Configuration.GetSection("AppSettings");

            services.AddVideoDbMySql(Configuration);
            services.AddVideoDb(Configuration);
            services.AddGallery(Configuration);

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IUserContextInformationProvider, Jaxx.WebApi.Shared.Controllers.Infrastructure.UserContextInformationFromHttpContextProvider>();

            var identityDbConnectionString = Configuration.GetValue<string>("IdentityDbConnectionString");
            services.AddDbContext<IdentityDbContext>(options =>
            {
                options.UseMySql(identityDbConnectionString, ServerVersion.AutoDetect(identityDbConnectionString));
            });

            services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<IdentityDbContext>()
                .AddDefaultTokenProviders()
                .AddTokenProvider("HomeWebVideoDB", typeof(DataProtectorTokenProvider<IdentityUser>));

            services.Configure<IdentityOptions>(options =>
            {
                // Default Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;
            });

            services.AddRouting(options => options.LowercaseUrls = true);

            services.AddCors();

            services.AddMvc(opt =>
            {
                opt.Filters.Add(typeof(LinkRewritingFilter));
                opt.EnableEndpointRouting = false;
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
            .AddNewtonsoftJson(opt =>
            {
                // These should be the defaults, but we can be explicit:
                opt.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                opt.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                opt.SerializerSettings.DateParseHandling = DateParseHandling.DateTimeOffset;
            });


            // Register the Swagger services
            services.AddOpenApiDocument(config =>
            {
                config.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.ApiKey,
                    Name = "Authorization",
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Description = "Type into the textbox: Bearer {your JWT token}."
                });

                config.OperationProcessors.Add(
                    new AspNetCoreOperationSecurityScopeProcessor("JWT"));

                config.PostProcess = document =>
                {
                    document.Info.Version = "1.15.0";
                    document.Info.Title = "VideoDb API";
                };
            });

           

            services.AddLogging(opt =>
            {
                opt.ClearProviders();
                opt.AddConfiguration(Configuration.GetSection("Logging"));
                opt.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                opt.AddNLog();
                //opt.AddConsole();
            });

            /*services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60);
                options.Cookie.IsEssential = true;
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Lax;
            });*/

            var jwtSecret = Configuration.GetValue<string>("Jwt:Key");
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "JwtBearer";
                options.DefaultChallengeScheme = "JwtBearer";
            })
            .AddJwtBearer("JwtBearer", jwtBearerOptions =>
            {
                jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),

                    ValidateIssuer = true,
                    ValidIssuer = Configuration.GetValue<string>("Jwt:Issuer"),

                    ValidateAudience = false,
                    //ValidAudience = "The name of the audience",

                    ValidateLifetime = true, //validate the expiration and not before values in the token

                    ClockSkew = TimeSpan.FromMinutes(30) //5 minute tolerance for the expiration date
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("GalleryUser", policy => policy.RequireClaim(ClaimTypes.Role, "GalleryUser"));
                options.AddPolicy("GalleryAdmin", policy => policy.RequireClaim(ClaimTypes.Role, "GalleryAdmin"));
                options.AddPolicy("Administrator", policy => policy.RequireClaim(ClaimTypes.Role, "Administrator"));
                options.AddPolicy("VideoDbUser", policy => policy.RequireClaim(ClaimTypes.Role, "VideoDbUser"));
            });

            services.Configure<PagingOptions>(Configuration.GetSection("DefaultPagingOptions"));
            
            services.AddTransient<IIdentityService, DefaultIdentityService>();
            services.AddTransient<ITokenService, DefaultTokenService>();

            services.AddSingleton<IConfiguration>(Configuration);

            // Set UrlRewrite mode depending on configuration
            var useStaticUrlRewritingMode = Configuration.GetValue<bool>("UrlRewritingMode:UseStaticLinkRewriter");
            if (useStaticUrlRewritingMode)
            {
                var protocol = Configuration.GetValue<string>("UrlRewritingMode:Protocol");
                var url = Configuration.GetValue<string>("UrlRewritingMode:RewriteUrl");

                services.AddTransient<IRewriteConfiguration>(s => new RewriteConfiguration { Protcol = protocol, RewriteUrl = url });
                services.AddScoped<ILinkRewriter, StaticLinkRewriter>();
            }
            else services.AddScoped<ILinkRewriter, LinkRewriter>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, IMapper mapper)
        {
            //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            //loggerFactory.AddDebug();
            
            //mapper.ConfigurationProvider.AssertConfigurationIsValid();

            app.UseDefaultFiles();
            app.UseStaticFiles();
            
            var CorsConfig = Configuration.GetValue<string>("AppSettings:CorsOrigin");

            if (!string.IsNullOrWhiteSpace(CorsConfig))
            {
                app.UseCors(b => b.WithOrigins(CorsConfig).AllowAnyHeader().AllowAnyMethod());
            }
            else app.UseCors();

            // Serialize all exceptions to JSON
            var jsonExceptionMiddleware = new JsonExceptionMiddleware(
                app.ApplicationServices.GetRequiredService<IWebHostEnvironment>(),
                app.ApplicationServices.GetRequiredService<ILogger<JsonExceptionMiddleware>>());
            app.UseExceptionHandler(new ExceptionHandlerOptions { ExceptionHandler = jsonExceptionMiddleware.Invoke });

            app.UseRouting();
            //app.UseCookiePolicy();

            //https://www.codeproject.com/Articles/5160941/ASP-NET-CORE-Token-Authentication-and-Authorizatio
            //app.UseSession();
            //Add JWToken to all incoming HTTP Request Header
            /*app.Use(async (context, next) =>
            {
                var JWToken = context.Session.GetString("JWToken");
                if (!string.IsNullOrEmpty(JWToken))
                {
                    context.Request.Headers.Add("Authorization", "Bearer " + JWToken);
                }
                await next();
            });*/
            app.UseAuthentication();
            app.UseAuthorization();

            // Register the Swagger generator and the Swagger UI middlewares
            app.UseOpenApi();
            app.UseSwaggerUi3(o =>
           {
               o.CustomJavaScriptPath = new Uri("/swagger-ui/custom.js", UriKind.Relative).ToString();
           });

            var pathBase = Configuration.GetValue<string>("AppSettings:PathBase");
            app.UsePathBase(pathBase);
            app.UseMvc();

            var initialAdminPassword = Configuration.GetValue<string>("InitialAdminPassword");
            if (!string.IsNullOrWhiteSpace(initialAdminPassword))
            {
                IIdentityService identityService = app.ApplicationServices.GetRequiredService<IIdentityService>();
                identityService.InitIdentity(initialAdminPassword, new System.Threading.CancellationToken());
            }
        }

    }
}
