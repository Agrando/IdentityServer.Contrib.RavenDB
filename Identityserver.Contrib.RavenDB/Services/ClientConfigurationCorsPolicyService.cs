using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Core.Services;
using Raven.Client;
using Raven.Client.Documents;

namespace Identityserver.Contrib.RavenDB.Services
{
    public class ClientConfigurationCorsPolicyService : ICorsPolicyService
    {
        private readonly IDocumentStore _store;
        public ClientConfigurationCorsPolicyService(IDocumentStore store)
        {
            _store = store;
        }

        public async Task<bool> IsOriginAllowedAsync(string origin)
        {
            using (var s = _store.OpenAsyncSession())
             
            {
                var q = s.Advanced.AsyncDocumentQuery<Data.StoredClient, Indexes.CorsIndex>().WhereEquals("AllowedOrigin", origin);
                var count = q.CountLazilyAsync();
                return await count.Value > 0;
            }
        }
    }
}
