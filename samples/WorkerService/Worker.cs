using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (_logger.BeginScope(new Dictionary<string, object> {{"VAR1", 1}, {"VAR2", null}}))
                {
                    _logger.LogInformation("Log with variables: VAR1 and VAR2");

                    using (_logger.BeginScope(("VAR3", 3)))
                    {
                        _logger.LogInformation("Add variable called VAR3");
                        
                        using (_logger.BeginScope(("VAR2", 2)))
                        {
                            _logger.LogInformation("Override VAR2 with value: 2");
                        }
                    }
                }

                _logger.LogWarning("Log without scope variables");

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}