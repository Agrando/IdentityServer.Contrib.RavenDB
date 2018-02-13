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

        public RavenDbServiceOptions(Func<IDocumentStore> createStore)
        {
            _store = createStore();
            _store.Conventions.TransformTypeCollectionNameToDocumentIdPrefix = (collectionName) =>
            {
                switch (collectionName)
                {
                    case "StoredClients":
                        return "clients";

                    case "StoredRefreshTokens":
                        return "refreshtokens";

                    case "StoredRelyingParties":
                        return "relyingparty";

                    case "StoredScopes":
                        return "scopes";

                    case "StoredAuthorizationCode":
                        return "authorizationcodes";

                    case "StoredConsents":
                        return "consent";

                    case "StoredTokens":
                        return "tokens";

                    default:
                        throw new Exception("Unknown collection");
                }
            };
            _store.Initialize();
            IndexCreation.CreateIndexes(GetType().Assembly, _store);
        }

        public IDocumentStore Store { get { return _store; } }
    }
}
