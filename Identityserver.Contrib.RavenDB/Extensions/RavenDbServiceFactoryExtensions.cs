/*
 * Copyright 2014 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using Identityserver.Contrib.RavenDB;
using Identityserver.Contrib.RavenDB.Registrations;
using IdentityServer3.Core.Services;
using Raven.Client;
using Raven.Client.Document;

namespace IdentityServer3.Core.Configuration
{
    public static class IdentityServerServiceFactoryExtensions
    {
        public static void RegisterOperationalServices(this IdentityServerServiceFactory factory, RavenDbServiceOptions options)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            if (options == null) throw new ArgumentNullException("options");

            factory.Register(new Registration<IDocumentStore>(options.Store));            
        }

        public static void RegisterConfigurationServices(this IdentityServerServiceFactory factory, RavenDbServiceOptions options, TimeSpan? replicationWaitingTime = null)
        {
            factory.RegisterClientStore(options);
            factory.RegisterScopeStore(options);
            factory.RegisterAuthorizationCodeStore(options, replicationWaitingTime);
            factory.RegisterTokenHandleStore(options);
            factory.RegisterConsentStore(options);
            factory.RegisterRefreshTokenStore(options);
        }

        public static void RegisterClientStore(this IdentityServerServiceFactory factory, RavenDbServiceOptions options)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            if (options == null) throw new ArgumentNullException("options");

            factory.Register(new Registration<IDocumentStore>(options.Store));
            factory.ClientStore = new Registration<IClientStore, ClientStore>();
            factory.CorsPolicyService = new ClientConfigurationCorsPolicyRegistration(options);
        }

        public static void RegisterScopeStore(this IdentityServerServiceFactory factory, RavenDbServiceOptions options)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            if (options == null) throw new ArgumentNullException("options");

            factory.Register(new Registration<IDocumentStore>(options.Store));
            factory.ScopeStore = new Registration<IScopeStore, ScopeStore>();
        }

        public static void RegisterTokenHandleStore(this IdentityServerServiceFactory factory, RavenDbServiceOptions options)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            if (options == null) throw new ArgumentNullException("options");

            factory.Register(new Registration<IDocumentStore>(options.Store));
            factory.TokenHandleStore = new Registration<ITokenHandleStore, TokenHandleStore>();
        }

        public static void RegisterRefreshTokenStore(this IdentityServerServiceFactory factory, RavenDbServiceOptions options)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            if (options == null) throw new ArgumentNullException("options");

            factory.Register(new Registration<IDocumentStore>(options.Store));
            factory.RefreshTokenStore = new Registration<IRefreshTokenStore, RefreshTokenStore>();
        }

        public static void RegisterConsentStore(this IdentityServerServiceFactory factory, RavenDbServiceOptions options)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            if (options == null) throw new ArgumentNullException("options");

            factory.Register(new Registration<IDocumentStore>(options.Store));
            factory.ConsentStore = new Registration<IConsentStore, ConsentStore>();
        }

        public static void RegisterAuthorizationCodeStore(this IdentityServerServiceFactory factory, RavenDbServiceOptions options, TimeSpan? replicationWaitingTime)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            if (options == null) throw new ArgumentNullException("options");

            factory.Register(new Registration<IDocumentStore>(options.Store));
            factory.AuthorizationCodeStore = new Registration<IAuthorizationCodeStore>((dr) =>
            {
                var docStore = dr.Resolve<IDocumentStore>();
                var scopeStore = dr.Resolve<IScopeStore>();

                return new AuthorizationCodeStore(docStore, scopeStore)
                {
                    AuthCodeReplicationWaitingTime = replicationWaitingTime
                };
            });
        }
    }
}