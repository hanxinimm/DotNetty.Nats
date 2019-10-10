﻿
using Hunter.STAN.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class STANServiceCollectionExtensions
    {
        private static readonly Regex _clientIdReplacer = new Regex("\\W\\D");
        public static void AddSTANServer(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.Configure<STANOptions>(options => configuration.GetSection("STANOptions").Bind(options));
        }

        public static void AddSTANServer(this IServiceCollection services, IConfigurationRoot configuration, string clientId)
        {
            services.Configure<STANOptions>(options =>
            {
                configuration.GetSection("STANOptions").Bind(options);
                options.ClientId = $"{_clientIdReplacer.Replace(clientId, "-")}_{Guid.NewGuid().ToString("N")}";
            });
        }
    }
}