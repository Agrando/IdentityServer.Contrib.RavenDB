using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;

namespace Identityserver.Contrib.RavenDB.Data
{
    public class StoredScopeClaim
    {
        public bool AlwaysIncludeInIdToken { get; set; }
        
        public string Description { get; set; }
        
        public string Name { get; set; }

        internal static StoredScopeClaim ToDbFormat(ScopeClaim scopeClaim)
        {
            return new StoredScopeClaim
            {
                Name = scopeClaim.Name,
                AlwaysIncludeInIdToken = scopeClaim.AlwaysIncludeInIdToken,
                Description = scopeClaim.Description
            };
        }

        internal static ScopeClaim FromDbFormat(StoredScopeClaim scopeClaim)
        {
            return new ScopeClaim
            {
                Name = scopeClaim.Name,
                Description = scopeClaim.Description,
                AlwaysIncludeInIdToken = scopeClaim.AlwaysIncludeInIdToken
            };
        }
    }

    public class StoredSecret
    {
        public string Description { get; set; }
        
        public DateTimeOffset? Expiration { get; set; }
        
        public string Type { get; set; }

        public string Value { get; set; }

        internal static StoredSecret ToDbFormat(Secret secret)
        {
            return new StoredSecret
            {
                Description = secret.Description,
                Type = secret.Type,
                Value = secret.Value,
                Expiration = secret.Expiration
            };
        }

        internal static Secret FromDbFormat(StoredSecret secret)
        {
            return new Secret
            {
                Description = secret.Description,
                Type = secret.Type,
                Value = secret.Value,
                Expiration = secret.Expiration
            };
        }
    }

    public class StoredScope
    {
        public string Id { get; set; }

        public bool AllowUnrestrictedIntrospection { get; set; }

        public List<StoredScopeClaim> Claims { get; set; }

        public string ClaimsRule { get; set; }

        public string Description { get; set; }

        public string DisplayName { get; set; }

        public bool Emphasize { get; set; }

        public bool Enabled { get; set; }

        public bool IncludeAllClaimsForUser { get; set; }

        public string Name { get; set; }

        public bool Required { get; set; }

        public List<StoredSecret> ScopeSecrets { get; set; }

        public bool ShowInDiscoveryDocument { get; set; }

        public string Type { get; set; }


        public static StoredScope ToDbFormat(Scope scope)
        {
            return new StoredScope
            {
                Id = "scopes/" + scope.Name,
                ShowInDiscoveryDocument = scope.ShowInDiscoveryDocument,
                Name =  scope.Name,
                AllowUnrestrictedIntrospection = scope.AllowUnrestrictedIntrospection,
                ClaimsRule = scope.ClaimsRule,
                Description =  scope.Description,
                DisplayName = scope.DisplayName,
                Emphasize = scope.Emphasize,
                Enabled = scope.Enabled,
                IncludeAllClaimsForUser = scope.IncludeAllClaimsForUser,
                Required = scope.Required,
                Type = scope.Type.ToString(),

                Claims = (from c in scope.Claims select StoredScopeClaim.ToDbFormat(c)).ToList(),
                ScopeSecrets = (from s in scope.ScopeSecrets select StoredSecret.ToDbFormat(s)).ToList()
            };
        }

        internal static Scope FromDbFormat(StoredScope scope)
        {
            return new Scope
            {
                Name = scope.Name,
                ShowInDiscoveryDocument = scope.ShowInDiscoveryDocument,
                Description = scope.Description,
                Type = (ScopeType)Enum.Parse(typeof(ScopeType),scope.Type),
                AllowUnrestrictedIntrospection = scope.AllowUnrestrictedIntrospection,
                ClaimsRule = scope.ClaimsRule,
                DisplayName = scope.DisplayName,
                Emphasize = scope.Emphasize,
                Enabled = scope.Enabled,
                IncludeAllClaimsForUser = scope.IncludeAllClaimsForUser,
                Required = scope.Required,
                ScopeSecrets = (from s in scope.ScopeSecrets select StoredSecret.FromDbFormat(s)).ToList(),
                Claims = (from c in scope.Claims select StoredScopeClaim.FromDbFormat(c)).ToList()
            };
        }
    }
}
