using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityAdmin.Core;
using IdentityAdmin.Core.Client;
using IdentityAdmin.Core.Metadata;
using IdentityAdmin.Core.Scope;
using Raven.Client;

namespace Identityserver.Contrib.RavenDB.Services
{
    public class IdentityAdminService : IIdentityAdminService
    {
        private readonly IDocumentStore _store;
        public IdentityAdminService(IDocumentStore store)
        {
            _store = store;
        }

        public Task<IdentityAdminMetadata> GetMetadataAsync()
        {
            var updateClient = new List<PropertyMetadata>();
            updateClient.AddRange(PropertyMetadata.FromType<Data.StoredClient>());

            var createClient = new List<PropertyMetadata>
            {
                PropertyMetadata.FromProperty<Data.StoredClient>(x => x.ClientName,"ClientName", required: true),
                PropertyMetadata.FromProperty<Data.StoredClient>(x => x.ClientId,"ClientId", required: true),
            };

            var client = new ClientMetaData
            {
                SupportsCreate = true,
                SupportsDelete = true,
                CreateProperties = createClient,
                UpdateProperties = updateClient
            };


            var updateScope = new List<PropertyMetadata>();
            updateScope.AddRange(PropertyMetadata.FromType<Data.StoredScope>());

            var createScope = new List<PropertyMetadata>
            {
                PropertyMetadata.FromProperty<Data.StoredScope>(x => x.Name,"ScopeName", required: true),
            };

            var scope = new ScopeMetaData
            {
                SupportsCreate = true,
                SupportsDelete = true,
                CreateProperties = createScope,
                UpdateProperties = updateScope
            };


            var identityAdminMetadata = new IdentityAdminMetadata
            {
                ClientMetaData = client,
                ScopeMetaData = scope
            };

            return Task.FromResult(identityAdminMetadata);
        }

        public async Task<IdentityAdminResult<ScopeDetail>> GetScopeAsync(string subject)
        {
            using (var s = _store.OpenAsyncSession())
            {
                var loaded = await s.LoadAsync<Data.StoredScope>(subject);

                if (loaded == null)
                    return new IdentityAdminResult<ScopeDetail>("Scope not found");

                return new IdentityAdminResult<ScopeDetail>(new ScopeDetail
                {
                    Subject = loaded.Id,
                    Description = loaded.Description,
                    Name = loaded.Name,
                    ScopeClaimValues = from sc in loaded.Claims
                        select new ScopeClaimValue
                        {
                            AlwaysIncludeInIdToken = sc.AlwaysIncludeInIdToken,
                            Description = sc.Description,
                            Name = sc.Name,
                            Id = sc.Name
                        }
                });
            }
        }

        public async Task<IdentityAdminResult<QueryResult<ScopeSummary>>> QueryScopesAsync(string filter, int start, int count)
        {
            using (var s = _store.OpenAsyncSession())
            {
                RavenQueryStatistics stat;
                var q = s.Query<Data.StoredScope>().Statistics(out stat).Take(count).Skip(start);
                if (string.IsNullOrWhiteSpace(filter) == false)
                {
                    q = q.Where(x => x.Name.StartsWith(filter));
                }

                var items = from sc in await q.ToListAsync()
                    select new ScopeSummary
                    {
                        Name = sc.Name,
                        Description = sc.Description,
                        Subject = sc.Id
                    };

                return new IdentityAdminResult<QueryResult<ScopeSummary>>(new QueryResult<ScopeSummary>
                {
                    Count = items.Count(),
                    Filter = filter,
                    Items = items,
                    Start = start,
                    Total = stat.TotalResults
                });
            }
        }

        public Task<IdentityAdminResult<CreateResult>> CreateScopeAsync(IEnumerable<PropertyValue> properties)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityAdminResult> SetScopePropertyAsync(string subject, string type, string value)
        {
            throw new NotImplementedException();
        }

        public async Task<IdentityAdminResult> DeleteScopeAsync(string subject)
        {
            using (var s = _store.OpenAsyncSession())
            {
                s.Delete(subject);
                await s.SaveChangesAsync();

                return new IdentityAdminResult();
            }
        }

        public Task<IdentityAdminResult> AddScopeClaimAsync(string subject, string name, string description, bool alwaysIncludeInIdToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityAdminResult> RemoveScopeClaimAsync(string subject, string id)
        {
            throw new NotImplementedException();
        }

        public async Task<IdentityAdminResult<ClientDetail>> GetClientAsync(string subject)
        {
            using (var s = _store.OpenAsyncSession())
            {
                var loaded = await s.LoadAsync<Data.StoredClient>(subject);
                if (loaded == null)
                    return new IdentityAdminResult<ClientDetail>("Client not found");

                return new IdentityAdminResult<ClientDetail>(new ClientDetail
                {
                    Subject = subject,
                    AllowedCorsOrigins = (from c in loaded.AllowedCorsOrigins select new ClientCorsOriginValue { Id = c,Origin = c}).ToList(),
                    ClientId = loaded.ClientId,
                    ClientName = loaded.ClientName,
                    AllowedCustomGrantTypes = (from c in loaded.AllowedCustomGrantTypes select new ClientCustomGrantTypeValue { GrantType = c, Id = c}).ToList(),
                    Claims = (from c in loaded.Claims select new ClientClaimValue { Id = c.Type + c.Value, Type = c.Type, Value = c.Value}).ToList(),
                    ClientSecrets = (from c in loaded.ClientSecrets select new ClientSecretValue { Id = c.Type + c.Value, Type = c.Type, Value = c.Value}).ToList(),
                    RedirectUris = (from c in loaded.RedirectUris select new ClientRedirectUriValue { Id = c, Uri = c}).ToList(),
                    AllowedScopes = (from c in loaded.AllowedScopes select new ClientScopeValue { Id = c, Scope = c}).ToList(),
                    PostLogoutRedirectUris = (from c in loaded.PostLogoutRedirectUris select new ClientPostLogoutRedirectUriValue { Id = c, Uri = c}).ToList(),
                    IdentityProviderRestrictions = (from c in loaded.IdentityProviderRestrictions select new ClientIdPRestrictionValue { Id = c, Provider = c}).ToList()
                });
            }
        }

        public async Task<IdentityAdminResult> DeleteClientAsync(string subject)
        {
            using (var s = _store.OpenAsyncSession())
            {
                s.Delete(subject);
                await s.SaveChangesAsync();
                return new IdentityAdminResult();
            }
        }

        public Task<IdentityAdminResult> AddClientClaimAsync(string subject, string type, string value)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityAdminResult> RemoveClientClaimAsync(string subject, string id)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityAdminResult> AddClientSecretAsync(string subject, string type, string value)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityAdminResult> RemoveClientSecretAsync(string subject, string id)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityAdminResult> AddClientIdPRestrictionAsync(string subject, string provider)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityAdminResult> RemoveClientIdPRestrictionAsync(string subject, string id)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityAdminResult> AddPostLogoutRedirectUriAsync(string subject, string uri)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityAdminResult> RemovePostLogoutRedirectUriAsync(string subject, string id)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityAdminResult> AddClientRedirectUriAsync(string subject, string uri)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityAdminResult> RemoveClientRedirectUriAsync(string subject, string id)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityAdminResult> AddClientCorsOriginAsync(string subject, string origin)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityAdminResult> RemoveClientCorsOriginAsync(string subject, string id)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityAdminResult> AddClientCustomGrantTypeAsync(string subject, string grantType)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityAdminResult> RemoveClientCustomGrantTypeAsync(string subject, string id)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityAdminResult> AddClientScopeAsync(string subject, string scope)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityAdminResult> RemoveClientScopeAsync(string subject, string id)
        {
            throw new NotImplementedException();
        }

        public async Task<IdentityAdminResult<QueryResult<ClientSummary>>> QueryClientsAsync(string filter, int start, int count)
        {
            using (var s = _store.OpenAsyncSession())
            {
                RavenQueryStatistics stat;
                var q = s.Query<Data.StoredClient>().Statistics(out stat).Take(count).Skip(start);
                if (string.IsNullOrWhiteSpace(filter) == false)
                {
                    q = q.Where(x => x.ClientName.StartsWith(filter));
                }

                var items = await q.ToListAsync();

                return new IdentityAdminResult<QueryResult<ClientSummary>>(new QueryResult<ClientSummary>
                {
                    Count = items.Count(),
                    Filter = filter,
                    Start = start,
                    Total = stat.TotalResults,
                    Items = from c in items
                        select new ClientSummary
                        {
                            ClientId = c.ClientId,
                            ClientName = c.ClientName,
                            Subject = c.Id
                        }
                });
            }
        }

        public Task<IdentityAdminResult<CreateResult>> CreateClientAsync(IEnumerable<PropertyValue> properties)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityAdminResult> SetClientPropertyAsync(string subject, string type, string value)
        {
            throw new NotImplementedException();
        }
    }
}
