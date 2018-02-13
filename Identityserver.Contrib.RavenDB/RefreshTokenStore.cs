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
    public class RefreshTokenStore : IRefreshTokenStore
    {
        private readonly IDocumentSession s;

        public RefreshTokenStore(SessionWrapper session)
        {
            s = session.Session;
        }

        public Task StoreAsync(string key, RefreshToken value)
        {
            var toSave = Data.StoredRefreshToken.ToDbFormat(key, value);
            s.Store(toSave);
            return Task.CompletedTask;
        }

        public Task<RefreshToken> GetAsync(string key)
        {
            var loaded = s.Load<Data.StoredRefreshToken>("refreshtokens/" + key);
            if (loaded == null)
                return null;

            var client = s.Load<Data.StoredClient>("clients/" + loaded.ClientId);

            return Task.FromResult(Data.StoredRefreshToken.FromDbFormat(loaded, client));
        }

        public Task RemoveAsync(string key)
        {
            s.Delete("refreshtokens/" + key);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<ITokenMetadata>> GetAllAsync(string subject)
        {
            var result = new List<ITokenMetadata>();

            var q = s.Query<Data.StoredRefreshToken, Indexes.RefreshTokenIndex>().Where(x => x.SubjectId == subject);
            var loaded = q.ToList();
            var clients = s.Load<Data.StoredClient>(from l in loaded select "clients/" + l.ClientId);

            foreach (var thisOne in loaded)
            {
                result.Add(Data.StoredRefreshToken.FromDbFormat(thisOne, clients["clients/" + thisOne.ClientId]));
            }

            return Task.FromResult(result.Cast<ITokenMetadata>());
        }

        public Task RevokeAsync(string subject, string client)
        {
            var q = s.Query<Data.StoredRefreshToken, Indexes.RefreshTokenIndex>().Where(x => x.SubjectId == subject && x.ClientId == client);
            var loaded = q.ToList();
            foreach (var thisOne in loaded)
            {
                s.Delete(thisOne);
            }
            return Task.CompletedTask;
        }
    }
}
