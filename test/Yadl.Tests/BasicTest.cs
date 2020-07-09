using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IServiceProvider _serviceProvider;
        private ILogger<BasicTest> _logger;

        public BasicTest()
        {
            var webHost = new HostBuilder()
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

            var host = webHost.Build();
            _serviceProvider = host.Services;
            _logger = _serviceProvider.GetService<ILogger<BasicTest>>();

            host.Start();
        }

        [Fact]
        public void VerifyInsertElementsAfterBatchSize()
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                {"adddt", DateTimeOffset.Now}
            });

            _logger.LogInformation("Test 1");
            _logger.LogInformation("Test 2");
            _logger.LogInformation("Test 3");
            _logger.LogInformation("Test 4");

            var messages = _serviceProvider.GetService<IYadlProcessor>().Messages;
            Assert.NotNull(messages);
            Assert.NotEmpty(messages);
            Assert.NotEqual("Test 4", messages.FirstOrDefault()?.Message);
        }
    }
}