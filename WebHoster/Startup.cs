using IdentityServer3.Core.Configuration;
using Microsoft.Owin;
using Owin;
using WebHost.Config;

[assembly: OwinStartupAttribute(typeof(WebHoster.Startup))]
namespace WebHoster
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
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
        }
    }
}
