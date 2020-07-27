using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Yadl.Tests
{
    public class BasicTest : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly IHost _host;

        public BasicTest()
        {
            var hostBuilder = new HostBuilder()
                .ConfigureServices(c =>
                {
                    c.AddRouting();
                    c.AddLogging(builder =>
                    {
                        builder.AddYadl(options =>
                        {
                            options.IncludeScopes = true;
                            options.BatchPeriod = 30000;
                            options.BatchSize = 100;
                            options.TableDestination = "Logs";
                            options.ConnectionString = Variables.CnnString;
                                options.GlobalFields = new Dictionary<string, object>
                            {
                                {"ServerName", "PROD-APP-01"},
                                {"Ip", "192.168.0.1"}
                            };
                        });
                    });
                })
                .ConfigureWebHost(builder =>
                {
                    builder.UseTestServer();
                    builder.Configure(app =>
                    {
                        app.UseRouting();
                        app.UseEndpoints(e =>
                        {
                            e.MapGet("/test", async context =>
                            {
                                var loggerFactory = context.RequestServices.GetService<ILoggerFactory>();
                                var logger = loggerFactory.CreateLogger<BasicTest>();
                                
                                using (logger.BeginScope(new Dictionary<string, object> {{"Test", "Test"}}))
                                {
                                    for (var i = 1; i <= 1_000; i++)
                                    {
                                        logger.LogCritical($"Msg_{i}");
                                    }
                                }

                                await Task.Delay(1500);

                                context.Response.StatusCode = StatusCodes.Status200OK;
                                await context.Response.WriteAsync("");
                            });
                        });
                    });
                });

            _host = hostBuilder.Start();
            _httpClient = _host.GetTestClient();
        }

        [Fact]
        public async Task VerifyInsertElementsAfterBatchSize()
        {
            var response = await _httpClient.GetAsync("/test");
            Assert.Equal((HttpStatusCode) 200, response.StatusCode);
        }

        public void Dispose()
        {
            _host?.Dispose();
            _httpClient?.Dispose();
        }
    }
}