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
    public class StoredToken
    {
        public string Id { get; set; }

        public string Audience { get; set; }

        public List<StoredClientClaim> Claims { get; set; } = new List<StoredClientClaim>();

        public string ClientId { get; set; }

        public string Client { get; set; }

        public DateTimeOffset CreationTime { get; set; }

        public DateTimeOffset Expires { get; set; }

        public string Issuer { get; set; }

        public int Lifetime { get; set; }

        public string SubjectId { get; set; }

        public string Type { get; set; }

        public int Version { get; set; }

        internal static StoredToken ToDbFormat(Token token)
        {
            return new StoredToken
            {
                ClientId = token.ClientId,
                Client = token.Client.ClientId,
                Audience = token.Audience,
                CreationTime = token.CreationTime,
                Lifetime = token.Lifetime,
                Expires = token.CreationTime.AddSeconds(token.Lifetime),
                Type = token.Type,
                Issuer = token.Issuer,
                SubjectId = token.SubjectId,
                Version = token.Version,
                Claims = (from c in token.Claims select StoredClientClaim.ToDbFormat(c)).ToList()
            };
        }

        internal static async Task<Token> FromDbFormat(StoredToken token, IClientStore clientStore)
        {
            return new Token
            {
                Audience = token.Audience,
                Client = await clientStore.FindClientByIdAsync(token.Client).ConfigureAwait(false),
                Type = token.Type,
                CreationTime = token.CreationTime,
                Issuer = token.Issuer,
                Lifetime = token.Lifetime,
                Version = token.Version,
                Claims = (from c in token.Claims select Data.StoredClientClaim.FromDbFormat(c)).ToList()
            };
        }
    }
}
