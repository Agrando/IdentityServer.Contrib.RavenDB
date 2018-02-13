using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services;
using Raven.Client;

namespace Identityserver.Contrib.RavenDB.Services
{
    public class ClientConfigurationCorsPolicyService : ICorsPolicyService
    {
        private readonly IDocumentSession s;
        public ClientConfigurationCorsPolicyService(SessionWrapper session)
        {
            s = session.Session;
        }

        public Task<bool> IsOriginAllowedAsync(string origin)
        {
            var q = s.Advanced.DocumentQuery<Data.StoredClient, Indexes.CorsIndex>().WhereEquals("AllowedOrigin", origin);
            var count = q.CountLazily();
            return Task.FromResult(count.Value > 0);
        }
    }
}
