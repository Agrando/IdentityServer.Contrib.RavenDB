using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raven.Client;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;

namespace Identityserver.Contrib.RavenDB
{
    public class RavenDbServiceOptions
    {
        private readonly IDocumentStore _store;

        public RavenDbServiceOptions(string[] urls, string databasename)
        {
            _store = new DocumentStore { Urls = urls, Database = databasename };
            _store.Initialize();

            IndexCreation.CreateIndexes(GetType().Assembly, _store);
        }

        public IDocumentStore Store { get { return _store; } }
    }
}
