using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;

namespace Identityserver.Contrib.RavenDB.Data
{
    public class StoredClientClaim
    {
        public string Type { get; set; }
        public string Value { get; set; }
        public string ValueType { get; set; }

        internal static StoredClientClaim ToDbFormat(Claim claim)
        {
            return new StoredClientClaim
            {
                Type = claim.Type,
                Value = claim.Value,
                ValueType = claim.ValueType
            };
        }

        internal static Claim FromDbFormat(StoredClientClaim claim)
        {
            return new Claim(claim.Type, claim.Value, claim.ValueType);
        }
    }

    public class StoredClient
    {
        public string Id { get; set; }

        public int AbsoluteRefreshTokenLifetime { get; set; }

        public int AccessTokenLifetime { get; set; }
        
        public string AccessTokenType { get; set; }
        
        public bool AllowAccessToAllCustomGrantTypes { get; set; }
        
        public bool AllowAccessToAllScopes { get; set; }
        
        public bool AllowClientCredentialsOnly { get; set; }

        public List<string> AllowedCorsOrigins { get; set; } = new List<string>();

        public List<string> AllowedCustomGrantTypes { get; set; } = new List<string>();

        public List<string> AllowedScopes { get; set; } = new List<string>();
        
        public bool AllowRememberConsent { get; set; }
        
        public bool AlwaysSendClientClaims { get; set; }
        
        public int AuthorizationCodeLifetime { get; set; }

        public List<StoredClientClaim> Claims { get; set; } = new List<StoredClientClaim>();
        
        public string ClientId { get; set; }
        
        public string ClientName { get; set; }

        public List<StoredSecret> ClientSecrets { get; set; } = new List<StoredSecret>();
        
        public string ClientUri { get; set; }
        
        public bool Enabled { get; set; }
        
        public bool EnableLocalLogin { get; set; }
        
        public string Flow { get; set; }

        public List<string> IdentityProviderRestrictions { get; set; } = new List<string>();
        
        public int IdentityTokenLifetime { get; set; }
        
        public bool IncludeJwtId { get; set; }
        
        public string LogoUri { get; set; }
        
        public bool LogoutSessionRequired { get; set; }
        
        public string LogoutUri { get; set; }

        public List<string> PostLogoutRedirectUris { get; set; } = new List<string>();
        
        public bool PrefixClientClaims { get; set; }

        public List<string> RedirectUris { get; set; } = new List<string>();
        
        public string RefreshTokenExpiration { get; set; }
        
        public string RefreshTokenUsage { get; set; }
        
        public bool RequireConsent { get; set; }
        
        public bool RequireSignOutPrompt { get; set; }
        
        public int SlidingRefreshTokenLifetime { get; set; }
        
        public bool UpdateAccessTokenClaimsOnRefresh { get; set; }

        internal static Client FromDbFormat(StoredClient client)
        {
            return new Client
            {
                ClientId = client.ClientId,
                Enabled = client.Enabled,
                AbsoluteRefreshTokenLifetime = client.AbsoluteRefreshTokenLifetime,
                AccessTokenLifetime = client.AccessTokenLifetime,
                AccessTokenType = (AccessTokenType)Enum.Parse(typeof(AccessTokenType),client.AccessTokenType),
                AllowAccessToAllCustomGrantTypes = client.AllowAccessToAllCustomGrantTypes,
                AllowAccessToAllScopes = client.AllowAccessToAllScopes,
                AllowClientCredentialsOnly = client.AllowClientCredentialsOnly,
                AllowRememberConsent = client.AllowRememberConsent,
                AllowedCorsOrigins = client.AllowedCorsOrigins,
                AllowedCustomGrantTypes = client.AllowedCustomGrantTypes,
                AllowedScopes = client.AllowedScopes,
                AlwaysSendClientClaims = client.AlwaysSendClientClaims,
                AuthorizationCodeLifetime = client.AuthorizationCodeLifetime,
                ClientName = client.ClientName,
                ClientUri = client.ClientUri,
                EnableLocalLogin = client.EnableLocalLogin,
                Flow = (Flows)Enum.Parse(typeof(Flows), client.Flow),
                IdentityProviderRestrictions = client.IdentityProviderRestrictions,
                IdentityTokenLifetime = client.IdentityTokenLifetime,
                IncludeJwtId = client.IncludeJwtId,
                LogoUri = client.LogoUri,
                LogoutSessionRequired = client.LogoutSessionRequired,
                LogoutUri = client.LogoutUri,
                PostLogoutRedirectUris = client.PostLogoutRedirectUris,
                PrefixClientClaims = client.PrefixClientClaims,
                RedirectUris = client.RedirectUris,
                RefreshTokenExpiration = (TokenExpiration)Enum.Parse(typeof(TokenExpiration), client.RefreshTokenExpiration),
                RefreshTokenUsage = (TokenUsage)Enum.Parse(typeof(TokenUsage), client.RefreshTokenUsage),
                RequireConsent = client.RequireConsent,
                RequireSignOutPrompt = client.RequireSignOutPrompt,
                SlidingRefreshTokenLifetime = client.SlidingRefreshTokenLifetime,
                UpdateAccessTokenClaimsOnRefresh = client.UpdateAccessTokenClaimsOnRefresh,
                ClientSecrets = (from s in client.ClientSecrets select StoredSecret.FromDbFormat(s)).ToList(),
                Claims = (from c in client.Claims select StoredClientClaim.FromDbFormat(c)).ToList()
            };
        }

        public static StoredClient ToDbFormat(Client client)
        {
            return new StoredClient
            {
                Id = "clients/" + client.ClientId,
                ClientId = client.ClientId,
                Enabled = client.Enabled,
                AbsoluteRefreshTokenLifetime = client.AbsoluteRefreshTokenLifetime,
                AccessTokenLifetime = client.AccessTokenLifetime,
                AccessTokenType = client.AccessTokenType.ToString(),
                AllowAccessToAllCustomGrantTypes = client.AllowAccessToAllCustomGrantTypes,
                AllowAccessToAllScopes = client.AllowAccessToAllScopes,
                AllowClientCredentialsOnly = client.AllowClientCredentialsOnly,
                AllowRememberConsent = client.AllowRememberConsent,
                AllowedCorsOrigins = client.AllowedCorsOrigins,
                AllowedCustomGrantTypes = client.AllowedCustomGrantTypes,
                AllowedScopes = client.AllowedScopes,
                AlwaysSendClientClaims = client.AlwaysSendClientClaims,
                AuthorizationCodeLifetime = client.AuthorizationCodeLifetime,
                ClientName = client.ClientName,
                ClientUri = client.ClientUri,
                EnableLocalLogin = client.EnableLocalLogin,
                Flow = client.Flow.ToString(),
                IdentityProviderRestrictions = client.IdentityProviderRestrictions,
                IdentityTokenLifetime = client.IdentityTokenLifetime,
                IncludeJwtId = client.IncludeJwtId,
                LogoUri = client.LogoUri,
                LogoutSessionRequired = client.LogoutSessionRequired,
                LogoutUri = client.LogoutUri,
                PostLogoutRedirectUris = client.PostLogoutRedirectUris,
                PrefixClientClaims = client.PrefixClientClaims,
                RedirectUris = client.RedirectUris,
                RefreshTokenExpiration = client.RefreshTokenExpiration.ToString(),
                RefreshTokenUsage = client.RefreshTokenUsage.ToString(),
                RequireConsent = client.RequireConsent,
                RequireSignOutPrompt = client.RequireSignOutPrompt,
                SlidingRefreshTokenLifetime = client.SlidingRefreshTokenLifetime,
                UpdateAccessTokenClaimsOnRefresh = client.UpdateAccessTokenClaimsOnRefresh,
                ClientSecrets = (from s in client.ClientSecrets select StoredSecret.ToDbFormat(s)).ToList(),
                Claims = (from c in client.Claims select StoredClientClaim.ToDbFormat(c)).ToList()
            };
        }
    }
}
