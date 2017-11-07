using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using Raven.Client;
using Raven.Client.Document;

namespace Identityserver.Contrib.RavenDB
{
    public class RefreshTokenStore : IRefreshTokenStore
    {
        private readonly IDocumentStore _store;
        private readonly IClientStore _clientStore;

        public RefreshTokenStore(IDocumentStore store, IClientStore clientStore)
        {
            _store = store;
            _clientStore = clientStore;
        }

        public async Task StoreAsync(string key, RefreshToken value)
        {
            using (var s = _store.OpenAsyncSession())
            using (s.Advanced.DocumentStore.AggressivelyCache())
            {
                var toSave = Data.StoredRefreshToken.ToDbFormat(key, value);
                await s.StoreAsync(toSave);
                await s.SaveChangesAsync();
            }
        }

        public async Task<RefreshToken> GetAsync(string key)
        {
            using (var s = _store.OpenAsyncSession())
            using (s.Advanced.DocumentStore.AggressivelyCache())
            {
                var loaded = await s.LoadAsync<Data.StoredRefreshToken>("refreshtokens/" + key);
                if (loaded == null)
                    return null;

                return await Data.StoredRefreshToken.FromDbFormat(loaded, _clientStore);
            }
        }

        public async Task RemoveAsync(string key)
        {
            using (var s = _store.OpenAsyncSession())
            using (s.Advanced.DocumentStore.AggressivelyCache())
            {
                s.Delete("refreshtokens/" + key);
                await s.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ITokenMetadata>> GetAllAsync(string subject)
        {
            var result = new List<ITokenMetadata>();

            using (var s = _store.OpenAsyncSession())
            using (s.Advanced.DocumentStore.AggressivelyCache())
            {
                var q = s.Query<Data.StoredRefreshToken, Indexes.RefreshTokenIndex>().Where(x => x.SubjectId == subject);
                var loaded = await q.Take(int.MaxValue).ToListAsync();

                foreach(var thisOne in loaded)
                {
                    result.Add(await Data.StoredRefreshToken.FromDbFormat(thisOne, _clientStore));
                }
            }

            return result;
        }

        public async Task RevokeAsync(string subject, string client)
        {
            using (var s = _store.OpenAsyncSession())
            using (s.Advanced.DocumentStore.AggressivelyCache())
            {
                var q = s.Query<Data.StoredRefreshToken, Indexes.RefreshTokenIndex>().Where(x => x.SubjectId == subject && x.ClientId == client);
                var loaded = await q.Take(int.MaxValue).ToListAsync();
                foreach(var thisOne in loaded)
                {
                    s.Delete(thisOne);
                }
                await s.SaveChangesAsync();
            }
        }
    }
}
