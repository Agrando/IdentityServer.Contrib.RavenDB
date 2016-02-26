using System.Collections.Generic;
using IdentityAdmin.Configuration;
using IdentityAdmin.Core;
using Identityserver.Contrib.RavenDB.Services;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Logging;
using Microsoft.Owin;
using Owin;
using Serilog;
using WebHost.Config;

[assembly: OwinStartupAttribute(typeof(WebHoster.Startup))]
namespace WebHoster
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Trace(outputTemplate: "{Timestamp} [{Level}] ({Name}){NewLine} {Message}{NewLine}{Exception}")
                .CreateLogger();

            var options = new IdentityServerOptions
            {
                SiteName = "IdentityServer3 (RavenDB)",
                SigningCertificate = Certificate.Get(),
                Factory = Factory.Configure(),
                RequireSsl = false
            };

            app.Map(
                "/core",
                coreApp =>
                {
                    coreApp.UseIdentityServer(options);
                });

            var adminFactory = new IdentityAdminServiceFactory
            {
                IdentityAdminService = new IdentityAdmin.Configuration.Registration<IIdentityAdminService>(new IdentityAdminService(Factory.RavenConfig.Store))
            };

            var adminOptions = new IdentityAdminOptions
            {
                Factory = adminFactory
            };
            adminOptions.AdminSecurityConfiguration.RequireSsl = false;

            app.Map("/admin", adminApp =>
            {
                adminApp.UseIdentityAdmin(adminOptions);
            });
        }
    }
}
