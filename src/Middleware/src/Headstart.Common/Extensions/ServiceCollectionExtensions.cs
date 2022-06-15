using System;
using System.Linq;
using System.Reflection;
using Headstart.Common.Commands;
using Headstart.Common.Services;
using Headstart.Common.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

        public static IServiceCollection InjectOrderCloud<T>(this IServiceCollection services, OrderCloudSettings orderCloudSettings)
        {
            services.AddSingleton<IOrderCloudClient>(provider => new OrderCloudClient(new OrderCloudClientConfig
            {
                ApiUrl = orderCloudSettings.ApiUrl,
                AuthUrl = orderCloudSettings.ApiUrl,
                ClientId = orderCloudSettings.MiddlewareClientID,
                ClientSecret = orderCloudSettings.MiddlewareClientSecret,
                Roles = new[] { ApiRole.FullAccess },
            }));

            return services;
        }

        public static IServiceCollection InjectOrderCloud<T>(this IServiceCollection services, OrderCloudClientConfig config)
        {
            services.AddSingleton<IOrderCloudClient>(provider => new OrderCloudClient(config));
            return services;
        }

        public static IServiceCollection AddMockShippingProvider(this IServiceCollection services)
        {
            services.TryAddSingleton<IShippingCommand, MockShippingCommand>();
            services.TryAddSingleton<IShippingService, DefaultShippingService>();

            return services;
        }

        public static IServiceCollection AddMockTaxProvider(this IServiceCollection services)
        {
            services.TryAddSingleton<ITaxCodesProvider, MockTaxService>();
            services.TryAddSingleton<ITaxCalculator, MockTaxService>();

            return services;
        }

        public static IServiceCollection AddMockCreditCardProcessor(this IServiceCollection services)
        {
            services.TryAddSingleton<ICreditCardProcessor, MockCreditCardProcessor>();

            return services;
        }

        public static IServiceCollection AddDefaultAddressProvider(this IServiceCollection services)
        {
            services.TryAddSingleton<IAddressCommand, AddressCommand>();
            services.TryAddSingleton<IAddressValidationCommand, AddressValidationCommand>();

            return services;
        }
    }
}
