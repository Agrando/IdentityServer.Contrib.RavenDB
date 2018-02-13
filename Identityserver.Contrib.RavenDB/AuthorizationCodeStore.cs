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
    public class AuthorizationCodeStore : IAuthorizationCodeStore
    {
        private readonly IDocumentSession s;
        private readonly IClientStore _clientStore;
        private readonly IScopeStore _scopeStore;

        public AuthorizationCodeStore(SessionWrapper session, IClientStore clientStore, IScopeStore scopeStore)
        {
            s = session.Session;
            _clientStore = clientStore;
            _scopeStore = scopeStore;
        }

        public Task StoreAsync(string key, AuthorizationCode value)
        {
            var toSave = Data.StoredAuthorizationCode.ToDbFormat(key, value);
            s.Store(toSave);
            return Task.Delay(0);
        }

        public async Task<AuthorizationCode> GetAsync(string key)
        {
            var loaded = s.Load<Data.StoredAuthorizationCode>("authorizationcodes/" + key);
            if (loaded == null)
                return null;

            return await Data.StoredAuthorizationCode.FromDbFormat(loaded, _clientStore, _scopeStore);
        }

        public Task RemoveAsync(string key)
        {
            s.Delete("authorizationcodes/" + key);
            return Task.Delay(0);
        }

        public async Task<IEnumerable<ITokenMetadata>> GetAllAsync(string subject)
        {
            var result = new List<AuthorizationCode>();

            var q = s.Query<Data.StoredAuthorizationCode, Indexes.AuthorizationCodeIndex>().Where(x => x.SubjectId == subject);
            var loaded = q.ToList();

            foreach (var thisOne in loaded)
            {
                result.Add(await Data.StoredAuthorizationCode.FromDbFormat(thisOne, _clientStore, _scopeStore));
            }

            return result;
        }

        public Task RevokeAsync(string subject, string client)
        {
            var q = s.Query<Data.StoredAuthorizationCode, Indexes.AuthorizationCodeIndex>().Where(x => x.ClientId == client && x.SubjectId == subject);
            var loaded = q.ToList();

            foreach (var thisOne in loaded)
            {
                s.Delete(thisOne.Id);
            }

            return Task.Delay(0);
        }
    }
}
