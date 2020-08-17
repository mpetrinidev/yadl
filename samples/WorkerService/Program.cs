using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WorkerService
{
    public class Program
    {
        public static string CnnString { get; } =
            "Server=localhost,1433;Database=TEST_LOGS;User ID=SA;Password=Password1!";

        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                    services.AddLogging(builder =>
                    {
                        builder.AddYadl(options =>
                        {
                            options.BatchPeriod = 30_000;
                            options.BatchSize = 5;
                            options.ConnectionString = CnnString;
                            options.TableDestination = "Logs";
                            options.GlobalFields = new Dictionary<string, object> {{"App", "WorkerService"}};
                        });
                    });
                });
    }
}