using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace OrderCloud.Integrations.CosmosDB
{
    /// <summary>
    /// Extension methods for IApplicationBuilder.
    /// </summary>
    public static class IApplicationBuilderExtensions
    {
        /// <summary>
        /// Ensure Cosmos DB is created.
        /// </summary>
        /// <param name="builder">The <see cref="IApplicationBuilder"/>.</param>
        public static void EnsureCosmosDbIsCreated(this IApplicationBuilder builder)
        {
            using (IServiceScope serviceScope = builder.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                ICosmosDbContainerFactory factory = serviceScope.ServiceProvider.GetService<ICosmosDbContainerFactory>();
                if (factory != null)
                {
                    factory.EnsureDbSetupAsync().Wait();
                }
            }
        }
    }
}
