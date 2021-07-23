using Hunter.NATS.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Text.RegularExpressions;
using Hunter.Extensions.Cryptography;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class NATSServiceCollectionExtensions
    {
        private static readonly Regex _clientIdReplacer = new Regex("[^A-Za-z0-9_]");

        public static void AddNATSServer(this IServiceCollection services,
            Action<NATSOptions> steup,
            ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
        {
            services.Configure(steup);
            services.PostConfigure<NATSOptions>(options =>
            {
                if (options.IsAuthentication && (AppEnvironment.IsProduction || HostEnvironment.IsProduction))
                {
                    options.UserName = options.UserName.DecryptDES();
                    options.Password = options.Password.DecryptDES();
                }
            });
            services.Add(new ServiceDescriptor(typeof(NATSClient),
                spr => new NATSClient(spr.GetService<ILogger<NATSClient>>(), spr.GetService<IOptions<NATSOptions>>()),
                serviceLifetime));
        }

        public static void AddNATSServer(this IServiceCollection services,
            IConfigurationRoot configuration, 
            ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
        {
            services.Configure<NATSOptions>(options =>
            {
                options.ClientId = "NATSClient";
                configuration.GetSection("NATSOptions").Bind(options);
            });
            services.PostConfigure<NATSOptions>(options =>
            {
                if (options.IsAuthentication && (AppEnvironment.IsProduction || HostEnvironment.IsProduction))
                {
                    options.UserName = options.UserName.DecryptDES();
                    options.Password = options.Password.DecryptDES();
                }
            });

            services.Add(new ServiceDescriptor(typeof(NATSClient),
                spr => new NATSClient(spr.GetService<ILogger<NATSClient>>(), spr.GetService<IOptions<NATSOptions>>()),
                serviceLifetime));
        }

        public static void AddNATSServer(this IServiceCollection services,
            IConfigurationRoot configuration,
            string clientId,
            ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
        {
            services.Configure<NATSOptions>(options =>
            {
                options.ClientId = _clientIdReplacer.Replace(clientId, "_");
                configuration.GetSection("NATSOptions").Bind(options);
            });
            services.PostConfigure<NATSOptions>(options =>
            {
                if (options.IsAuthentication && (AppEnvironment.IsProduction || HostEnvironment.IsProduction))
                {
                    options.UserName = options.UserName.DecryptDES();
                    options.Password = options.Password.DecryptDES();
                }
            });
            services.Add(new ServiceDescriptor(typeof(NATSClient),
                spr => new NATSClient(spr.GetService<ILogger<NATSClient>>(), spr.GetService<IOptions<NATSOptions>>()),
                serviceLifetime));
        }
    }
}
