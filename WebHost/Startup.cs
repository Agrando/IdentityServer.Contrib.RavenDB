using System;
using System.Threading.Tasks;
using IdentityServer3.Core.Configuration;
using Microsoft.Owin;
using Owin;
using WebHost.Config;

[assembly: OwinStartup(typeof(WebHost.Startup))]

namespace WebHost
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var options = new IdentityServerOptions
            {
                SiteName = "IdentityServer3 (RavenDB)",
                SigningCertificate = Certificate.Get(),
                Factory = Factory.Configure()
            };

            appBuilder.UseIdentityServer(options);
        }
    }
}
