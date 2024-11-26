using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace Prober.Services;

public class PrometheusClient
{
    private readonly HttpClient _httpClient;

    public PrometheusClient(HttpClient client)
    {
        _httpClient = client;
    }

    public async Task<double> LastValueAsync(string query, DateTimeOffset time, double defaultValue, bool castToInt, CancellationToken ct)
    {
        var queryDict = new Dictionary<string, string?>
        {
            ["query"] = query,
            ["time"] = time.ToUnixTimeSeconds().ToString()
        };
        var response = await _httpClient.GetAsync(QueryHelpers.AddQueryString("/api/v1/query", queryDict), ct);

        if (!response.IsSuccessStatusCode) return defaultValue;
        
        var content = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        var resultsArray = content
            .GetProperty("data")
            .GetProperty("result");
        if (resultsArray.GetArrayLength() == 0) return defaultValue;

        var resStr = resultsArray
            .EnumerateArray().ElementAt(0)
            .GetProperty("value")
            .EnumerateArray().ElementAt(1)
            .GetString();

        var res = double.TryParse(resStr, CultureInfo.InvariantCulture, out var val) ? val : defaultValue;
        return castToInt ? (int) res : res;
    }
}
