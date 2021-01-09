
using Hunter.Extensions.Cryptography;
using Hunter.STAN.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        {
            services.Configure(steup);
            services.PostConfigure<STANOptions>(options =>
            {
                if (options.IsAuthentication && (AppEnvironment.IsProduction || HostEnvironment.IsProduction))
                {
                    options.UserName = options.UserName.DecryptDES();
                    options.Password = options.Password.DecryptDES();
                }
            });
            services.Add(new ServiceDescriptor(typeof(STANClient),
                spr => new STANClient(spr.GetService<ILogger<STANClient>>(), spr.GetService<IOptions<STANOptions>>()),
                serviceLifetime));
        }

        public static void AddSTANServer(this IServiceCollection services,
            IConfigurationRoot configuration,
            ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        {
            services.Configure<STANOptions>(options =>
            {
                options.ClientId = $"STANClient_{Guid.NewGuid():N}";
                configuration.GetSection("STANOptions").Bind(options);
            });
            services.PostConfigure<STANOptions>(options =>
            {
                if (options.IsAuthentication && (AppEnvironment.IsProduction || HostEnvironment.IsProduction))
                {
                    options.UserName = options.UserName.DecryptDES();
                    options.Password = options.Password.DecryptDES();
                }
            });
            services.Add(new ServiceDescriptor(typeof(STANClient),
                spr => new STANClient(spr.GetService<ILogger<STANClient>>(), spr.GetService<IOptions<STANOptions>>()),
                serviceLifetime));
        }

        public static void AddSTANServer(this IServiceCollection services,
            IConfigurationRoot configuration, 
            string clientId,
            ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        {
            services.Configure<STANOptions>(options =>
            {
                options.ClientId = $"{_clientIdReplacer.Replace(clientId, "_")}_{Guid.NewGuid():N}";
                configuration.GetSection("STANOptions").Bind(options);
            });
            services.PostConfigure<STANOptions>(options =>
            {
                if (options.IsAuthentication && (AppEnvironment.IsProduction || HostEnvironment.IsProduction))
                {
                    options.UserName = options.UserName.DecryptDES();
                    options.Password = options.Password.DecryptDES();
                }
            });
            services.Add(new ServiceDescriptor(typeof(STANClient),
                spr => new STANClient(spr.GetService<ILogger<STANClient>>(), spr.GetService<IOptions<STANOptions>>()),
                serviceLifetime));
        }
    }
}
