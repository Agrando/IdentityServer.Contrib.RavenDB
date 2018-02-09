using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using Raven.Client;
using Raven.Client.Documents;

namespace Identityserver.Contrib.RavenDB
{
    public class ClientStore : IClientStore
    {
        private readonly IDocumentStore _store;
        public ClientStore(IDocumentStore store)
        {
            _store = store;
        }

        public async Task<Client> FindClientByIdAsync(string clientId)
        {
            using (var s = _store.OpenAsyncSession())
             
            {
                var loaded = await s.LoadAsync<Data.StoredClient>("clients/" + clientId);
                if (loaded == null)
                    return null;

                return Data.StoredClient.FromDbFormat(loaded);
            }
        }
    }
}
