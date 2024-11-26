using System.Diagnostics;
using System.Net;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Prober.Options;
using Prometheus;

namespace Prober.BackgroundService;

public class ProbeService : Microsoft.Extensions.Hosting.BackgroundService
{
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly ExporterSettings _exporterSettings;
    private readonly ILogger<ProbeService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public ProbeService(IOptions<ExporterSettings> exporterSettings, ILogger<ProbeService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _exporterSettings = exporterSettings.Value;
        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
    }

    private static readonly Counter ProberCreateTeamScenarioTotal =
        Metrics.CreateCounter("prober_create_team_scenario_total",
            "Total count of runs the create team scenario to oncall API");

    private static readonly Counter ProberCreateTeamScenarioSuccessTotal =
        Metrics.CreateCounter("prober_create_team_scenario_success_total",
            "Total count of success runs the create team scenario to oncall API");

    private static readonly Counter ProberCreateTeamScenarioFailTotal =
        Metrics.CreateCounter("prober_create_team_scenario_fail_total",
            "Total count of fail runs the create team scenario to oncall API");

    private static readonly Gauge ProberCreateTeamScenarioDurationSeconds =
        Metrics.CreateGauge("prober_create_team_scenario_duration_seconds",
            "Duration in seconds of runs the create team scenario to oncall API");
    
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("Run Prober");
        while (!ct.IsCancellationRequested)
        {
            await Probe(ct);
            
            await Task.Delay(_exporterSettings.ScrapeInterval * 1000, ct);
        }
    }

    private const string TeamName = "some_test_team_name";
    private static readonly CreateTeamDto TeamDto = new(TeamName, "US/Pacific", "root");
    private const string TeamsPath = "/api/v0/teams";
    private const string DeleteTeamsPath = $"{TeamsPath}/{TeamName}";
    
    private async Task Probe(CancellationToken ct)
    {
        ProberCreateTeamScenarioTotal.Inc();
        _logger.LogDebug("Try create user");

        var stopwatch = Stopwatch.StartNew();

        HttpResponseMessage? createReponse = null, deleteResponse = null;
        using var client = _httpClientFactory.CreateClient(ConfigConstants.OnCallHttpClientName);
        try
        {
            createReponse = await SendAsync(client, HttpMethod.Post, TeamsPath, TeamDto, ct);
        }   
        catch (Exception err)
        {
            _logger.LogError("{err}", err);
            ProberCreateTeamScenarioFailTotal.Inc();
        }
        finally
        {
            try
            {
                deleteResponse = await SendAsync(client, HttpMethod.Delete, DeleteTeamsPath, null, ct);
            }
            catch (Exception err)
            {
                _logger.LogError("{err}", err);
            }
        }

        if (createReponse?.StatusCode == HttpStatusCode.Created && deleteResponse?.StatusCode == HttpStatusCode.OK)
            ProberCreateTeamScenarioSuccessTotal.Inc();
        else
            ProberCreateTeamScenarioFailTotal.Inc();

        stopwatch.Stop();
        
        ProberCreateTeamScenarioDurationSeconds.Set(stopwatch.Elapsed.TotalSeconds);
    }

    private async Task<HttpResponseMessage> SendAsync(HttpClient client, HttpMethod method, string requestPath, object? body, CancellationToken ct)
    {
        var bodyAsString = JsonSerializer.Serialize(body, _serializerOptions);
        var request = new HttpRequestMessage(method, requestPath)
        {
            Content = new StringContent(bodyAsString, Encoding.UTF8, MediaTypeNames.Application.Json)
        };
        request.Headers.Add(HeaderNames.Authorization, GetAuthHeader(method, requestPath, bodyAsString));
        return await client.SendAsync(request, ct);
    }

    private string GetAuthHeader(HttpMethod method, string path, string bodyAsString)
    {
        var methodStr = method.Method.ToUpper();
        var window = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;
        
        var text = $"{window} {methodStr} {path} {bodyAsString}";
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_exporterSettings.AppSettings.Key));
        var hashed = hmac.ComputeHash(Encoding.UTF8.GetBytes(text));
        var base64 = Convert.ToBase64String(hashed)
            .Replace('+', '-')
            .Replace('/', '_');

        return $"hmac {_exporterSettings.AppSettings.Name}:{base64}";
    }

    record CreateTeamDto(string Name, string SchedulingTimezone, string Admin);
}
