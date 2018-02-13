using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using Raven.Client;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace Identityserver.Contrib.RavenDB
{
    public class ConsentStore : IConsentStore
    {
        private readonly IDocumentSession s;
        public ConsentStore(SessionWrapper session)
        {
            s = session.Session;
        }

        public Task<IEnumerable<Consent>> LoadAllAsync(string subject)
        {
            var result = new List<Consent>();

            var loaded = s.Advanced.LoadStartingWith<Data.StoredConsent>("consents/" + subject, pageSize: int.MaxValue);
            foreach (var thisOne in loaded)
            {
                result.Add(Data.StoredConsent.FromDbFormat(thisOne));
            }

            return Task.FromResult((IEnumerable<Consent>) result);
        }

        public Task RevokeAsync(string subject, string client)
        {
            var id = "consents/" + subject + "/" + client;
            s.Delete(id);
            return Task.CompletedTask;
        }

        public Task<Consent> LoadAsync(string subject, string client)
        {
            var id = "consents/" + subject + "/" + client;

            var loaded = s.Load<Data.StoredConsent>(id);
            if (loaded == null)
                return null;

            return Task.FromResult(Data.StoredConsent.FromDbFormat(loaded));
        }

        public Task UpdateAsync(Consent consent)
        {
            var storedConsent = Data.StoredConsent.ToDbFormat(consent);
            s.Store(storedConsent);
            return Task.CompletedTask;
        }
    }
}
