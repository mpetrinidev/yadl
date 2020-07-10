using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using Yadl.Abstractions;
using Yadl.Channels;
using Yadl.HostedServices;

namespace Yadl.Tests
{
    public class HostedServiceTest
    {
        [Fact]
        public async Task CoreLoggerHostedService_ShouldBatchInsert()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddUserSecrets<HostedServiceTest>();
            var config = configurationBuilder.Build();

            var options = new YadlLoggerOptions
            {
                BatchPeriod = 30000,
                BatchSize = 100,
                ChannelFullMode = 0,
                ConnectionString = config["TestCnnString"],
                TableDestination = "Logs"
            };

            var yadlProcessor = new YadlProcessor(options);
            var hostedService = new CoreLoggerHostedService(yadlProcessor, options);

            await hostedService.StartAsync(default);

            for (int i = 1; i <= 1_000; i++)
            {
                yadlProcessor.ChannelWriter.TryWrite(new YadlMessage
                {
                    Message = $"MSG: {i}",
                    Level = 1,
                    LevelDescription = "Debug",
                    TimeStamp = DateTimeOffset.Now
                });

                if (i % 100 == 0)
                {
                    await Task.Delay(250);
                }
            }
            
            hostedService.Dispose();
            Assert.Empty(yadlProcessor.Messages);
        }
    }
}