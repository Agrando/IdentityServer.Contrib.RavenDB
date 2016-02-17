using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raven.Client.Indexes;

namespace Identityserver.Contrib.RavenDB.Indexes
{
    public class CorsIndex : AbstractIndexCreationTask<Data.StoredClient>
    {
        public CorsIndex()
        {
            Map = clients => from client in clients
                from allowedOrigin in client.AllowedCorsOrigins
                select new
                {
                    client.ClientId,
                    AllowedOrigin = allowedOrigin
                };
        }
    }
}
