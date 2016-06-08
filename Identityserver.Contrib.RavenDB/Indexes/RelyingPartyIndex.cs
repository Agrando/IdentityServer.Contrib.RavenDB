using Raven.Client.Indexes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identityserver.Contrib.RavenDB.Indexes
{
    public class RelyingPartyIndex : AbstractIndexCreationTask<Data.StoredRelyingParty>
    {
        public RelyingPartyIndex()
        {
            Map = maps => from m in maps
                          select new
                          {
                              m.Realm
                          };
        }
    }
}
