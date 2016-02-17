using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;

namespace Identityserver.Contrib.RavenDB.Data
{
    public class StoredConsent
    {
        public string Id { get; set; }

        public string ClientId { get; set; }

        public IEnumerable<string> Scopes { get; set; }

        public string Subject { get; set; }

        internal static StoredConsent ToDbFormat(Consent consent)
        {
            return new StoredConsent
            {
                Id = "consents/" + consent.Subject + "/" + consent.ClientId,
                ClientId = consent.ClientId,
                Subject = consent.Subject,
                Scopes = consent.Scopes
            };
        }

        internal static Consent FromDbFormat(StoredConsent consent)
        {
            return new Consent
            {
                Subject = consent.Subject,
                ClientId = consent.ClientId,
                Scopes = consent.Scopes
            };
        }
    }
}
