using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raven.Client.Documents.Indexes;

namespace Identityserver.Contrib.RavenDB.Indexes
{
    public class AdminClientQuery : AbstractIndexCreationTask<Data.StoredClient>
    {
        public AdminClientQuery()
        {
            Map = maps => from m in maps
                select new
                {
                    m.ClientName
                };

            Index(x => x.ClientName, FieldIndexing.Search);
        }
    }

    public class AdminScopeQuery : AbstractIndexCreationTask<Data.StoredScope>
    {
        public AdminScopeQuery()
        {
            Map = maps => from m in maps
                select new
                {
                    m.Name
                };

            Index(x => x.Name, FieldIndexing.Search);
        }
    }
}
