using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;
using Yadl.Abstractions;

namespace Yadl.Tests
{
    public class BasicTest
    {
        private readonly ILogger<BasicTest> _logger;
        private readonly IServiceProvider _serviceProvider;

        public BasicTest()
        {
            var hostBuilder = new HostBuilder()
                .ConfigureWebHostDefaults(configure =>
                {
                    configure.ConfigureServices(serviceCollection =>
                    {
                        serviceCollection.AddLogging(builder =>
                        {
                            builder.AddYadl(options =>
                            {
                                options.BatchPeriod = 30000;
                                options.BatchSize = 3;
                                options.TableDestination = "LOGS";
                                options.LogCnnStr = "Ok";
                                options.GlobalFields = new Dictionary<string, object>
                                {
                                    {"ServerName", "PROD-APP-01"},
                                    {"ep_origen", "192.168.0.1"}
                                };
                            });
                        });
                    });
                });

            var host = hostBuilder.Build();
            _serviceProvider = host.Services;
            _logger = _serviceProvider.GetService<ILogger<BasicTest>>();
            
            host.Run();
        }

        [Fact]
        public async Task VerifyInsertElementsAfterBatchSize()
        {
            // var coreLogger = _host.Services.GetService<CoreLoggerHostedService>();
            // await coreLogger.StartAsync(default);

            _logger.LogInformation("Test 1");
            _logger.LogInformation("Test 2");
            _logger.LogInformation("Test 3");
            _logger.LogInformation("Test 4");

            var messages = _serviceProvider.GetService<IYadlProcessor>().Messages;
            Assert.NotNull(messages);
            Assert.NotEmpty(messages);
            Assert.Equal("Test 4", messages.FirstOrDefault()?.Descripcion);

            // await coreLogger.StopAsync(default);
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

            Assert.Equal("192.168.0.1", msg.IpOrigen);
            Assert.Equal("Esto es un mensaje con nivel information", msg.Descripcion);
        }
    }
}