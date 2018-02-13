using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using Raven.Client;
using System.Security.Claims;

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

        public List<StoredRefreshTokenClaim> Claims { get; set; } = new List<StoredRefreshTokenClaim>();

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
                AccessToken = Data.StoredToken.ToDbFormat(refreshToken.AccessToken),
                Claims = (from c in refreshToken.Subject.Claims select StoredRefreshTokenClaim.ToDbFormat(c)).ToList()
            };
        }

        internal static RefreshToken FromDbFormat(StoredRefreshToken token, StoredClient client)
        {
            return new RefreshToken
            {
                CreationTime = token.CreationTime,
                LifeTime = token.LifeTime,
                Version = token.Version,
                Subject = new ClaimsPrincipal(new ClaimsIdentity((from c in token.Claims select StoredRefreshTokenClaim.FromDbFormat(c)).ToList())),
                AccessToken = Data.StoredToken.FromDbFormat(token.AccessToken, client)
            };
        }
    }

    public class StoredRefreshTokenClaim
    {
        public string Type { get; set; }
        public string Value { get; set; }
        public string ValueType { get; set; }

        internal static StoredRefreshTokenClaim ToDbFormat(Claim claim)
        {
            return new StoredRefreshTokenClaim
            {
                Type = claim.Type,
                Value = claim.Value,
                ValueType = claim.ValueType
            };
        }

        internal static Claim FromDbFormat(StoredRefreshTokenClaim claim)
        {
            return new Claim(claim.Type, claim.Value, claim.ValueType);
        }
    }
}
