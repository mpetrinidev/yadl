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
    public class CoreLoggerHostedServiceTest
    {
        [Fact]
        public async Task ExecuteAsync_InsertAllMessages_Ok()
        {
            var sqlServerMock = new Mock<ISqlServerBulk>();
            sqlServerMock.Setup(
                    v => v.ExecuteAsync(It.IsAny<ICollection<YadlMessage>>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.CompletedTask);
            
            var options = new YadlLoggerOptions
            {
                BatchSize = 5
            };
            
            var yadlProcessor = new YadlProcessor(options);
            var hostedService = new CoreLoggerHostedService(yadlProcessor, options, sqlServerMock.Object);

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

            await Task.Delay(25);
            await hostedService.StopAsync(CancellationToken.None);

            yadlProcessor.Messages.Should().BeEmpty();
        }
        
        [Fact]
        public async Task ExecuteAsync_CountMessagesLessThanBatchSize_Ok()
        {
            var sqlServerMock = new Mock<ISqlServerBulk>();
            sqlServerMock.Setup(
                    v => v.ExecuteAsync(It.IsAny<ICollection<YadlMessage>>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.CompletedTask);
            
            var options = new YadlLoggerOptions
            {
                BatchSize = 1_000
            };
            
            var yadlProcessor = new YadlProcessor(options);
            var hostedService = new CoreLoggerHostedService(yadlProcessor, options, sqlServerMock.Object);

            await hostedService.StartAsync(CancellationToken.None);

            for (int i = 1; i <= 10; i++)
            {
                _ = yadlProcessor!.ChannelWriter.TryWrite(new YadlMessage
                {
                    Message = $"MSG: {i}",
                    Level = 1,
                    LevelDescription = "Debug",
                    TimeStamp = DateTimeOffset.Now
                });
            }

            await Task.Delay(25);
            await hostedService.StopAsync(CancellationToken.None);

            yadlProcessor.Messages.Should().HaveCount(10);
        }
        
        [Fact]
        public async Task ExecuteAsync_CountMessagesMoreThanBatchSize_Ok()
        {
            var sqlServerMock = new Mock<ISqlServerBulk>();
            sqlServerMock.Setup(
                    v => v.ExecuteAsync(It.IsAny<ICollection<YadlMessage>>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.CompletedTask);
            
            var options = new YadlLoggerOptions
            {
                BatchSize = 100
            };
            
            var yadlProcessor = new YadlProcessor(options);
            var hostedService = new CoreLoggerHostedService(yadlProcessor, options, sqlServerMock.Object);

            await hostedService.StartAsync(CancellationToken.None);

            for (int i = 1; i <= 250; i++)
            {
                _ = yadlProcessor!.ChannelWriter.TryWrite(new YadlMessage
                {
                    Message = $"MSG: {i}",
                    Level = 1,
                    LevelDescription = "Debug",
                    TimeStamp = DateTimeOffset.Now
                });
            }

            await Task.Delay(25);
            await hostedService.StopAsync(CancellationToken.None);

            yadlProcessor.Messages.Should().HaveCount(50);
        }
    }
}