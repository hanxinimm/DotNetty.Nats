using Hunter.NATS.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class NATSServiceCollectionExtensions
    {
        private static readonly Regex _clientIdReplacer = new Regex("[^A-Za-z0-9_]");
        public static void AddNATSServer(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.Configure<NATSOptions>(options => configuration.GetSection("NATSOptions").Bind(options));
        }

        public static void AddNATSServer(this IServiceCollection services, IConfigurationRoot configuration, string clientId)
        {
            services.Configure<NATSOptions>(options =>
            {
                options.ClientId = $"{_clientIdReplacer.Replace(clientId, "_")}-{Guid.NewGuid().ToString("N")}";
                configuration.GetSection("NATSOptions").Bind(options);
            });

            services.AddSingleton(spr => new NATSClient(spr.GetRequiredService<IOptions<NATSOptions>>()));
        }
    }
}
