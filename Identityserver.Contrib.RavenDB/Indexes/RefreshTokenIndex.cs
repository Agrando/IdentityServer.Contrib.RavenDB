using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raven.Client.Indexes;

namespace Identityserver.Contrib.RavenDB.Indexes
{
    public class RefreshTokenIndex : AbstractIndexCreationTask<Data.StoredRefreshToken>
    {
        public RefreshTokenIndex()
        {
            Map = refreshTokens => from refreshToken in refreshTokens
                select new
                {
                    refreshToken.SubjectId,
                    refreshToken.ClientId
                };
        }
    }
}
