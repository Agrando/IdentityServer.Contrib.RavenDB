using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identityserver.Contrib.RavenDB.Data
{
    public class StoredRelyingParty
    {
        public string Id { get; set; }

        public List<StoredClaimMapping> ClaimMappings { get; set; } = new List<StoredClaimMapping>();
        
        public string DefaultClaimTypeMappingPrefix { get; set; }
        
        public string DigestAlgorithm { get; set; }
        
        public bool Enabled { get; set; }
        
        public byte[] EncryptingCertificate { get; set; }
        
        public bool IncludeAllClaimsForUser { get; set; }
        
        public string Name { get; set; }

        public List<string> PostLogoutRedirectUris { get; set; } = new List<string>();
        
        public string Realm { get; set; }
        
        public string ReplyUrl { get; set; }
        
        public string SamlNameIdentifierFormat { get; set; }
        
        public string SignatureAlgorithm { get; set; }
        
        public int TokenLifeTime { get; set; }
        
        public string TokenType { get; set; }
    }

    public class StoredClaimMapping
    {
        public string InboundClaim { get; set; }
        public string OutboundClaim { get; set; }
    }
}
