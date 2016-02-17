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
        private readonly Raven.Client.IDocumentStore _store;

        public ScopeStore(Raven.Client.IDocumentStore store)
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

                foreach (var scope in storedScopes)
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
                var scopesReader = await s.Advanced.StreamAsync<Data.StoredScope>("scopes/");
                while (await scopesReader.MoveNextAsync())
                {
                    var thisOne = scopesReader.Current.Document;
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
