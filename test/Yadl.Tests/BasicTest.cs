using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;
using Yadl.Abstractions;

namespace Yadl.Tests
{
    public class BasicTest
    {
        private IServiceProvider _serviceProvider;
        private ILogger<BasicTest> _logger;
        private IHostBuilder _hostBuilder;

        public BasicTest()
        {
            _hostBuilder = new HostBuilder()
                .ConfigureWebHost(builder =>
                {
                    builder.UseTestServer();
                    builder.UseEnvironment("Test");
                    builder.Configure(app => app.Run(async ctx => await ctx.Response.WriteAsync("Hello World!")));
                })
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddUserSecrets<BasicTest>();
                })
                .ConfigureServices(c =>
                {
                    c.AddLogging(builder =>
                    {
                        builder.AddYadl(options =>
                        {
                            options.BatchPeriod = 30000;
                            options.BatchSize = 3;
                            options.TableDestination = "Logs";
                            options.ConnectionString = builder.Services.BuildServiceProvider().GetService<IConfiguration>()["TestCnnString"];
                            options.GlobalFields = new Dictionary<string, object>
                            {
                                {"ServerName", "PROD-APP-01"},
                                {"ep_origen", "192.168.0.1"}
                            };
                        });
                    });
                });

            // IHost host = webHost.Build();
            // _serviceProvider = host.Services;
            // _logger = _serviceProvider.GetService<ILogger<BasicTest>>();
            //
            // host.Start();
        }

        [Fact]
        public async Task VerifyInsertElementsAfterBatchSize()
        {
            var host = await _hostBuilder.StartAsync();
            
            _serviceProvider = host.Services;
            _logger = _serviceProvider.GetService<ILogger<BasicTest>>();

            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                {"adddt", DateTimeOffset.Now}
            });

            _logger.LogInformation("Test 1");
            _logger.LogInformation("Test 2");
            _logger.LogInformation("Test 3");
            _logger.LogInformation("Test 4");

            var messages = _serviceProvider.GetService<IYadlProcessor>().Messages;

            await Task.Delay(5000);
            
            Assert.NotNull(messages);
            Assert.NotEmpty(messages);
            Assert.Equal("Test 4", messages.FirstOrDefault()?.Message);
        }
    }
}