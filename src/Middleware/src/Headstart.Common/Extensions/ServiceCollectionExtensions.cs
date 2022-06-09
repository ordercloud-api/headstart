using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using OrderCloud.SDK;

namespace Headstart.Common.Extensions
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

        public static IServiceCollection AddServicesByConvention(this IServiceCollection services, Assembly asm, string @namespace = null)
        {
            var mappings =
                from impl in asm.GetTypes()
                let iface = impl.GetInterface($"I{impl.Name}")
                where iface != null
                where @namespace == null || iface.Namespace == @namespace
                select new { iface, impl };

            foreach (var m in mappings)
            {
                services.AddSingleton(m.iface, m.impl);
            }

            return services;
        }

        public static IServiceCollection InjectOrderCloud<T>(this IServiceCollection services, OrderCloudClientConfig config)
        {
            services.AddSingleton<IOrderCloudClient>(provider => new OrderCloudClient(config));
            return services;
        }
    }
}
