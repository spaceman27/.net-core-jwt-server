using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.AspNetCore.Swagger;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Rewrite;
using System.Diagnostics.CodeAnalysis;
using Swashbuckle.AspNetCore.SwaggerUI;
using Common.Standard.REST.Interfaces.Security;
using Common.Standard.REST.Interfaces.API;
using Vision.Core.REST.Auth.DTO;
using Common.Standard.REST.Base.Security;
using Core.REST.APV.DataLayer.RepoUOW;
using Common.Core.EFCore.AirportVision.Models;
using Common.Standard.REST.Base.DataModel;
using Common.Standard.REST.Base.Extensions;
using Vision.Core.REST.Auth.Service;
using Vision.Core.REST.Auth.Data;
using Common.Standard.REST.Interfaces.DataModel;
using Microsoft.AspNetCore.Http;
using Common.Standard.REST.Base.Services;
using System.Data.SqlClient;

namespace ComNet.AirportVision.REST.Auth
{
    [ExcludeFromCodeCoverage]
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

            string connectionString = Configuration["ConnectionStrings:APVConnectionString"];
            string secretKey = Configuration["RESTTokenSettings:Secret"];
            AESEncryption encryption = new AESEncryption(
                    new byte[] { 123, 217, 19, 11, 14, 26, 85, 45, 114, 184, 27, 142, 37, 112, 222, 209, 241, 24, 175, 144, 173, 53, 196, 29, 24, 26, 17, 218, 131, 236, 53, 209 },
                    new byte[] { 146, 64, 191, 121, 23, 13, 113, 119, 231, 121, 251, 112, 79, 32, 114, 156 });
            try
            {
                connectionString = encryption.Decrypt(connectionString);
            }
            catch
            {
                connectionString = Configuration["ConnectionStrings:APVConnectionString"];
            }

            

            var dbContextOptions = new DbContextOptionsBuilder();
            dbContextOptions.UseSqlServer(connectionString);

            using (var db = new AirportVisionDBContext(dbContextOptions.Options))
            {

                var secretOption = db.Set<SystemOptions>().FirstOrDefault(s => s.OptionName.ToLower() == "restsecretkey");
                if (secretOption == null)
                {
                    secretOption = new SystemOptions
                    {
                        Description = "REST Secret Key",
                        OptionName = "RESTSECRETKEY",
                        Value = secretKey,
                        Source = "System",
                        OptionGroup = "System"
                    };
                    db.Set<SystemOptions>().Add(secretOption);
                }

                Configuration["RESTTokenSettings:Secret"] = secretOption.Value;
                db.SaveChanges();
            }

            services.AddCommonREST<AirportVisionDBContext, APVUnitOfWork>(opt =>
            {
                opt.DbContextOptionsBuilder = bld => bld.UseSqlServer(connectionString);
                opt.JWTSigningKeyResolver = (these, are, all, pointless) =>
                {
                    // This gets called a lot, so avoid the overhead of the DB Context.
                    using (var conn = new SqlConnection(connectionString))
                    {
                        using (var comm = conn.CreateCommand())
                        {
                            try
                            {
                                SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["RESTTokenSettings:Secret"]));

                                conn.Close();
                                return new List<SecurityKey> { key };
                            }
                            catch
                            {
                                throw new APVRESTException("Authentication server not running.");
                            }
                        }
                    }
                };
                opt.TokenAudience = Configuration["RESTTokenSettings:Audience"];
                opt.TokenIssuer = Configuration["RESTTokenSettings:Issuer"];
            });

            // Data Layer Interfaces
            services.AddDbContext<AirportVisionDBContext>(options => options.UseSqlServer(connectionString), ServiceLifetime.Scoped);

            services.AddApiImplementation<UsersDTO, Users, UsersService, UsersProjector, UsersRepository>();

            services.AddScoped<IProjector<UserOptions, UserOptionsDTO>, UserOptionsProjector>();
            services.AddScoped<IGenericRepository<UserOptionsDTO>, UserOptionsRepository>();
            services.AddScoped<IProjector<Sites, SiteDTO>, SiteProjector>();
            services.AddScoped<IGenericRepository<SiteDTO>, SitesRepository>();
            services.AddScoped<ILoginHandler<UsersDTO>, LoginHandler>();
            services.AddScoped<IEncryption<string, string>>(s => encryption);

            services.AddScoped<IDictionary<Type, dynamic>>(s =>
            {
                return new Dictionary<Type, dynamic>
                {
                    {typeof(UsersDTO), s.GetService<IGenericRepository<UsersDTO>>() },
                    {typeof(UserOptionsDTO), s.GetService<IGenericRepository<UserOptionsDTO>>() },
                    {typeof(SiteDTO), s.GetService<IGenericRepository<SiteDTO>>() },
                };
            });

            services.AddMvc();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }
        
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseCommonRest(loggerFactory);
            
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "v1/api/{controller=Home}/{id?}");
            });            
        }
    }
}
