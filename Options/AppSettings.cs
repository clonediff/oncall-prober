namespace Prober.Options;

public class AppSettings
{
    public const string SectionName = nameof(AppSettings);
    
    public string Name { get; set; } = default!;
    public string Key { get; set; } = default!;
}
