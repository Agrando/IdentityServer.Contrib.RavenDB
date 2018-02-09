using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services;
using Raven.Client;
using Raven.Client.Documents;

namespace Identityserver.Contrib.RavenDB.Registrations
{
    public class ClientConfigurationCorsPolicyRegistration : Registration<ICorsPolicyService, Services.ClientConfigurationCorsPolicyService>
    {
        public ClientConfigurationCorsPolicyRegistration(RavenDbServiceOptions options)
        {
            if (options == null) throw new ArgumentNullException("options");

            this.AdditionalRegistrations.Add(new Registration<IDocumentStore>(options.Store));
        }
    }
}
