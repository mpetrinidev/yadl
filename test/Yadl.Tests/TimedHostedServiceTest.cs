using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using Yadl.Abstractions;
using Yadl.Channels;
using Yadl.HostedServices;

namespace Yadl.Tests
{
    public class TimedHostedServiceTest
    {
        [Fact]
        public async Task ExecuteAsync_InsertAllMessagesAfter50ms_Ok()
        {
            var sqlServerMock = new Mock<ISqlServerBulk>();
            sqlServerMock.Setup(
                    v => v.ExecuteAsync(It.IsAny<ICollection<YadlMessage>>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.CompletedTask);

            var options = new YadlLoggerOptions
            {
                BatchSize = 100,
                BatchPeriod = 50
            };

            var yadlProcessor = new YadlProcessor(options);
            var hostedService = new TimedHostedService(yadlProcessor, options, sqlServerMock.Object);

            await hostedService.StartAsync(CancellationToken.None);

            for (int i = 1; i <= 15; i++)
            {
                _ = yadlProcessor!.ChannelWriter.TryWrite(new YadlMessage
                {
                    Message = $"MSG: {i}",
                    Level = 1,
                    LevelDescription = "Debug",
                    TimeStamp = DateTimeOffset.Now
                });
            }

            await hostedService.StopAsync(CancellationToken.None);

            yadlProcessor.Messages.Should().BeEmpty();
        }
    }
}