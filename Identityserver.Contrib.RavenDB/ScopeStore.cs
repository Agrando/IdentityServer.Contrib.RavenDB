using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;

namespace Identityserver.Contrib.RavenDB
{
    public class ScopeStore : IScopeStore
    {
        private readonly Raven.Client.Documents.IDocumentStore _store;

        public ScopeStore(Raven.Client.Documents.IDocumentStore store)
        {
            _store = store;
        }

        public async Task<IEnumerable<Scope>> FindScopesAsync(IEnumerable<string> scopeNames)
        {
            var result = new List<Scope>();

            using (var s = _store.OpenAsyncSession())
             
            {
                var ids = from scopeName in scopeNames select "scopes/" + scopeName;
                var storedScopes = await s.LoadAsync<Data.StoredScope>(ids);

                foreach (var scope in storedScopes.Values.Where(x => x != null))
                {
                    result.Add(Data.StoredScope.FromDbFormat(scope));
                }
            }

            return result;
        }

        public async Task<IEnumerable<Scope>> GetScopesAsync(bool publicOnly = true)
        {
            var result = new List<Scope>();

            using (var s = _store.OpenAsyncSession())
             
            {
                var loadedScopes = await s.Advanced.LoadStartingWithAsync<Data.StoredScope>("scopes/", pageSize: int.MaxValue);
                
                foreach(var thisOne in loadedScopes)
                {
                    if (publicOnly == false || thisOne.ShowInDiscoveryDocument)
                    {
                        result.Add(Data.StoredScope.FromDbFormat(thisOne));
                    }
                }
            }

            return result;
        }
    }
}
