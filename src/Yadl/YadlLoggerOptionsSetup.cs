using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Yadl
{
    public class YadlLoggerOptionsSetup : ConfigureFromConfigurationOptions<YadlLoggerOptions>
    {
        public YadlLoggerOptionsSetup(IConfiguration config) : base(config)
        {
        }
    }
}