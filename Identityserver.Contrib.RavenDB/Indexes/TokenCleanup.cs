using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raven.Client.Indexes;

namespace Identityserver.Contrib.RavenDB.Indexes
{
    public class TokenCleanupResult
    {
        public DateTimeOffset Expires { get; set; }
    }

    public class TokenCleanupIndex : AbstractMultiMapIndexCreationTask<TokenCleanupResult>
    {
        public TokenCleanupIndex()
        {
            AddMap<Data.StoredAuthorizationCode>(tokens => from t in tokens select new {t.Expires});
            AddMap<Data.StoredToken>(tokens => from t in tokens select new {t.Expires});
            AddMap<Data.StoredRefreshToken>(tokens => from t in tokens select new {t.Expires});
        }
    }
}
