using System;
using System.Diagnostics;
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
                BatchPeriod = 30_000,
                BatchSize = 1_000,
                ChannelFullMode = 0,
                ConnectionString = config["TestCnnString"],
                TableDestination = "Logs"
            };

            var yadlProcessor = new YadlProcessor(options);
            var hostedService = new CoreLoggerHostedService(yadlProcessor, options);

            await hostedService.StartAsync(CancellationToken.None);

            for (int i = 1; i <= 100_000; i++)
            {
                _ = yadlProcessor.ChannelWriter.TryWrite(new YadlMessage
                {
                    Message = $"MSG: {i}",
                    Level = 1,
                    LevelDescription = "Debug",
                    TimeStamp = DateTimeOffset.Now
                });
            }
            
            await Task.Delay(7500);
            Assert.False(yadlProcessor.Messages.Any());
        }
    }
}