using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

namespace Yadl
{
    public class YadlLoggerOptionsSetup : ConfigureFromConfigurationOptions<YadlLoggerOptions>
    {
        public YadlLoggerOptionsSetup(ILoggerProviderConfiguration<YadlLoggerProvider> providerConfiguration) : base(providerConfiguration.Configuration)
        {
        }
    }
}