using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace VmLogsFunction
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.AddOptions<AzureTenantOptions>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection("AzureRmAuth").Bind(settings);
                });
            }
    }
}
