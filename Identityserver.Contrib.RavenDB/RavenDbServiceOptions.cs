using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;

namespace Identityserver.Contrib.RavenDB
{
    public class RavenDbServiceOptions
    {
        private readonly IDocumentStore _store;

        public RavenDbServiceOptions(string conStringName)
        {
            _store = new DocumentStore {ConnectionStringName = conStringName};
            _store.Initialize();

            IndexCreation.CreateIndexes(GetType().Assembly, _store);
        }

        public IDocumentStore Store { get { return _store; } }
    }
}
