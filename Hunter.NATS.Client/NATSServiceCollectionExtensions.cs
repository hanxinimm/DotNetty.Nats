using Hunter.NATS.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Text.RegularExpressions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class NATSServiceCollectionExtensions
    {
        private static readonly Regex _clientIdReplacer = new Regex("[^A-Za-z0-9_]");

        public static void AddNATSServer(this IServiceCollection services, Action<NATSOptions> steup)
        {
            services.Configure(steup);
            services.AddTransient<NATSClient>();
        }

        public static void AddNATSServer(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.Configure<NATSOptions>(options =>
            {
                options.ClientId = "NATSClientId";
                configuration.GetSection("NATSOptions").Bind(options);
            });
            services.AddSingleton<NATSClient>();
        }

        public static void AddNATSServer(this IServiceCollection services, IConfigurationRoot configuration, string clientId)
        {
            services.Configure<NATSOptions>(options =>
            {
                options.ClientId = _clientIdReplacer.Replace(clientId, "_");
                configuration.GetSection("NATSOptions").Bind(options);
            });

            services.AddSingleton<NATSClient>();
        }
    }
}
