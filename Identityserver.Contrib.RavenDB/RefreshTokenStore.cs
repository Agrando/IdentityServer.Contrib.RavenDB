﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using Raven.Client;
using Raven.Client.Document;

namespace Identityserver.Contrib.RavenDB
{
    public class RefreshTokenStore : IRefreshTokenStore
    {
        private readonly IDocumentSession s;
        private readonly IClientStore _clientStore;

        public RefreshTokenStore(SessionWrapper session, IClientStore clientStore)
        {
            s = session.Session;
            _clientStore = clientStore;
        }

        public Task StoreAsync(string key, RefreshToken value)
        {
            var toSave = Data.StoredRefreshToken.ToDbFormat(key, value);
            s.Store(toSave);
            return Task.Delay(0);
        }

        public async Task<RefreshToken> GetAsync(string key)
        {
            var loaded = s.Load<Data.StoredRefreshToken>("refreshtokens/" + key);
            if (loaded == null)
                return null;

            var client = s.Load<Data.StoredClient>("clients/" + loaded.ClientId);

            return await Data.StoredRefreshToken.FromDbFormat(loaded, _clientStore);
        }

        public Task RemoveAsync(string key)
        {
            s.Delete("refreshtokens/" + key);
            return Task.Delay(0);
        }

        public async Task<IEnumerable<ITokenMetadata>> GetAllAsync(string subject)
        {
            var result = new List<ITokenMetadata>();

            var q = s.Query<Data.StoredRefreshToken, Indexes.RefreshTokenIndex>().Where(x => x.SubjectId == subject);
            var loaded = q.ToList();

            foreach (var thisOne in loaded)
            {
                result.Add(await Data.StoredRefreshToken.FromDbFormat(thisOne, _clientStore));
            }

            return result;
        }

        public Task RevokeAsync(string subject, string client)
        {
            var q = s.Query<Data.StoredRefreshToken, Indexes.RefreshTokenIndex>().Where(x => x.SubjectId == subject && x.ClientId == client);
            var loaded = q.ToList();
            foreach (var thisOne in loaded)
            {
                s.Delete(thisOne);
            }
            return Task.Delay(0);
        }
    }
}
