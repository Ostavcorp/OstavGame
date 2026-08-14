using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Ostav.Api
{
    public interface IApiRequestAuthenticator { bool Authenticate(string suppliedKey); }
    public sealed class DevelopmentApiKeyAuthenticator : IApiRequestAuthenticator
    {
        private readonly byte[] expected;
        public DevelopmentApiKeyAuthenticator(IConfiguration configuration)
        {
            string value=configuration["Ostav:ApiKey"];
            expected=string.IsNullOrEmpty(value)?null:Encoding.UTF8.GetBytes(value);
        }
        public bool Authenticate(string suppliedKey)
        {
            if(expected==null||string.IsNullOrEmpty(suppliedKey))return false;
            byte[] supplied=Encoding.UTF8.GetBytes(suppliedKey);
            return supplied.Length==expected.Length&&CryptographicOperations.FixedTimeEquals(supplied,expected);
        }
    }
}
