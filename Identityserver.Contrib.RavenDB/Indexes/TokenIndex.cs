using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raven.Client.Documents.Indexes;

namespace Identityserver.Contrib.RavenDB.Indexes
{
    public class TokenIndex : AbstractIndexCreationTask<Data.StoredToken>
    {
        public TokenIndex()
        {
            Map = tokens => from token in tokens
                select new
                {
                    token.ClientId,
                    token.SubjectId
                };
        }
    }
}
