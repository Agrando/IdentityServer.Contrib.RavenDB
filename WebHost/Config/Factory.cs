using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identityserver.Contrib.RavenDB;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;

namespace WebHost.Config
{
    class Factory
    {
        public static IdentityServerServiceFactory Configure()
        {
            var efConfig = new RavenDbServiceOptions("Raven");

            // these two calls just pre-populate the test DB from the in-memory config
            ConfigureClients(Clients.Get(), efConfig);
            ConfigureScopes(Scopes.Get(), efConfig);

            var factory = new IdentityServerServiceFactory();

            factory.RegisterConfigurationServices(efConfig);
            factory.RegisterOperationalServices(efConfig);

            factory.UseInMemoryUsers(Users.Get());

            return factory;
        }

        public static void ConfigureClients(IEnumerable<Client> clients, RavenDbServiceOptions options)
        {
            using (var s = options.Store.OpenSession())
            {
                foreach (var client in clients)
                {
                    var toSave = Identityserver.Contrib.RavenDB.Data.StoredClient.ToDbFormat(client);
                    s.Store(toSave);
                }
                s.SaveChanges();
            }
        }

        public static void ConfigureScopes(IEnumerable<Scope> scopes, RavenDbServiceOptions options)
        {
            using (var s = options.Store.OpenSession())
            {
                foreach (var scope in scopes)
                {
                    var toSave = Identityserver.Contrib.RavenDB.Data.StoredScope.ToDbFormat(scope);
                    s.Store(toSave);
                }
                s.SaveChanges();
            }
        }
    }
}