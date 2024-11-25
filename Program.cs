using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Prober;
using Prober.BackgroundService;
using Prober.Data;
using Prober.Options;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

var mySqlSettings = new MySqlSettings();
builder.Configuration.GetSection(MySqlSettings.SectionName).Bind(mySqlSettings);
Console.WriteLine($"MySQL Connection String: {mySqlSettings.ConnectionString}");
builder.Services.AddDbContext<AppDbContext>(options => options.UseMySQL(mySqlSettings.ConnectionString));
builder.Services.AddScoped<DbContext, AppDbContext>();

var exporterSettings = new ExporterSettings();
var exporterSettingsSection = builder.Configuration.GetSection(ExporterSettings.SectionName);
exporterSettingsSection.Bind(exporterSettings);
builder.Services.Configure<ExporterSettings>(exporterSettingsSection);

builder.WebHost.UseUrls($"http://[::]:{exporterSettings.MetricsPort}");

builder.Services
    .AddHttpClient(ConfigConstants.OnCallHttpClientName, client =>
    {
        client.BaseAddress = new Uri(exporterSettings.ApiUrl);
    });

builder.Services.AddHostedService<ProbeService>();

var app = builder.Build();

await app.InitApplicationNames(exporterSettings.AppSettings);

app.MapMetrics();

app.Run();