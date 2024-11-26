namespace Prober.Options;

public class PrometheusSettings
{
    public const string SectionName = nameof(PrometheusSettings);

    public string ApiUrl { get; set; } = default!;
    public int ScrapeInterval { get; set; }
}
