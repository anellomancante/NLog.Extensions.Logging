#if !NETCORE1_0


using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace NLog.Extensions.Logging
{
    internal static class SharedConfigureExtensions
    {

        internal static void AddNLogLoggerProvider(this ILoggingBuilder builder, IConfiguration configuration, NLogProviderOptions options, Func<IServiceProvider, IConfiguration, NLogProviderOptions, NLogLoggerProvider> factory, IServiceCollection services)
        {
            var sharedFactory = factory;

            if ((options ?? NLogProviderOptions.Default).ReplaceLoggerFactory)
            {
                NLogLoggerProvider singleInstance = null; // Ensure that registration of ILoggerFactory and ILoggerProvider shares the same single instance
                sharedFactory = (provider, cfg, opt) => singleInstance ?? (singleInstance = factory(provider, cfg, opt));

                builder.ClearProviders(); // Cleanup the existing LoggerFactory, before replacing it with NLogLoggerFactory
                services.Replace(ServiceDescriptor.Singleton<ILoggerFactory, NLogLoggerFactory>(serviceProvider => new NLogLoggerFactory(sharedFactory(serviceProvider, configuration, options))));
            }

            services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, NLogLoggerProvider>(serviceProvider => sharedFactory(serviceProvider, configuration, options)));

            if ((options ?? NLogProviderOptions.Default).RemoveLoggerFactoryFilter)
            {
                // Will forward all messages to NLog if not specifically overridden by user
                builder.AddFilter<NLogLoggerProvider>(null, Microsoft.Extensions.Logging.LogLevel.Trace);
            }
        }
    }
}
#endif