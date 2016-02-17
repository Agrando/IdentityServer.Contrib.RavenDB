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
    public class TokenHandleStore : ITokenHandleStore
    {
        private readonly IDocumentStore _store;
        public TokenHandleStore(IDocumentStore store)
        {
            _store = store;
        }

        public async Task StoreAsync(string key, Token value)
        {
            using (var s = _store.OpenAsyncSession())
            {
                var toSave = Data.StoredToken.ToDbFormat(value);
                toSave.Id = "tokens/" + key;
                await s.StoreAsync(toSave);
                await s.SaveChangesAsync();
            }
        }

        public async Task<Token> GetAsync(string key)
        {
            using (var s = _store.OpenAsyncSession())
            {
                var loaded = await s.LoadAsync<Data.StoredToken>("tokens/" + key);
                if (loaded == null)
                    return null;

                return Data.StoredToken.FromDbFormat(loaded, s);
            }
        }

        public async Task RemoveAsync(string key)
        {
            using (var s = _store.OpenAsyncSession())
            {
                s.Delete("tokens/" + key);
                await s.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ITokenMetadata>> GetAllAsync(string subject)
        {
            var result = new List<ITokenMetadata>();

            using (var s = _store.OpenAsyncSession())
            {
                var q = s.Query<Data.StoredToken, Indexes.TokenIndex>().Where(x => x.SubjectId == subject);

                var streamer = await s.Advanced.StreamAsync(q);
                while (await streamer.MoveNextAsync())
                {
                    var thisOne = streamer.Current.Document;
                    result.Add(Data.StoredToken.FromDbFormat(thisOne, s));
                }
            }

            return result;
        }

        public async Task RevokeAsync(string subject, string client)
        {
            using (var s = _store.OpenAsyncSession())
            {
                var q = s.Query<Data.StoredToken, Indexes.TokenIndex>().Where(x => x.SubjectId == subject && x.ClientId == client);
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
