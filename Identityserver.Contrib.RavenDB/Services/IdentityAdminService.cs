using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityAdmin.Core;
using IdentityAdmin.Core.Client;
using IdentityAdmin.Core.Metadata;
using IdentityAdmin.Core.Scope;
using IdentityAdmin.Extensions;
using Identityserver.Contrib.RavenDB.Data;
using IdentityServer3.Core.Models;
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
                var loaded = await s.LoadAsync<Data.StoredScope>("scopes/" + subject);

                if (loaded == null)
                    return new IdentityAdminResult<ScopeDetail>("Scope not found");

                var metadata = await GetMetadataAsync();
                var props = from prop in metadata.ScopeMetaData.UpdateProperties
                            select new PropertyValue
                            {
                                Type = prop.Type,
                                Value = GetScopeProperty(prop, loaded),
                            };


                return new IdentityAdminResult<ScopeDetail>(new ScopeDetail
                {
                    Subject = subject,
                    Description = loaded.Description,
                    Name = loaded.Name,
                    Properties = props.ToArray(),
                    ScopeClaimValues = loaded.Claims == null ? new List<ScopeClaimValue>() : (from sc in loaded.Claims
                                                                                              select new ScopeClaimValue
                                                                                              {
                                                                                                  AlwaysIncludeInIdToken = sc.AlwaysIncludeInIdToken,
                                                                                                  Description = sc.Description,
                                                                                                  Name = sc.Name,
                                                                                                  Id = sc.Name
                                                                                              })
                });
            }
        }

        public async Task<IdentityAdminResult<QueryResult<ScopeSummary>>> QueryScopesAsync(string filter, int start, int count)
        {
            using (var s = _store.OpenAsyncSession())
            {
                RavenQueryStatistics stat;
                var q = s.Query<Data.StoredScope, Indexes.AdminScopeQuery>().Statistics(out stat).Take(count).Skip(start);
                if (string.IsNullOrWhiteSpace(filter) == false)
                {
                    q = q.Where(x => x.Name.StartsWith(filter));
                }

                var items = from sc in await q.ToListAsync()
                            select new ScopeSummary
                            {
                                Name = sc.Name,
                                Description = sc.Description,
                                Subject = sc.Id.Replace("scopes/", "")
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

        public async Task<IdentityAdminResult<CreateResult>> CreateScopeAsync(IEnumerable<PropertyValue> properties)
        {
            var scopeName = properties.Single(x => x.Type == "ScopeName");
            var scopeNameValue = scopeName.Value;
            string[] exclude = { "ScopeName" };
            var otherProperties = properties.Where(x => !exclude.Contains(x.Type)).ToArray();
            var metadata = await GetMetadataAsync();
            var createProps = metadata.ScopeMetaData.CreateProperties;
            var scope = new Data.StoredScope { Name = scopeNameValue };

            foreach (var prop in otherProperties)
            {
                var propertyResult = SetScopeProperty(createProps, scope, prop.Type, prop.Value);
                if (!propertyResult.IsSuccess)
                {
                    return new IdentityAdminResult<CreateResult>(propertyResult.Errors.ToArray());
                }
            }

            using (var s = _store.OpenAsyncSession())
            {
                s.Advanced.UseOptimisticConcurrency = true;
                scope.Id = "scopes/" + scopeName.Value;
                await s.StoreAsync(scope);
                await s.SaveChangesAsync();
            }

            return new IdentityAdminResult<CreateResult>(new CreateResult { Subject = scopeName.Value });
        }

        public async Task<IdentityAdminResult> SetScopePropertyAsync(string subject, string type, string value)
        {
            using (var s = _store.OpenAsyncSession())
            {
                var loaded = await s.LoadAsync<Data.StoredScope>("scopes/" + subject);

                if (loaded == null)
                    return new IdentityAdminResult("Invalid subject");

                var meta = await GetMetadataAsync();
                var propResult = SetScopeProperty(meta.ScopeMetaData.UpdateProperties, loaded, type, value);
                if (!propResult.IsSuccess)
                {
                    return propResult;
                }

                await s.StoreAsync(loaded);
                await s.SaveChangesAsync();
                return IdentityAdminResult.Success;
            }
        }

        public async Task<IdentityAdminResult> DeleteScopeAsync(string subject)
        {
            using (var s = _store.OpenAsyncSession())
            {
                s.Delete("scopes/" + subject);
                await s.SaveChangesAsync();

                return new IdentityAdminResult();
            }
        }

        public async Task<IdentityAdminResult> AddScopeClaimAsync(string subject, string name, string description, bool alwaysIncludeInIdToken)
        {
            using (var s = _store.OpenAsyncSession())
            {
                var loaded = await s.LoadAsync<Data.StoredScope>("scopes/" + subject);
                if (loaded == null)
                    return new IdentityAdminResult("Invalid subject");

                if (loaded.Claims == null)
                    loaded.Claims = new List<StoredScopeClaim>();

                var found = loaded.Claims.FirstOrDefault(x => x.Name == name);
                if (found == null)
                {
                    var newOne = new Data.StoredScopeClaim
                    {
                        Name = name,
                        Description = description,
                        AlwaysIncludeInIdToken = alwaysIncludeInIdToken
                    };
                    loaded.Claims.Add(newOne);

                    await s.StoreAsync(loaded);
                    await s.SaveChangesAsync();
                }

                return IdentityAdminResult.Success;
            }
        }

        public async Task<IdentityAdminResult> RemoveScopeClaimAsync(string subject, string id)
        {
            using (var s = _store.OpenAsyncSession())
            {
                var loaded = await s.LoadAsync<Data.StoredScope>("scopes/" + subject);
                if (loaded == null)
                    return new IdentityAdminResult("Invalid subject");

                var found = loaded.Claims.FirstOrDefault(x => x.Name == id);
                if (found != null)
                {
                    loaded.Claims.Remove(found);

                    await s.StoreAsync(loaded);
                    await s.SaveChangesAsync();
                }

                return IdentityAdminResult.Success;
            }
        }

        public async Task<IdentityAdminResult<ClientDetail>> GetClientAsync(string subject)
        {
            using (var s = _store.OpenAsyncSession())
            {
                var loaded = await s.LoadAsync<Data.StoredClient>("clients/" + subject);
                if (loaded == null)
                    return new IdentityAdminResult<ClientDetail>("Client not found");

                var metadata = await GetMetadataAsync();
                var props = from prop in metadata.ClientMetaData.UpdateProperties
                            select new PropertyValue
                            {
                                Type = prop.Type,
                                Value = GetClientProperty(prop, loaded),
                            };

                return new IdentityAdminResult<ClientDetail>(new ClientDetail
                {
                    Subject = subject,
                    AllowedCorsOrigins = loaded.AllowedCorsOrigins == null ? new List<ClientCorsOriginValue>() : (from c in loaded.AllowedCorsOrigins select new ClientCorsOriginValue { Id = c, Origin = c }).ToList(),
                    ClientId = loaded.ClientId,
                    ClientName = loaded.ClientName,
                    AllowedCustomGrantTypes = loaded.AllowedCustomGrantTypes == null ? new List<ClientCustomGrantTypeValue>() : (from c in loaded.AllowedCustomGrantTypes select new ClientCustomGrantTypeValue { GrantType = c, Id = c }).ToList(),
                    Claims = loaded.Claims == null ? new List<ClientClaimValue>() : (from c in loaded.Claims select new ClientClaimValue { Id = c.Type + c.Value, Type = c.Type, Value = c.Value }).ToList(),
                    ClientSecrets = loaded.ClientSecrets == null ? new List<ClientSecretValue>() : (from c in loaded.ClientSecrets select new ClientSecretValue { Id = c.Type + c.Value, Type = c.Type, Value = c.Value }).ToList(),
                    RedirectUris = loaded.RedirectUris == null ? new List<ClientRedirectUriValue>() : (from c in loaded.RedirectUris select new ClientRedirectUriValue { Id = c, Uri = c }).ToList(),
                    AllowedScopes = loaded.AllowedScopes == null ? new List<ClientScopeValue>() : (from c in loaded.AllowedScopes select new ClientScopeValue { Id = c, Scope = c }).ToList(),
                    PostLogoutRedirectUris = loaded.PostLogoutRedirectUris == null ? new List<ClientPostLogoutRedirectUriValue>() : (from c in loaded.PostLogoutRedirectUris select new ClientPostLogoutRedirectUriValue { Id = c, Uri = c }).ToList(),
                    IdentityProviderRestrictions = loaded.IdentityProviderRestrictions == null ? new List<ClientIdPRestrictionValue>() : (from c in loaded.IdentityProviderRestrictions select new ClientIdPRestrictionValue { Id = c, Provider = c }).ToList(),
                    Properties = props.ToArray()
                });
            }
        }

        public async Task<IdentityAdminResult> DeleteClientAsync(string subject)
        {
            using (var s = _store.OpenAsyncSession())
            {
                s.Delete("clients/" + subject);
                await s.SaveChangesAsync();
                return IdentityAdminResult.Success;
            }
        }

        public async Task<IdentityAdminResult> AddClientClaimAsync(string subject, string type, string value)
        {
            using (var s = _store.OpenAsyncSession())
            {
                var loaded = await s.LoadAsync<Data.StoredClient>("clients/" + subject);

                if (loaded == null)
                    return new IdentityAdminResult("Invalid subject");

                if (loaded.Claims == null)
                    loaded.Claims = new List<StoredClientClaim>();

                var found = loaded.Claims.FirstOrDefault(x => x.Type == type && x.Value == value);
                if (found == null)
                {
                    loaded.Claims.Add(new Data.StoredClientClaim
                    {
                        Type = type,
                        Value = value
                    });
                    await s.StoreAsync(loaded);
                    await s.SaveChangesAsync();
                }

                return IdentityAdminResult.Success;
            }
        }

        public async Task<IdentityAdminResult> RemoveClientClaimAsync(string subject, string id)
        {
            using (var s = _store.OpenAsyncSession())
            {
                var loaded = await s.LoadAsync<Data.StoredClient>("clients/" + subject);

                if (loaded == null)
                    return new IdentityAdminResult("Invalid subject");

                var found = loaded.Claims.FirstOrDefault(x => x.Type + x.Value == id);
                if (found != null)
                {
                    loaded.Claims.Remove(found);
                    await s.StoreAsync(loaded);
                    await s.SaveChangesAsync();
                }

                return IdentityAdminResult.Success;
            }
        }

        public async Task<IdentityAdminResult> AddClientSecretAsync(string subject, string type, string value)
        {
            using (var s = _store.OpenAsyncSession())
            {
                var loaded = await s.LoadAsync<Data.StoredClient>("clients/" + subject);

                if (loaded == null)
                    return new IdentityAdminResult("Invalid subject");

                if (loaded.ClientSecrets == null)
                    loaded.ClientSecrets = new List<StoredSecret>();

                var found = loaded.ClientSecrets.FirstOrDefault(x => x.Value == value);
                if (found == null)
                {
                    loaded.ClientSecrets.Add(new Data.StoredSecret
                    {
                        Type = type,
                        Value = value
                    });

                    await s.StoreAsync(loaded);
                    await s.SaveChangesAsync();
                }

                return IdentityAdminResult.Success;
            }
        }

        public async Task<IdentityAdminResult> RemoveClientSecretAsync(string subject, string id)
        {
            using (var s = _store.OpenAsyncSession())
            {
                var loaded = await s.LoadAsync<Data.StoredClient>("clients/" + subject);

                if (loaded == null)
                    return new IdentityAdminResult("Invalid subject");

                var found = loaded.ClientSecrets.FirstOrDefault(x => x.Type + x.Value == id);
                if (found != null)
                {
                    loaded.ClientSecrets.Remove(found);
                    await s.StoreAsync(loaded);
                    await s.SaveChangesAsync();
                }

                return IdentityAdminResult.Success;
            }
        }

        public async Task<IdentityAdminResult> AddClientIdPRestrictionAsync(string subject, string provider)
        {
            using (var s = _store.OpenAsyncSession())
            {
                var loaded = await s.LoadAsync<Data.StoredClient>("clients/" + subject);

                if (loaded == null)
                    return new IdentityAdminResult("Invalid subject");

                if (loaded.IdentityProviderRestrictions == null)
                    loaded.IdentityProviderRestrictions = new List<string>();

                var found = loaded.IdentityProviderRestrictions.FirstOrDefault(x => x == provider);
                if (found == null)
                {
                    loaded.PostLogoutRedirectUris.Add(provider);
                    await s.StoreAsync(loaded);
                    await s.SaveChangesAsync();
                }

                return IdentityAdminResult.Success;
            }
        }

        public async Task<IdentityAdminResult> RemoveClientIdPRestrictionAsync(string subject, string id)
        {
            using (var s = _store.OpenAsyncSession())
            {
                var loaded = await s.LoadAsync<Data.StoredClient>("clients/" + subject);

                if (loaded == null)
                    return new IdentityAdminResult("Invalid subject");

                var found = loaded.IdentityProviderRestrictions.FirstOrDefault(x => x == id);
                if (found != null)
                {
                    loaded.IdentityProviderRestrictions.Remove(id);
                    await s.StoreAsync(loaded);
                    await s.SaveChangesAsync();
                }

                return IdentityAdminResult.Success;
            }
        }

        public async Task<IdentityAdminResult> AddPostLogoutRedirectUriAsync(string subject, string uri)
        {
            using (var s = _store.OpenAsyncSession())
            {
                var loaded = await s.LoadAsync<Data.StoredClient>("clients/" + subject);

                if (loaded == null)
                    return new IdentityAdminResult("Invalid subject");

                if (loaded.PostLogoutRedirectUris == null)
                    loaded.PostLogoutRedirectUris = new List<string>();

                var found = loaded.PostLogoutRedirectUris.FirstOrDefault(x => x == uri);
                if (found == null)
                {
                    loaded.PostLogoutRedirectUris.Add(uri);
                    await s.StoreAsync(loaded);
                    await s.SaveChangesAsync();
                }

                return IdentityAdminResult.Success;
            }
        }

        public async Task<IdentityAdminResult> RemovePostLogoutRedirectUriAsync(string subject, string id)
        {
            using (var s = _store.OpenAsyncSession())
            {
                var loaded = await s.LoadAsync<Data.StoredClient>("clients/" + subject);

                if (loaded == null)
                    return new IdentityAdminResult("Invalid subject");

                var found = loaded.PostLogoutRedirectUris.FirstOrDefault(x => x == id);
                if (found != null)
                {
                    loaded.PostLogoutRedirectUris.Remove(id);
                    await s.StoreAsync(loaded);
                    await s.SaveChangesAsync();
                }

                return IdentityAdminResult.Success;
            }
        }

        public async Task<IdentityAdminResult> AddClientRedirectUriAsync(string subject, string uri)
        {
            using (var s = _store.OpenAsyncSession())
            {
                var loaded = await s.LoadAsync<Data.StoredClient>("clients/" + subject);

                if (loaded == null)
                    return new IdentityAdminResult("Invalid subject");

                if (loaded.RedirectUris == null)
                    loaded.RedirectUris = new List<string>();

                var found = loaded.RedirectUris.FirstOrDefault(x => x == uri);
                if (found == null)
                {
                    loaded.RedirectUris.Add(uri);
                    await s.StoreAsync(loaded);
                    await s.SaveChangesAsync();
                }

                return IdentityAdminResult.Success;
            }
        }

        public async Task<IdentityAdminResult> RemoveClientRedirectUriAsync(string subject, string id)
        {
            using (var s = _store.OpenAsyncSession())
            {
                var loaded = await s.LoadAsync<Data.StoredClient>("clients/" + subject);

                if (loaded == null)
                    return new IdentityAdminResult("Invalid subject");

                var found = loaded.RedirectUris.FirstOrDefault(x => x == id);
                if (found != null)
                {
                    loaded.RedirectUris.Remove(id);
                    await s.StoreAsync(loaded);
                    await s.SaveChangesAsync();
                }

                return IdentityAdminResult.Success;
            }
        }

        public async Task<IdentityAdminResult> AddClientCorsOriginAsync(string subject, string origin)
        {
            using (var s = _store.OpenAsyncSession())
            {
                var loaded = await s.LoadAsync<Data.StoredClient>("clients/" + subject);

                if (loaded == null)
                    return new IdentityAdminResult("Invalid subject");

                if (loaded.AllowedCorsOrigins == null)
                    loaded.AllowedCorsOrigins = new List<string>();

                var found = loaded.AllowedCorsOrigins.FirstOrDefault(x => x == origin);
                if (found == null)
                {
                    loaded.AllowedCorsOrigins.Add(origin);
                    await s.StoreAsync(loaded);
                    await s.SaveChangesAsync();
                }

                return IdentityAdminResult.Success;
            }
        }

        public async Task<IdentityAdminResult> RemoveClientCorsOriginAsync(string subject, string id)
        {
            using (var s = _store.OpenAsyncSession())
            {
                var loaded = await s.LoadAsync<Data.StoredClient>("clients/" + subject);

                if (loaded == null)
                    return new IdentityAdminResult("Invalid subject");

                var found = loaded.AllowedCorsOrigins.FirstOrDefault(x => x == id);
                if (found != null)
                {
                    loaded.AllowedCorsOrigins.Remove(id);
                    await s.StoreAsync(loaded);
                    await s.SaveChangesAsync();
                }

                return IdentityAdminResult.Success;
            }
        }

        public async Task<IdentityAdminResult> AddClientCustomGrantTypeAsync(string subject, string grantType)
        {
            using (var s = _store.OpenAsyncSession())
            {
                var loaded = await s.LoadAsync<Data.StoredClient>("clients/" + subject);

                if (loaded == null)
                    return new IdentityAdminResult("Invalid subject");

                if (loaded.AllowedCustomGrantTypes == null)
                    loaded.AllowedCustomGrantTypes = new List<string>();

                var found = loaded.AllowedCustomGrantTypes.FirstOrDefault(x => x == grantType);
                if (found == null)
                {
                    loaded.AllowedCustomGrantTypes.Add(grantType);
                    await s.StoreAsync(loaded);
                    await s.SaveChangesAsync();
                }

                return IdentityAdminResult.Success;
            }
        }

        public async Task<IdentityAdminResult> RemoveClientCustomGrantTypeAsync(string subject, string id)
        {
            using (var s = _store.OpenAsyncSession())
            {
                var loaded = await s.LoadAsync<Data.StoredClient>("clients/" + subject);

                if (loaded == null)
                    return new IdentityAdminResult("Invalid subject");

                var found = loaded.AllowedCustomGrantTypes.FirstOrDefault(x => x == id);
                if (found != null)
                {
                    loaded.AllowedCustomGrantTypes.Remove(id);
                    await s.StoreAsync(loaded);
                    await s.SaveChangesAsync();
                }

                return IdentityAdminResult.Success;
            }
        }

        public async Task<IdentityAdminResult> AddClientScopeAsync(string subject, string scope)
        {
            using (var s = _store.OpenAsyncSession())
            {
                var loaded = await s.LoadAsync<Data.StoredClient>("clients/" + subject);

                if (loaded == null)
                    return new IdentityAdminResult("Invalid subject");

                if (loaded.AllowedScopes == null)
                    loaded.AllowedScopes = new List<string>();

                var found = loaded.AllowedScopes.FirstOrDefault(x => x == scope);
                if (found == null)
                {
                    loaded.AllowedScopes.Add(scope);
                    await s.StoreAsync(loaded);
                    await s.SaveChangesAsync();
                }

                return IdentityAdminResult.Success;
            }
        }

        public async Task<IdentityAdminResult> RemoveClientScopeAsync(string subject, string id)
        {
            using (var s = _store.OpenAsyncSession())
            {
                var loaded = await s.LoadAsync<Data.StoredClient>("clients/" + subject);

                if (loaded == null)
                    return new IdentityAdminResult("Invalid subject");

                var found = loaded.AllowedScopes.FirstOrDefault(x => x == id);
                if (found != null)
                {
                    loaded.AllowedScopes.Remove(id);
                    await s.StoreAsync(loaded);
                    await s.SaveChangesAsync();
                }

                return IdentityAdminResult.Success;
            }
        }

        public async Task<IdentityAdminResult<QueryResult<ClientSummary>>> QueryClientsAsync(string filter, int start, int count)
        {
            using (var s = _store.OpenAsyncSession())
            {
                RavenQueryStatistics stat;
                var q = s.Query<Data.StoredClient, Indexes.AdminClientQuery>().Statistics(out stat).Take(count).Skip(start);
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
                                Subject = c.Id.Replace("clients/", "")
                            }
                });
            }
        }

        public async Task<IdentityAdminResult<CreateResult>> CreateClientAsync(IEnumerable<PropertyValue> properties)
        {
            var clientNameClaim = properties.Single(x => x.Type == "ClientName");
            var clientIdClaim = properties.Single(x => x.Type == "ClientId");

            var clientId = clientIdClaim.Value;
            var clientName = clientNameClaim.Value;

            string[] exclude = new string[] { "ClientName", "ClientId" };
            var otherProperties = properties.Where(x => !exclude.Contains(x.Type)).ToArray();

            var metadata = await GetMetadataAsync();
            var createProps = metadata.ClientMetaData.CreateProperties;

            var client = new Data.StoredClient { ClientId = clientId, ClientName = clientName };
            foreach (var prop in otherProperties)
            {
                var propertyResult = SetClientProperty(createProps, client, prop.Type, prop.Value);
                if (!propertyResult.IsSuccess)
                {
                    return new IdentityAdminResult<CreateResult>(propertyResult.Errors.ToArray());
                }
            }

            client.Enabled = true;
            client.EnableLocalLogin = true;
            client.RequireConsent = true;
            client.Flow = Flows.Implicit.ToString();
            client.AllowClientCredentialsOnly = false;
            client.IdentityTokenLifetime = 300;
            client.AccessTokenLifetime = 3600;
            client.AuthorizationCodeLifetime = 300;
            client.AbsoluteRefreshTokenLifetime = 300;
            client.SlidingRefreshTokenLifetime = 1296000;
            client.AccessTokenType = AccessTokenType.Jwt.ToString();
            client.AlwaysSendClientClaims = false;
            client.PrefixClientClaims = true;

            using (var s = _store.OpenAsyncSession())
            {
                client.Id = "clients/" + clientId;
                s.Advanced.UseOptimisticConcurrency = true;
                await s.StoreAsync(client);
                await s.SaveChangesAsync();
            }

            return new IdentityAdminResult<CreateResult>(new CreateResult { Subject = clientId });
        }

        public async Task<IdentityAdminResult> SetClientPropertyAsync(string subject, string type, string value)
        {
            using (var s = _store.OpenAsyncSession())
            {
                var loaded = await s.LoadAsync<Data.StoredClient>("clients/" + subject);

                if (loaded == null)
                    return new IdentityAdminResult("Invalid subject");

                var meta = await GetMetadataAsync();
                var propResult = SetClientProperty(meta.ClientMetaData.UpdateProperties, loaded, type, value);
                if (!propResult.IsSuccess)
                {
                    return propResult;
                }

                await s.StoreAsync(loaded);
                await s.SaveChangesAsync();

                return IdentityAdminResult.Success;
            }
        }

        #region helperMethods
        protected IdentityAdminResult SetClientProperty(IEnumerable<PropertyMetadata> propsMeta, Data.StoredClient client, string type, string value)
        {
            IdentityAdminResult result;
            if (propsMeta.TrySet(client, type, value, out result))
            {
                return result;
            }

            throw new Exception("Invalid property type " + type);
        }

        protected string GetClientProperty(PropertyMetadata propMetadata, Data.StoredClient client)
        {
            string val;
            if (propMetadata.TryGet(client, out val))
            {
                return val;
            }
            throw new Exception("Invalid property type " + propMetadata.Type);
        }

        protected IdentityAdminResult SetScopeProperty(IEnumerable<PropertyMetadata> propsMeta, Data.StoredScope scope, string type, string value)
        {
            IdentityAdminResult result;
            if (propsMeta.TrySet(scope, type, value, out result))
            {
                return result;
            }

            throw new Exception("Invalid property type " + type);
        }

        protected string GetScopeProperty(PropertyMetadata propMetadata, Data.StoredScope scope)
        {
            string val;
            if (propMetadata.TryGet(scope, out val))
            {
                return val;
            }
            throw new Exception("Invalid property type " + propMetadata.Type);
        }

        public Task<IdentityAdminResult> AddScopeSecretAsync(string subject, string type, string value, string description, DateTime? expiration)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityAdminResult> UpdateScopeSecret(string subject, string scopeSecretSubject, string type, string value, string description, DateTime? expiration)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityAdminResult> RemoveScopeSecretAsync(string subject, string id)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityAdminResult> UpdateScopeClaim(string subject, string scopeClaimSubject, string name, string description, bool alwaysIncludeInIdToken)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
