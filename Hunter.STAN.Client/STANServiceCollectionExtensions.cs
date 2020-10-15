﻿
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

        public static void AddSTANServer(this IServiceCollection services, Action<STANOptions> steup)
        {
            services.Configure(steup);
            services.AddTransient<STANClient>();
        }

        public static void AddSTANServer(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.Configure<STANOptions>(options =>
            {
                options.ClientId = Guid.NewGuid().ToString("N");
                configuration.GetSection("STANOptions").Bind(options);
            });
            services.AddTransient<STANClient>();
        }

        public static void AddSTANServer(this IServiceCollection services, IConfigurationRoot configuration, string clientId)
        {
            services.Configure<STANOptions>(options =>
            {
                options.ClientId = $"{_clientIdReplacer.Replace(clientId, "_")}_{Guid.NewGuid():N}";
                configuration.GetSection("STANOptions").Bind(options);
            });
            services.AddTransient<STANClient>();
        }
    }
}
