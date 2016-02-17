using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using Raven.Client.Document;

namespace Identityserver.Contrib.RavenDB
{
    public class RefreshTokenStore : IRefreshTokenStore
    {
        private readonly DocumentStore _store;

        public RefreshTokenStore(DocumentStore store)
        {
            _store = store;
        }

        public async Task StoreAsync(string key, RefreshToken value)
        {
            using (var s = _store.OpenAsyncSession())
            {
                var toSave = Data.StoredRefreshToken.ToDbFormat(key, value);
                await s.StoreAsync(toSave);
                await s.SaveChangesAsync();
            }
        }

        public async Task<RefreshToken> GetAsync(string key)
        {
            using (var s = _store.OpenAsyncSession())
            {
                var loaded = await s.LoadAsync<Data.StoredRefreshToken>("refreshtokens/" + key);
                if (loaded == null)
                    return null;

                return Data.StoredRefreshToken.FromDbFormat(loaded, s);
            }
        }

        public async Task RemoveAsync(string key)
        {
            using (var s = _store.OpenAsyncSession())
            {
                s.Delete("refreshtokens/" + key);
                await s.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ITokenMetadata>> GetAllAsync(string subject)
        {
            var result = new List<ITokenMetadata>();

            using (var s = _store.OpenAsyncSession())
            {
                var q = s.Query<Data.StoredRefreshToken, Indexes.RefreshTokenIndex>().Where(x => x.SubjectId == subject);

                var streamer = await s.Advanced.StreamAsync(q);
                while (await streamer.MoveNextAsync())
                {
                    var thisOne = streamer.Current.Document;
                    result.Add(Data.StoredRefreshToken.FromDbFormat(thisOne, s));
                }
            }

            return result;
        }

        public async Task RevokeAsync(string subject, string client)
        {
            using (var s = _store.OpenAsyncSession())
            {
                var q = s.Query<Data.StoredRefreshToken, Indexes.RefreshTokenIndex>().Where(x => x.SubjectId == subject && x.ClientId == client);
                var streamer = await s.Advanced.StreamAsync(q);
                while (await streamer.MoveNextAsync())
                {
                    s.Delete(streamer.Current.Document);
                }
                await s.SaveChangesAsync();
            }
        }
    }
}
