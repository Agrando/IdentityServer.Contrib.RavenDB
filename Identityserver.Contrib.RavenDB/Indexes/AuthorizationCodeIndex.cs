using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raven.Client.Documents.Indexes;

namespace Identityserver.Contrib.RavenDB.Indexes
{
    public class AuthorizationCodeIndex : AbstractIndexCreationTask<Data.StoredAuthorizationCode>
    {
        public AuthorizationCodeIndex()
        {
            Map = codes => from code in codes
                select new
                {
                    code.SubjectId,
                    code.ClientId
                };
        }
    }
}
