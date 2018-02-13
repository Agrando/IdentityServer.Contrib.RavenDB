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
    public class TokenHandleStore : ITokenHandleStore
    {
        private readonly IDocumentSession s;
        private readonly IClientStore _clientStore;

        public TokenHandleStore(SessionWrapper session, IClientStore clientStore)
        {
            s = session.Session;
            _clientStore = clientStore;
        }

        public Task StoreAsync(string key, Token value)
        {
            var toSave = Data.StoredToken.ToDbFormat(value);
            toSave.Id = "tokens/" + key;
            s.Store(toSave);
            return Task.Delay(0);
        }

        public async Task<Token> GetAsync(string key)
        {

            var loaded = s.Load<Data.StoredToken>("tokens/" + key);
            if (loaded == null)
                return null;

            return await Data.StoredToken.FromDbFormat(loaded, _clientStore);
        }

        public Task RemoveAsync(string key)
        {
            s.Delete("tokens/" + key);
            return Task.Delay(0);
        }

        public async Task<IEnumerable<ITokenMetadata>> GetAllAsync(string subject)
        {
            var result = new List<ITokenMetadata>();

            var q = s.Query<Data.StoredToken, Indexes.TokenIndex>().Where(x => x.SubjectId == subject);
            var loaded = q.ToList();

            foreach (var thisOne in loaded)
            {
                result.Add(await Data.StoredToken.FromDbFormat(thisOne, _clientStore));
            }

            return result;
        }

        public Task RevokeAsync(string subject, string client)
        {
            var q = s.Query<Data.StoredToken, Indexes.TokenIndex>().Where(x => x.SubjectId == subject && x.ClientId == client);
            var loaded = q.ToList();

            foreach (var thisOne in loaded)
            {
                s.Delete(thisOne);
            }
            return Task.Delay(0);
        }
    }
}
