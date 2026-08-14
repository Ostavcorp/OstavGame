using System;
using Microsoft.Extensions.Configuration;

namespace Ostav.Api
{
    public static class ApiHostConfiguration
    {
        public static string ResolveHttpUrl(IConfiguration configuration)
        {
            if(configuration==null)throw new ArgumentNullException(nameof(configuration));
            string configuredPort=configuration["PORT"]??configuration["Ostav:Port"];
            if(string.IsNullOrWhiteSpace(configuredPort))return null;
            if(!int.TryParse(configuredPort,out int port)||port<1||port>65535)throw new InvalidOperationException("PORT must be a valid TCP port.");
            return "http://0.0.0.0:"+port;
        }
    }
}
