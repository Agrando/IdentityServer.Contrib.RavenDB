using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using Raven.Client;

namespace Identityserver.Contrib.RavenDB
{
    public class ClientStore : IClientStore
    {
        private readonly IDocumentSession s;
        public ClientStore(SessionWrapper session)
        {
            s = session.Session;
        }

        public Task<Client> FindClientByIdAsync(string clientId)
        {
            if (clientId == null)
                return Task.FromResult<Client>(null);

            using (s.Advanced.DocumentStore.AggressivelyCache())
            {
                var loaded = s.Load<Data.StoredClient>("clients/" + clientId);
                if (loaded == null)
                    return Task.FromResult<Client>(null);

                return Task.FromResult(Data.StoredClient.FromDbFormat(loaded));
            }
        }
    }
}
