using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;
using Yadl;
using Yadl.Abstractions;
using Yadl.Channels;

namespace Microsoft.Extensions.Logging
{
    public static class LoggingBuilder
    {
        public static ILoggingBuilder AddYadl(this ILoggingBuilder builder)
        {
            builder.AddConfiguration();
            builder.Services.AddSingleton<IYadlProcessor, YadlProcessor>();
            builder.Services.AddSingleton<ILoggerProvider, YadlLoggerProvider>();
            builder.Services.TryAddSingleton<IConfigureOptions<YadlLoggerOptions>, YadlLoggerOptionsSetup>();
            
            return builder;
        }

        public static ILoggingBuilder AddYadl(this ILoggingBuilder builder, Action<YadlLoggerOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            builder.AddYadl();
            builder.Services.Configure(options);

            return builder;
        }
    }
}