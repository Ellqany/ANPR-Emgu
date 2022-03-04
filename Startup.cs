using ANPRCV.AppServices.Repository;
using ANPRCV.AppServices.Repository.Concreate;
using ANPRCV.AppServices.Service;
using ANPRCV.AppServices.Service.Concreate;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.Tasks;

namespace ANPRCV
{
    public class Startup
    {
        readonly string AllowAll = "allowAllOrigin";
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration) => Configuration = configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Inject The repository in the controller
            services = AddServices(services);

            // Authenticate the user using custom authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration["JwtSecurityToken:Issuer"],
                    ValidAudience = Configuration["JwtSecurityToken:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtSecurityToken:Key"]))
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        if (string.IsNullOrEmpty(accessToken) == false)
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            // Allowed CORS
            services.AddCors(options =>
            {
                options.AddPolicy(AllowAll, builder =>
                    builder.SetIsOriginAllowed((host) => true).AllowAnyHeader()
                                                              .AllowAnyMethod().AllowCredentials());
            });

            //This allow url to type is lower case
            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

            services.AddControllersWithViews().AddNewtonsoftJson(options =>
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            );

            services.AddMemoryCache();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseExceptionHandler("/error");
            app.UseStatusCodePagesWithReExecute("/error");
            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseCors(AllowAll);

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "error",
                    pattern: "{target:regex(error)}/{*catchall}",
                    defaults: new { controller = "Home", action = "error" });
            });
        }

        #region Private Methods
        static IServiceCollection AddServices(IServiceCollection services)
        {
            services.AddSingleton<IDetectPlatesRepository, DetectPlatesRepository>();
            services.AddSingleton<IDetectCharsRepository, DetectCharsRepository>();
            services.AddSingleton<IPreprocessRepository, PreprocessRepository>();

            services.AddTransient<IANPRService, ANPRService>();
            services.AddTransient<IOCRService, OCRService>();
            return services;
        }
        #endregion
    }
}
