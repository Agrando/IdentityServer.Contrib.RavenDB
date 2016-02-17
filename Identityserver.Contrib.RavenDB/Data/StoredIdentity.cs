using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Identityserver.Contrib.RavenDB.Data
{
    public class StoredIdentity
    {
        public string AuthenticationType { get; set; }
        public IEnumerable<StoredClientClaim> Claims { get; set; }

        internal static StoredIdentity ToDbFormat(ClaimsIdentity id)
        {
            return new StoredIdentity
            {
                AuthenticationType = id.AuthenticationType,
                Claims = (from c in id.Claims select Data.StoredClientClaim.ToDbFormat(c)).ToList()
            };
        }

        internal static ClaimsIdentity FromDbFormat(StoredIdentity id)
        {
            var claims = (from c in id.Claims select Data.StoredClientClaim.FromDbFormat(c)).ToList();
            if(id.AuthenticationType == null)
                return new ClaimsIdentity(claims);

            return new ClaimsIdentity(claims, id.AuthenticationType);
        }
    }
}
