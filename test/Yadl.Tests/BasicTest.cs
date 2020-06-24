using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Yadl.Abstractions;

namespace Yadl.Tests
{
    public class BasicTest
    {
        private readonly ILogger<BasicTest> _logger;
        private readonly ServiceProvider _serviceProvider;

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
                    options.LogCnnStr = "Ok";
                    options.GlobalFields = new Dictionary<string, object>
                    {
                        {"ServerName", "PROD-APP-01"}
                    };
                });
            });

            _serviceProvider = serviceCollection.BuildServiceProvider();
            _logger = _serviceProvider.GetService<ILogger<BasicTest>>();
        }

        [Fact]
        public async Task TestInformation()
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                {"Asset_Id", 2},
                {"Testing_Id2", 100}
            });

            _logger.LogInformation("Esto es un mensaje con nivel information");
            var channel = _serviceProvider.GetService<IYadlProcessor>().ChannelReader;
            var msg = await channel.ReadAsync();

            Assert.Equal("Esto es un mensaje con nivel information", msg.Descripcion);
        }
    }
}