using System;
using Headstart.Common.Services;
using Headstart.Common.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace OrderCloud.Integrations.Vertex.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddVertexTaxProvider(this IServiceCollection services, EnvironmentSettings environmentSettings, VertexConfig vertexSettings)
        {
            if (!environmentSettings.TaxProvider.Equals("Vertex", StringComparison.OrdinalIgnoreCase))
            {
                return services;
            }

            services
                .AddSingleton(x => vertexSettings)
                .AddSingleton<VertexClient>()
                .AddSingleton<ITaxCalculator, VertexCommand>();

            return services;
        }
    }
}
