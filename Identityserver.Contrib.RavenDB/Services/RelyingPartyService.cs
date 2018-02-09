using IdentityServer3.WsFederation.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.WsFederation.Models;
using Raven.Client;
using Raven.Client.Documents;

namespace Identityserver.Contrib.RavenDB.Services
{
    public class RelyingPartyService : IRelyingPartyService
    {
        readonly IDocumentStore _store;
        public RelyingPartyService(IDocumentStore store)
        {
            _store = store;
        }

        public async Task<RelyingParty> GetByRealmAsync(string realm)
        {
            using (var s = _store.OpenAsyncSession())
             
            {
                var rp = await s.Query<Data.StoredRelyingParty, Indexes.RelyingPartyIndex>().FirstOrDefaultAsync(x => x.Realm == realm);

                if (rp == null)
                    return null;

                return new RelyingParty
                {
                    ClaimMappings = rp.ClaimMappings.ToDictionary(x => x.InboundClaim, x => x.OutboundClaim),
                    DefaultClaimTypeMappingPrefix = rp.DefaultClaimTypeMappingPrefix,
                    DigestAlgorithm = rp.DigestAlgorithm,
                    Enabled = rp.Enabled,
                    EncryptingCertificate = rp.EncryptingCertificate == null ? null : new System.Security.Cryptography.X509Certificates.X509Certificate2(rp.EncryptingCertificate),
                    IncludeAllClaimsForUser = rp.IncludeAllClaimsForUser,
                    Name = rp.Name,
                    Realm = rp.Realm,
                    PostLogoutRedirectUris = rp.PostLogoutRedirectUris,
                    ReplyUrl = rp.ReplyUrl,
                    SamlNameIdentifierFormat = rp.SamlNameIdentifierFormat,
                    SignatureAlgorithm = rp.SignatureAlgorithm,
                    TokenLifeTime = rp.TokenLifeTime,
                    TokenType = rp.TokenType
                };
            }
        }
    }
}
