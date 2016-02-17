using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;

namespace Identityserver.Contrib.RavenDB
{
    public class AuthorizationCodeStore : IAuthorizationCodeStore
    {
        public Task StoreAsync(string key, AuthorizationCode value)
        {
            throw new NotImplementedException();
        }

        public Task<AuthorizationCode> GetAsync(string key)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(string key)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ITokenMetadata>> GetAllAsync(string subject)
        {
            throw new NotImplementedException();
        }

        public Task RevokeAsync(string subject, string client)
        {
            throw new NotImplementedException();
        }
    }
}
