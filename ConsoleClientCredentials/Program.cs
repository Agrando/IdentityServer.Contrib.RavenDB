using Newtonsoft.Json.Linq;
using System;
using System.CodeDom;
using System.Net.Http;
using System.Text;
using IdentityModel;
using IdentityModel.Client;

namespace ConsoleClientCredentialsClient
{
    public static class Constants
    {
        public const string BaseAddress = "http://localhost:9921/core";

        public const string AuthorizeEndpoint = BaseAddress + "/connect/authorize";
        public const string LogoutEndpoint = BaseAddress + "/connect/endsession";
        public const string TokenEndpoint = BaseAddress + "/connect/token";
        public const string UserInfoEndpoint = BaseAddress + "/connect/userinfo";
        public const string IdentityTokenValidationEndpoint = BaseAddress + "/connect/identitytokenvalidation";
        public const string TokenRevocationEndpoint = BaseAddress + "/connect/revocation";

        public const string AspNetWebApiSampleApi = "http://localhost:2727/";
    }

    class Program
    {
        private static void Main(string[] args)
        {
            var tokenClient = new TokenClient(Constants.TokenEndpoint, "roclient", "secret");
            var response = tokenClient.RequestResourceOwnerPasswordAsync("bob", "bob", "read write offline_access").Result;

            var refresh_token = response.RefreshToken;
            while (true)
            {
                response = RefreshToken(refresh_token, tokenClient);
                Console.ReadLine();
                //CallService(response.AccessToken);

                if (response.RefreshToken != refresh_token)
                {
                    refresh_token = response.RefreshToken;
                }
            }
        }

        private static TokenResponse RefreshToken(string refreshToken, TokenClient tokenClient)
        {
            Console.WriteLine("Using refresh token: {0}", refreshToken);
            return tokenClient.RequestRefreshTokenAsync(refreshToken).Result;
        }
    }
}