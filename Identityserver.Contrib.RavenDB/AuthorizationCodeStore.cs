using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using Raven.Client;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace Identityserver.Contrib.RavenDB
{
    public class AuthorizationCodeStore : IAuthorizationCodeStore
    {
        private readonly IDocumentSession s;

        public AuthorizationCodeStore(SessionWrapper session)
        {
            s = session.Session;
        }

        public Task StoreAsync(string key, AuthorizationCode value)
        {
            var toSave = Data.StoredAuthorizationCode.ToDbFormat(key, value);
            s.Store(toSave);
            return Task.CompletedTask;
        }

        public Task<AuthorizationCode> GetAsync(string key)
        {
            var loaded = s.Load<Data.StoredAuthorizationCode>("authorizationcodes/" + key);
            if (loaded == null)
                return null;

            var client = s.Load<Data.StoredClient>("clients/" + loaded.ClientId);
            var scopes = s.Load<Data.StoredScope>(from scope in loaded.RequestedScopes select "scopes/" + scope);

            return Task.FromResult(Data.StoredAuthorizationCode.FromDbFormat(loaded, client, scopes.Values));
        }

        public Task RemoveAsync(string key)
        {
            s.Delete("authorizationcodes/" + key);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<ITokenMetadata>> GetAllAsync(string subject)
        {
            var result = new List<AuthorizationCode>();

            var q = s.Query<Data.StoredAuthorizationCode, Indexes.AuthorizationCodeIndex>().Where(x => x.SubjectId == subject);
            var loaded = q.ToList();

            var clients = s.Load<Data.StoredClient>(from l in loaded select l.ClientId);
            var scopes = s.Load<Data.StoredScope>(from l in loaded from s in l.RequestedScopes select "scopes/" + s);

            foreach (var thisOne in loaded)
            {
                var thisScopes = (from s in thisOne.RequestedScopes select scopes["scopes/" + s]);
                result.Add(Data.StoredAuthorizationCode.FromDbFormat(thisOne, clients["clients/" + thisOne.ClientId], thisScopes));
            }

            return Task.FromResult(result.Cast<ITokenMetadata>());
        }

        public Task RevokeAsync(string subject, string client)
        {
            var q = s.Query<Data.StoredAuthorizationCode, Indexes.AuthorizationCodeIndex>().Where(x => x.ClientId == client && x.SubjectId == subject);
            var loaded = q.ToList();

            foreach (var thisOne in loaded)
            {
                s.Delete(thisOne.Id);
            }

            return Task.CompletedTask;
        }
    }
}
