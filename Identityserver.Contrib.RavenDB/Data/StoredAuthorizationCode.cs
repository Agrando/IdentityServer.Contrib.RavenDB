using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using Raven.Client;

namespace Identityserver.Contrib.RavenDB.Data
{
    public class StoredAuthorizationCode
    {
        public string Id { get; set; }
        public string Client { get; set; }

        public string ClientId { get; set; }
        
        public string CodeChallenge { get; set; }
        
        public string CodeChallengeMethod { get; set; }
        
        public DateTimeOffset CreationTime { get; set; }

        public DateTimeOffset Expires { get; set; }
        
        public bool IsOpenId { get; set; }
        
        public string Nonce { get; set; }

        public string RedirectUri { get; set; }

        public IEnumerable<string> RequestedScopes { get; set; }
        
        public string SessionId { get; set; }

        public IList<StoredIdentity> Subject { get; set; }
        
        public string SubjectId { get; set; }

        public bool WasConsentShown { get; set; }

        internal static StoredAuthorizationCode ToDbFormat(string key, AuthorizationCode code)
        {
            var result = new StoredAuthorizationCode
            {
                Id = "authorizationcodes/" + key,
                ClientId = code.ClientId,
                Client = code.Client.ClientId,
                CodeChallenge = code.CodeChallenge,
                WasConsentShown = code.WasConsentShown,
                CreationTime = code.CreationTime,
                Expires = code.CreationTime.AddSeconds(code.Client.AuthorizationCodeLifetime),
                RedirectUri = code.RedirectUri,
                IsOpenId = code.IsOpenId,
                Nonce = code.Nonce,
                SessionId = code.SessionId,
                CodeChallengeMethod = code.CodeChallengeMethod,
                SubjectId = code.SubjectId,
                RequestedScopes = (from s in code.RequestedScopes select s.Name).ToList(),
            };

            result.Subject = new List<StoredIdentity>();
            foreach (var id in code.Subject.Identities)
            {
                result.Subject.Add(Data.StoredIdentity.ToDbFormat(id));
            }

            return result;
        }

        internal static AuthorizationCode FromDbFormat(StoredAuthorizationCode code, IAsyncDocumentSession s, IScopeStore scopeStore)
        {
            var result = new AuthorizationCode
            {
                CreationTime = code.CreationTime,
                IsOpenId = code.IsOpenId,
                RedirectUri = code.RedirectUri,
                WasConsentShown = code.WasConsentShown,
                Nonce = code.Nonce,
                Client = Data.StoredClient.FromDbFormat(s.LoadAsync<Data.StoredClient>("clients/" + code.Client).Result),
                CodeChallenge = code.CodeChallenge,
                CodeChallengeMethod = code.CodeChallengeMethod,
                SessionId = code.SessionId,
                RequestedScopes = scopeStore.FindScopesAsync(code.RequestedScopes).Result
            };

            var claimsPrinciple = new ClaimsPrincipal();
            foreach (var id in code.Subject)
            {
                claimsPrinciple.AddIdentity(Data.StoredIdentity.FromDbFormat(id));
            }
            result.Subject = claimsPrinciple;

            return result;
        }
    }
}
