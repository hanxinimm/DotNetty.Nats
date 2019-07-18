
using Hunter.STAN.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class STANServiceCollectionExtensions
    {
        public static void AddSTANServer(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.Configure<STANOptions>(options => configuration.GetSection("STANOptions").Bind(options));
        }

        public static void AddSTANServer(this IServiceCollection services, IConfigurationRoot configuration, string clientId)
        {
            services.Configure<STANOptions>(options =>
            {
                configuration.GetSection("STANOptions").Bind(options);
                options.ClientId = clientId;
            });
        }
    }
}
