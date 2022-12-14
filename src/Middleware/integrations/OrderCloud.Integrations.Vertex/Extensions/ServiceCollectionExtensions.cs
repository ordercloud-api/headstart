using System;
using Headstart.Common.Services;
using Headstart.Common.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace OrderCloud.Integrations.Vertex.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddVertexTaxProvider(this IServiceCollection services, EnvironmentSettings environmentSettings, VertexSettings vertexSettings)
        {
            if (!environmentSettings.TaxProvider.Equals("Vertex", StringComparison.OrdinalIgnoreCase))
            {
                return services;
            }

            if (string.IsNullOrEmpty(vertexSettings.ClientID) ||
                string.IsNullOrEmpty(vertexSettings.ClientSecret) ||
                string.IsNullOrEmpty(vertexSettings.Username) ||
                string.IsNullOrEmpty(vertexSettings.Password))
            {
                throw new Exception("EnvironmentSettings:TaxProvider is set to 'Vertex' however missing required properties VertexSettings:ClientID, VertexSettings:ClientSecret, VertexSettings:Username, or VertexSettings:Password. Please define these properties or set EnvironmentSettings:TaxProvider to an empty string to use mocked tax rates");
            }

            services
                .AddSingleton(x => vertexSettings)
                .AddSingleton<VertexClient>()
                .AddSingleton<ITaxCalculator, VertexCommand>();

            return services;
        }
    }
}
