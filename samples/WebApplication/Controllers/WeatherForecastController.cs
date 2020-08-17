using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
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

            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = Summaries[rng.Next(Summaries.Length)]
                })
                .ToArray();
        }
    }
}