using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using Raven.Client;

namespace Identityserver.Contrib.RavenDB.Data
{
    public class StoredRefreshToken
    {
        public string Id { get; set; }

        public StoredToken AccessToken { get; set; }
        
        public string ClientId { get; set; }
        
        public DateTimeOffset CreationTime { get; set; }

        public DateTimeOffset Expires { get; set; }
        
        public int LifeTime { get; set; }
        
        public IEnumerable<string> Scopes { get; set; }
        
        public string SubjectId { get; set; }
        
        public int Version { get; set; }

        internal static StoredRefreshToken ToDbFormat(string key, RefreshToken refreshToken)
        {
            return new StoredRefreshToken
            {
                Id = "refreshtokens/" + key,
                ClientId = refreshToken.ClientId,
                Scopes = refreshToken.Scopes,
                CreationTime = refreshToken.CreationTime,
                LifeTime = refreshToken.LifeTime,
                SubjectId = refreshToken.SubjectId,
                Version = refreshToken.Version,
                Expires = refreshToken.CreationTime.AddSeconds(refreshToken.LifeTime),
                AccessToken = Data.StoredToken.ToDbFormat(refreshToken.AccessToken)
            };
        }

        internal static async Task<RefreshToken> FromDbFormat(StoredRefreshToken token, IClientStore clientStore)
        {
            return new RefreshToken
            {
                CreationTime = token.CreationTime,
                LifeTime = token.LifeTime,
                Version = token.Version,
                AccessToken = await Data.StoredToken.FromDbFormat(token.AccessToken, clientStore)
            };
        }
    }
}
