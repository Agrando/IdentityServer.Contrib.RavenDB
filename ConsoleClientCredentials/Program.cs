using Newtonsoft.Json.Linq;
using System;
using System.CodeDom;
using System.Net.Http;
using System.Text;
using Thinktecture.IdentityModel.Client;

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
        static OAuth2Client _tokenClient;

        static void Main(string[] args)
        {
            var response = GetUserToken();
            ShowResponse(response);

            Console.ReadLine();

            var refresh_token = response.RefreshToken;

            while (true)
            {
                response = RefreshToken(refresh_token);
                ShowResponse(response);

                Console.ReadLine();
                //CallService(response.AccessToken);

                if (response.RefreshToken != refresh_token)
                {
                    refresh_token = response.RefreshToken;
                }
            }
        }

        private static TokenResponse RefreshToken(string refreshToken)
        {
            Console.WriteLine("Using refresh token: {0}", refreshToken);

            return _tokenClient.RequestRefreshTokenAsync(refreshToken).Result;
        }

        static TokenResponse GetUserToken()
        {
            _tokenClient = new OAuth2Client(new Uri(Constants.TokenEndpoint), "roclient", "secret");
            var token = _tokenClient.RequestResourceOwnerPasswordAsync("bob", "bob", "read write offline_access").Result;
            return token;
        }

        static void CallService(string token)
        {
            var baseAddress = Constants.AspNetWebApiSampleApi;

            var client = new HttpClient
            {
                BaseAddress = new Uri(baseAddress)
            };

            client.SetBearerToken(token);
            var response = client.GetStringAsync("identity").Result;

            Console.WriteLine("\n\nService claims:");
            Console.WriteLine(JArray.Parse(response));
        }

        private static void ShowResponse(TokenResponse response)
        {
            if (!response.IsError)
            {
                Console.WriteLine("Token response:");
                Console.WriteLine(response.Json);

                if (response.AccessToken.Contains("."))
                {
                    Console.WriteLine("\nAccess Token (decoded):");

                    var parts = response.AccessToken.Split('.');
                    var header = parts[0];
                    var claims = parts[1];

                    Console.WriteLine(JObject.Parse(Encoding.UTF8.GetString(Base64Url.Decode(header))));
                    Console.WriteLine(JObject.Parse(Encoding.UTF8.GetString(Base64Url.Decode(claims))));
                }
            }
            else
            {
                if (response.IsHttpError)
                {
                    Console.WriteLine("HTTP error: ");
                    Console.WriteLine(response.HttpErrorStatusCode);
                    Console.WriteLine("HTTP error reason: ");
                    Console.WriteLine(response.HttpErrorReason);
                }
                else
                {
                    Console.WriteLine("Protocol error response:");
                    Console.WriteLine(response.Json);
                }
            }
        }
    }
}