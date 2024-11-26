using Microsoft.EntityFrameworkCore;
using Prober;
using Prober.BackgroundService;
using Prober.Data.AppDb;
using Prober.Data.SlaDb;
using Prober.Options;
using Prober.Services;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

var mySqlSettings = new MySqlSettings();
builder.Configuration.GetSection(MySqlSettings.SectionName).Bind(mySqlSettings);

var appDbConnectionString =
    mySqlSettings.ConnectionString(builder.Configuration.GetValue<string>(ConfigConstants.AppDbDatabase)!);
Console.WriteLine($"AppDbContext Connection String: {appDbConnectionString}");
builder.Services.AddDbContext<AppDbContext>(options => options.UseMySQL(appDbConnectionString));

var slaDbConnectionString =
    mySqlSettings.ConnectionString(builder.Configuration.GetValue<string>(ConfigConstants.SlaDbDatabase)!);
Console.WriteLine($"SlaDbContext Connection String: {slaDbConnectionString}");
builder.Services.AddDbContext<SlaDbContext>(options => options.UseMySQL(slaDbConnectionString));

var exporterSettings = new ExporterSettings();
var exporterSettingsSection = builder.Configuration.GetSection(ExporterSettings.SectionName);
exporterSettingsSection.Bind(exporterSettings);
builder.Services.Configure<ExporterSettings>(exporterSettingsSection);

var prometheusSettings = new PrometheusSettings();
var prometheusSettingsSection = builder.Configuration.GetSection(PrometheusSettings.SectionName);
prometheusSettingsSection.Bind(prometheusSettings);
builder.Services.Configure<PrometheusSettings>(prometheusSettingsSection);

builder.WebHost.UseUrls($"http://[::]:{exporterSettings.MetricsPort}");

builder.Services
    .AddHttpClient(ConfigConstants.OnCallHttpClientName, client =>
    {
        client.BaseAddress = new Uri(exporterSettings.ApiUrl);
    });

builder.Services
    .AddHttpClient<PrometheusClient>(client =>
    {
        client.BaseAddress = new Uri(prometheusSettings.ApiUrl);
    });

builder.Services.AddHostedService<ProbeService>();
builder.Services.AddHostedService<SlaService>();

var app = builder.Build();

await app.InitApplicationNames(exporterSettings.AppSettings);
await app.MigrateSlaDbContext();

app.MapMetrics();

app.Run();
