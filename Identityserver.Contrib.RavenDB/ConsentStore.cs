using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using Raven.Client;
using Raven.Client.Documents;

namespace Identityserver.Contrib.RavenDB
{
    public class ConsentStore : IConsentStore
    {
        private readonly IDocumentStore _store;
        public ConsentStore(IDocumentStore store)
        {
            _store = store;
        }

        public async Task<IEnumerable<Consent>> LoadAllAsync(string subject)
        {
            var result = new List<Consent>();

            using (var s = _store.OpenAsyncSession())
             
            {
                var loaded = await s.Advanced.LoadStartingWithAsync<Data.StoredConsent>("consents/" + subject, pageSize: int.MaxValue);
                foreach(var thisOne in loaded)
                {
                    result.Add(Data.StoredConsent.FromDbFormat(thisOne));
                }
            }

            return result;
        }

        public async Task RevokeAsync(string subject, string client)
        {
            var id = "consents/" + subject + "/" + client;
            using (var s = _store.OpenAsyncSession())
             
            {
                s.Delete(id);
                await s.SaveChangesAsync();
            }
        }

        public async Task<Consent> LoadAsync(string subject, string client)
        {
            var id = "consents/" + subject + "/" + client;
            using (var s = _store.OpenAsyncSession())
             
            {
                var loaded = await s.LoadAsync<Data.StoredConsent>(id);
                if (loaded == null)
                    return null;

                return Data.StoredConsent.FromDbFormat(loaded);
            }
        }

        public async Task UpdateAsync(Consent consent)
        {
            using (var s = _store.OpenAsyncSession())
             
            {
                var storedConsent = Data.StoredConsent.ToDbFormat(consent);

                await s.StoreAsync(storedConsent);
                await s.SaveChangesAsync();
            }
        }
    }
}
