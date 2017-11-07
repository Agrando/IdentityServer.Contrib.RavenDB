using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using Raven.Client;

namespace Identityserver.Contrib.RavenDB
{
    public class AuthorizationCodeStore : IAuthorizationCodeStore
    {
        private readonly IDocumentStore _store;
        private readonly IScopeStore _scopeStore;
        private readonly IClientStore _clientStore;
        public AuthorizationCodeStore(IDocumentStore store, IScopeStore scopeStore, IClientStore clientStore)
        {
            _store = store;
            _scopeStore = scopeStore;
            _clientStore = clientStore;
        }

        public async Task StoreAsync(string key, AuthorizationCode value)
        {
            using (var s = _store.OpenAsyncSession())
            using (s.Advanced.DocumentStore.AggressivelyCache())
            {
                var toSave = Data.StoredAuthorizationCode.ToDbFormat(key, value);
                await s.StoreAsync(toSave);
                await s.SaveChangesAsync();
            }
        }

        public async Task<AuthorizationCode> GetAsync(string key)
        {
            using (var s = _store.OpenAsyncSession())
            using (s.Advanced.DocumentStore.AggressivelyCache())
            {
                var loaded = await s.LoadAsync<Data.StoredAuthorizationCode>("authorizationcodes/" + key);
                if (loaded == null)
                    return null;

                return await Data.StoredAuthorizationCode.FromDbFormat(loaded, _clientStore, _scopeStore);
            }
        }

        public async Task RemoveAsync(string key)
        {
            using (var s = _store.OpenAsyncSession())
            using (s.Advanced.DocumentStore.AggressivelyCache())
            {
                s.Delete("authorizationcodes/" + key);
                await s.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ITokenMetadata>> GetAllAsync(string subject)
        {
            var result = new List<AuthorizationCode>();

            using (var s = _store.OpenAsyncSession())
            using (s.Advanced.DocumentStore.AggressivelyCache())
            {
                var q = s.Query<Data.StoredAuthorizationCode, Indexes.AuthorizationCodeIndex>().Where(x => x.SubjectId == subject);
                var loaded = await q.Take(int.MaxValue).ToListAsync();

                foreach(var thisOne in loaded)
                {
                    result.Add(await Data.StoredAuthorizationCode.FromDbFormat(thisOne, _clientStore, _scopeStore));
                }
            }

            return result;
        }

        public async Task RevokeAsync(string subject, string client)
        {
            using (var s = _store.OpenAsyncSession())
            using (s.Advanced.DocumentStore.AggressivelyCache())
            {
                var q = s.Query<Data.StoredAuthorizationCode, Indexes.AuthorizationCodeIndex>().Where(x => x.ClientId == client && x.SubjectId == subject);
                var loaded = await q.Take(int.MaxValue).ToListAsync();

                foreach(var thisOne in loaded)
                {
                    s.Delete(thisOne.Id);
                }

                await s.SaveChangesAsync();
            }
        }
    }
}
