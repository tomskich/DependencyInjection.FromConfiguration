using System;
using System.Collections.Generic;
using DependencyInjectionFromConfig.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Creates service types <see cref="ServiceItem" /> according to the configuration settings and adds them to the service collection <paramref name="services"/>.
        /// Each service can have its own options <see cref="ServiceItem.Options"/>,
        /// which are added to the service collection as <see cref="IOptions{TOptions}"/> using a method call <see cref="Configure"/>.
        /// Also adds options validation <see cref="ValidateDataAnnotations"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="configuration">Configuration section containing an array of service <see cref="ServiceItem" /> settings.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddFromConfiguration(this IServiceCollection services, IConfigurationSection configuration)
        {
            if (configuration.Exists())
            {
                foreach (ServiceItem service in configuration.Get<List<ServiceItem>>())
                {
                    (Type baseType, Type implementationType, Type optionsType) = GetServiceTypes(service);

                    var serviceDescriptor = new ServiceDescriptor(baseType ?? implementationType, implementationType, service.Lifetime);
                    services.Add(serviceDescriptor);

                    if (optionsType != null && service.Options?.Value != null)
                    {
                        services.Configure(optionsType, service.Options.Value)
                            .ValidateDataAnnotations(optionsType);
                    }
                }
            }
            return services;
        }

        /// <summary>
        /// Registers a configuration instance which <paramref name="optionsType"/> will bind against.
        /// See https://github.com/aspnet/Extensions/blob/7a6e096832b02d98649ed194e9368a76b757eacb/src/Options/ConfigurationExtensions/src/OptionsConfigurationServiceCollectionExtensions.cs#L57
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="optionsType">The type of service options.</param>
        /// <param name="config">The configuration being bound.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection Configure(this IServiceCollection services, Type optionsType, IConfigurationSection config)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (optionsType == null)
                throw new ArgumentNullException(nameof(optionsType));
            if (config == null || !config.Exists())
                throw new ArgumentNullException(nameof(config));
            if (!optionsType.IsClass)
                throw new ArgumentException("The options type must be a class.", nameof(optionsType));

            services.AddOptions();

            var optionsChangeTokenSourceType = typeof(IOptionsChangeTokenSource<>).MakeGenericType(optionsType);
            var configurationChangeTokenSourceInstance = Activator.CreateInstance(
                typeof(ConfigurationChangeTokenSource<>).MakeGenericType(optionsType),
                new object[] { Options.Options.DefaultName, config });
            services.AddSingleton(optionsChangeTokenSourceType, configurationChangeTokenSourceInstance);

            var configureOptionsType = typeof(IConfigureOptions<>).MakeGenericType(optionsType);
            var namedConfigureFromConfigurationOptionsTypeOfOptionsInstance = Activator.CreateInstance(
                typeof(NamedConfigureFromConfigurationOptions<>).MakeGenericType(optionsType),
                new object[] { Options.Options.DefaultName, config, new Action<BinderOptions>(_ => { }) });
            return services.AddSingleton(configureOptionsType, namedConfigureFromConfigurationOptionsTypeOfOptionsInstance);
        }

        private static (Type baseType, Type implementationType, Type optionsType) GetServiceTypes(ServiceItem service)
        {
            if (string.IsNullOrWhiteSpace(service.Implementation))
                throw new Exception($"The {nameof(ServiceItem.Implementation)} type is not specified for one of the services in Configuration.Services");

            Type baseType = null;
            if (!string.IsNullOrWhiteSpace(service.Service))
            {
                baseType = Type.GetType(service.Service);
                if (baseType == null)
                {
                    throw new Exception($"Failed to create the base service type from configuration " +
                        $"[{service.Service}] in Configuration.Services[].{nameof(ServiceItem.Service)}");
                }
            }

            Type implementationType = Type.GetType(service.Implementation);
            if (implementationType == null)
            {
                throw new Exception($"Failed to create the service implementation type from configuration " +
                    $"[{service.Implementation}] in Configuration.Services[].{nameof(ServiceItem.Implementation)}");
            }

            Type optionsType = null;
            if (!string.IsNullOrWhiteSpace(service.Options?.Implementation))
            {
                optionsType = Type.GetType(service.Options.Implementation);
                if (optionsType == null)
                {
                    throw new Exception($"Failed to create the type of service settings from configuration " +
                        $"[{service.Options.Implementation}] in Configuration.Services[].{nameof(ServiceItem.Options)}.{nameof(ServiceItem.Options.Implementation)}");
                }
            }

            return (baseType, implementationType, optionsType);
        }

        /// <summary>
        /// Adds annotations validation to the options <paramref name="optionsType"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="optionsType">The options type.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        private static IServiceCollection ValidateDataAnnotations(this IServiceCollection services, Type optionsType)
        {
            var validateOptionsType = typeof(IValidateOptions<>).MakeGenericType(optionsType);
            var dataAnnotationValidateOptionsType = typeof(DataAnnotationValidateOptions<>).MakeGenericType(optionsType);
            var dataAnnotationValidateOptions = Activator.CreateInstance(dataAnnotationValidateOptionsType, Options.Options.DefaultName);
            return services.AddSingleton(validateOptionsType, dataAnnotationValidateOptions);
        }
    }
}