using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Wordle_Tracker_Telegram_Bot.HealthChecks
{
    public class ServerHealthCheck : IHealthCheck
    {
        private Random _random = new Random();
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var responseTime = _random.Next(1, 300);

            if (responseTime < 100)
            {
                return Task.FromResult(HealthCheckResult.Healthy("Healthy result from ServerHealthCheck"));
            }
            else if (responseTime < 200)
            {
                return Task.FromResult(HealthCheckResult.Degraded("Degraded result from ServerHealthCheck"));
            }

            return Task.FromResult(HealthCheckResult.Unhealthy("Unhealthy result from ServerHealthCheck"));
        }
    }
}
