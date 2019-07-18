using Hunter.NATS.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class NATSServiceCollectionExtensions
    {
        public static void AddNATSServer(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.Configure<NATSOptions>(options => configuration.GetSection("NATSOptions").Bind(options));
        }

        public static void AddNATSServer(this IServiceCollection services, IConfigurationRoot configuration, string clientId)
        {
            services.Configure<NATSOptions>(options =>
            {
                configuration.GetSection("NATSOptions").Bind(options);
                options.ClientId = clientId;
            });
        }
    }
}
