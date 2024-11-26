namespace Prober.Entities;

public class Indicator
{
    public Guid Id { get; set; }
    public DateTimeOffset Time { get; set; }
    public string Name { get; set; } = default!;
    public double Slo { get; set; }
    public double Value { get; set; }
    public bool IsBad { get; set; }
}
