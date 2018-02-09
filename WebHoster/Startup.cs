using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityAdmin.Configuration;
using IdentityAdmin.Core;
using IdentityAdmin.Extensions;
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
            //var logStore = new Raven.Client.Documents.DocumentStore() { ConnectionStringName = "Raven" }.Initialize();

            Log.Logger = new LoggerConfiguration().MinimumLevel.Warning()
                //.WriteTo.ColoredConsole(outputTemplate: "{Timestamp:HH:MM} [{Level}] ({Name:l}){NewLine} {Message}{NewLine}{Exception}")
                //.WriteTo.RavenDB(logStore, errorExpiration: System.Threading.Timeout.InfiniteTimeSpan, expiration: TimeSpan.FromHours(2))
                .CreateLogger();
            Log.Logger.Debug("Go");

            var options = new IdentityServerOptions
            {
                SiteName = "IdentityServer3 (RavenDB)",
                SigningCertificate = Certificate.Get(),
                Factory = Factory.Configure(),
                RequireSsl = false
            };

            #region TEST
            app.UseCookieAuthentication(new Microsoft.Owin.Security.Cookies.CookieAuthenticationOptions { AuthenticationType = "Cookies", });

            app.UseOpenIdConnectAuthentication(new Microsoft.Owin.Security.OpenIdConnect.OpenIdConnectAuthenticationOptions
            {
                AuthenticationType = "oidc",
                Authority = "http://localhost:9921/core",
                ClientId = "idAdmin",
                RedirectUri = "http://localhost:9921",
                ResponseType = "id_token",
                UseTokenLifetime = false,
                Scope = "openid idAdmin",
                SignInAsAuthenticationType = "Cookies",
                Notifications = new Microsoft.Owin.Security.OpenIdConnect.OpenIdConnectAuthenticationNotifications
                {
                    SecurityTokenValidated = n =>
                    {
                        n.AuthenticationTicket.Identity.AddClaim(new Claim("id_token", n.ProtocolMessage.IdToken));
                        return Task.FromResult(0);
                    },
                    RedirectToIdentityProvider = async n =>
                    {
                        if (n.ProtocolMessage.RequestType == Microsoft.IdentityModel.Protocols.OpenIdConnectRequestType.LogoutRequest)
                        {
                            var result = await n.OwinContext.Authentication.AuthenticateAsync("Cookies");
                            if (result != null)
                            {
                                var id_token = result.Identity.Claims.GetValue("id_token");
                                if (id_token != null)
                                {
                                    n.ProtocolMessage.IdTokenHint = id_token;
                                    n.ProtocolMessage.PostLogoutRedirectUri = "http://localhost:9921/admin";
                                }
                            }
                        }
                    }
                }
            });
            #endregion

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
                Factory = adminFactory,
                AdminSecurityConfiguration = new AdminHostSecurityConfiguration { HostAuthenticationType = "Cookies" }
            };
            adminOptions.AdminSecurityConfiguration.RequireSsl = false;

            app.Map("/admin", adminApp =>
            {
                adminApp.UseIdentityAdmin(adminOptions);
            });
        }
    }
}
