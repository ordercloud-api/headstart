using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Cosmonaut;
using Cosmonaut.Extensions.Microsoft.DependencyInjection;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OrderCloud.SDK;

namespace ordercloud.integrations.library
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Inject(this IServiceCollection services, Type service)
        {
            return services.AddServicesByConvention(service.Assembly, service.Namespace);
        }

        public static IServiceCollection Inject<T>(this IServiceCollection services)
        {
            return services.AddServicesByConvention(typeof(T).Assembly, typeof(T).Namespace);
        }
        public static IServiceCollection AddServicesByConvention(this IServiceCollection services, Assembly asm,
            string @namespace = null)
        {
            var mappings =
                from impl in asm.GetTypes()
                let iface = impl.GetInterface($"I{impl.Name}")
                where iface != null
                where @namespace == null || iface.Namespace == @namespace
                select new { iface, impl };

            foreach (var m in mappings)
                services.AddSingleton(m.iface, m.impl);

            return services;
        }
        public static IServiceCollection InjectOrderCloud<T>(this IServiceCollection services, OrderCloudClientConfig config)
        {
            services.AddSingleton<IOrderCloudClient>(provider => new OrderCloudClient(config));
            return services;
        }

        public static IServiceCollection InjectCosmosStore<TQuery, TModel>(this IServiceCollection services, CosmosConfig config)
            where TQuery : class
            where TModel : class
        {
            var settings = new CosmosStoreSettings(config.DatabaseName, config.EndpointUri, config.PrimaryKey,
                new ConnectionPolicy
                {
                    ConnectionProtocol = Protocol.Tcp,
                    ConnectionMode = ConnectionMode.Direct,
                    RequestTimeout = config.RequestTimeout
                }, defaultCollectionThroughput: 400)
            {
                UniqueKeyPolicy = new UniqueKeyPolicy()
                {
                    UniqueKeys =
                        (Collection<UniqueKey>)typeof(TModel).GetMethod("GetUniqueKeys")?.Invoke(null, null) ??
                        new Collection<UniqueKey>()
                }
            };
            services.AddSingleton(typeof(TQuery), typeof(TQuery));
            return services.AddCosmosStore<TModel>(settings);
        }
    }
}
