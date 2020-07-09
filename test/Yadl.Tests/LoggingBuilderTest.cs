using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Yadl.Tests
{
    public class LoggingBuilderTest
    {
        private MemoryConfigurationSource _memoryConfigurationSource = new MemoryConfigurationSource
        {
            InitialData = new Dictionary<string, string>
            {
                ["Logging:Yadl:IncludeScopes"] = "true",
                ["Logging:Yadl:GlobalFields:ServerName"] = "PROD-APP-01",
                ["Logging:Yadl:BatchSize"] = "100",
                ["Logging:Yadl:ConnectionString"] = "SQL_TEST_LOG",
                ["Logging:Yadl:TableDestination"] = "Logs",
                ["Logging:Yadl:BatchPeriod"] = "30000",
                ["Logging:Yadl:ChannelFullMode"] = "0"
            }
        };

        [Fact]
        public void SetYadlOptions_AppSettings_OK()
        {
            var configuration = new ConfigurationBuilder().Add(_memoryConfigurationSource).Build();
            var serviceCollection = new ServiceCollection()
                .AddLogging(builder =>
                {
                    builder
                        .AddConfiguration(configuration.GetSection("Logging"))
                        .AddYadl();
                });

            using var serviceProvider = serviceCollection.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<YadlLoggerOptions>>();

            Assert.True(options.Value.IncludeScopes);
            Assert.Equal("PROD-APP-01", options.Value.GlobalFields.FirstOrDefault().Value);
            Assert.Equal(100, options.Value.BatchSize);
            Assert.Equal("SQL_TEST_LOG", options.Value.ConnectionString);
            Assert.Equal("Logs", options.Value.TableDestination);
            Assert.Equal(30000, options.Value.BatchPeriod);
            Assert.Equal(BoundedChannelFullMode.Wait, options.Value.ChannelFullMode);
        }
        
        [Fact]
        public void SetYadlOptions_OverwriteAppSettings_OK()
        {
            var configuration = new ConfigurationBuilder().Add(_memoryConfigurationSource).Build();
            const int batchPeriod = 10_000;
            const int batchSize = 5_000;
            
            var serviceCollection = new ServiceCollection()
                .AddLogging(builder =>
                {
                    builder
                        .AddConfiguration(configuration.GetSection("Logging"))
                        .AddYadl(yadlLoggerOptions =>
                        {
                            yadlLoggerOptions.BatchPeriod = batchPeriod;
                            yadlLoggerOptions.BatchSize = batchSize;
                        });
                });

            using var serviceProvider = serviceCollection.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<YadlLoggerOptions>>();

            Assert.Equal(batchSize, options.Value.BatchSize);
            Assert.Equal(batchPeriod, options.Value.BatchPeriod);
            
            Assert.True(options.Value.IncludeScopes);
            Assert.Equal("PROD-APP-01", options.Value.GlobalFields.FirstOrDefault().Value);
            Assert.Equal("SQL_TEST_LOG", options.Value.ConnectionString);
            Assert.Equal("Logs", options.Value.TableDestination);
            Assert.Equal(BoundedChannelFullMode.Wait, options.Value.ChannelFullMode);
        }
    }
}