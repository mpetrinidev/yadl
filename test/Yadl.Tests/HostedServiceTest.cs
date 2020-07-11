using System;
using System.Linq;
using System.Threading;
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
                BatchSize = 10000,
                ChannelFullMode = 0,
                ConnectionString = config["TestCnnString"],
                TableDestination = "Logs"
            };

            var yadlProcessor = new YadlProcessor(options);
            var hostedService = new CoreLoggerHostedService(yadlProcessor, options);

            await hostedService.StartAsync(default);

            for (int i = 1; i <= 100_000; i++)
            {
                yadlProcessor.ChannelWriter.TryWrite(new YadlMessage
                {
                    Message = $"MSG: {i}",
                    Level = 1,
                    LevelDescription = "Debug",
                    TimeStamp = DateTimeOffset.Now
                });
            }

            Assert.False(yadlProcessor.Messages.Any());
            
            await Task.Delay(10000);
            hostedService.Dispose();
        }
    }
}