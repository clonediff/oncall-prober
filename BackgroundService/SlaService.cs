using Microsoft.Extensions.Options;
using Prober.Data.SlaDb;
using Prober.Entities;
using Prober.Options;
using Prober.Services;

namespace Prober.BackgroundService;

public class SlaService : Microsoft.Extensions.Hosting.BackgroundService
{
    private readonly PrometheusSettings _prometheusSettings;
    private readonly ILogger<ProbeService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SlaService(IOptions<PrometheusSettings> prometheusSetting, ILogger<ProbeService> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _prometheusSettings = prometheusSetting.Value;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("Start Sla checker");

        while (!ct.IsCancellationRequested)
        {
            await CheckAll(ct);

            _logger.LogDebug("Waiting {scrapeInterval} seconds for next loop", _prometheusSettings.ScrapeInterval);
            await Task.Delay(_prometheusSettings.ScrapeInterval * 1000, ct);
        }
    }

    private async Task CheckAll(CancellationToken ct)
    {
        var time = DateTimeOffset.UtcNow;

        await CheckAndSave(
            "increase(prober_create_team_scenario_success_total[1m])", time, 0, true,
            "prober_create_team_scenario_success_total", 1, val => val < 1,
            ct
        );

        await CheckAndSave(
            "increase(prober_create_team_scenario_fail_total[1m])", time, 100, true,
            "prober_create_team_scenario_fail_total", 0, val => val > 0,
            ct
        );

        await CheckAndSave(
            "prober_create_team_scenario_duration_seconds", time, 2, false,
            "prober_create_team_scenario_duration_seconds", 0.1, val => val > 0.1,
            ct
        );
    }

    private async Task CheckAndSave(string query, DateTimeOffset time, double defaultValue, bool castToInt,
        string indicatorName, double slo, Predicate<double> isBad,
        CancellationToken ct)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var prometheusClient = scope.ServiceProvider.GetRequiredService<PrometheusClient>();
        var slaDbContext = scope.ServiceProvider.GetRequiredService<SlaDbContext>();
        
        var value = await prometheusClient.LastValueAsync(query, time, defaultValue, castToInt, ct);
        var indicator = new Indicator
        {
            Name = indicatorName,
            Slo = slo,
            Time = time,
            Value = value,
            IsBad = isBad(value)
        };
        slaDbContext.Indicators.Add(indicator);
        await slaDbContext.SaveChangesAsync(ct);
    }
}
