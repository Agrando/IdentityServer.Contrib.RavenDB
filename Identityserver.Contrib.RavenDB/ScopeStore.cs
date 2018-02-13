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
    public class ScopeStore : IScopeStore
    {
        private readonly IDocumentSession s;

        public ScopeStore(SessionWrapper session)
        {
            s = session.Session;
        }

        public Task<IEnumerable<Scope>> FindScopesAsync(IEnumerable<string> scopeNames)
        {
            var result = new List<Scope>();

            using (s.Advanced.DocumentStore.AggressivelyCache())
            {
                var ids = from scopeName in scopeNames select "scopes/" + scopeName;
                var storedScopes = s.Load<Data.StoredScope>(ids);

                foreach (var scope in storedScopes.Where(x => x != null))
                {
                    result.Add(Data.StoredScope.FromDbFormat(scope));
                }

                return Task.FromResult((IEnumerable<Scope>) result);
            }
        }

        public Task<IEnumerable<Scope>> GetScopesAsync(bool publicOnly = true)
        {
            var result = new List<Scope>();

            using (s.Advanced.DocumentStore.AggressivelyCache())
            {
                var loadedScopes = s.Advanced.LoadStartingWith<Data.StoredScope>("scopes/", pageSize: int.MaxValue);

                foreach (var thisOne in loadedScopes)
                {
                    if (publicOnly == false || thisOne.ShowInDiscoveryDocument)
                    {
                        result.Add(Data.StoredScope.FromDbFormat(thisOne));
                    }
                }

                return Task.FromResult((IEnumerable<Scope>) result);
            }
        }
    }
}
