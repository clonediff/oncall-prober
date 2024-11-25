namespace Prober.Options;

public class ExporterSettings
{
    public const string SectionName = nameof(ExporterSettings);

    public string ApiUrl { get; set; } = default!;
    public int ScrapeInterval { get; set; }
    public int MetricsPort { get; set; }

    public AppSettings AppSettings { get; set; } = default!;
}
