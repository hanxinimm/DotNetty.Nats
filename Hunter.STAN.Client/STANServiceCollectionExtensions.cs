
using Hunter.STAN.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class STANServiceCollectionExtensions
    {
        private static readonly Regex _clientIdReplacer = new Regex("[^A-Za-z0-9_]");

        public static void AddSTANServer(this IServiceCollection services, 
            Action<STANOptions> steup,
            ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
        {
            services.Configure(steup);
            services.Add(new ServiceDescriptor(typeof(STANClient), serviceLifetime));
        }

        public static void AddSTANServer(this IServiceCollection services,
            IConfigurationRoot configuration,
            ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
        {
            services.Configure<STANOptions>(options =>
            {
                options.ClientId = "STANClientId";
                configuration.GetSection("STANOptions").Bind(options);
            });
            services.Add(new ServiceDescriptor(typeof(STANClient), serviceLifetime));
        }

        public static void AddSTANServer(this IServiceCollection services,
            IConfigurationRoot configuration, 
            string clientId,
            ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
        {
            services.Configure<STANOptions>(options =>
            {
                options.ClientId = _clientIdReplacer.Replace(clientId, "_");
                configuration.GetSection("STANOptions").Bind(options);
            });
            services.Add(new ServiceDescriptor(typeof(STANClient), serviceLifetime));
        }
    }
}
