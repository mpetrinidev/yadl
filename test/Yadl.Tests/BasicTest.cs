using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Yadl.Tests
{
    public class BasicTest
    {
        public BasicTest()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder =>
            {
                builder.AddYadl(options =>
                {
                    options.BatchPeriod = 100;
                    options.BatchSize = 10;
                    options.TableDestination = "LOGS";
                });
            });
        }
    }
}