using System.Text;
using System.Text.Json;

namespace FusionCli;

public sealed class FusionClient : IDisposable
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;

    public FusionClient(string url = "http://127.0.0.1:7432")
    {
        _baseUrl = url.TrimEnd('/');
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
    }

    public async Task PingAndPrintAsync()
    {
        try
        {
            var resp = await _http.GetAsync($"{_baseUrl}/ping");
            resp.EnsureSuccessStatusCode();
            var raw = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(raw);
            Console.WriteLine($"Connected: {doc.RootElement.GetProperty("message").GetString()}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: Cannot reach Fusion 360. {ex.Message}");
            Environment.ExitCode = 1;
        }
    }

    public async Task CallAndPrintAsync(string command, Dictionary<string, object?>? parameters = null)
    {
        try
        {
            var json = BuildRequestJson(command, parameters);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await _http.PostAsync($"{_baseUrl}/command", content);
            var raw = await resp.Content.ReadAsStringAsync();
            Console.WriteLine(Prettify(raw));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Environment.ExitCode = 1;
        }
    }

    private static string BuildRequestJson(string command, Dictionary<string, object?>? parameters)
    {
        var sb = new StringBuilder();
        sb.Append("{\"command\":\"");
        sb.Append(Escape(command));
        sb.Append("\",\"params\":{");
        if (parameters != null)
        {
            var first = true;
            foreach (var (key, value) in parameters)
            {
                if (!first) sb.Append(',');
                first = false;
                sb.Append('"');
                sb.Append(Escape(key));
                sb.Append("\":");
                sb.Append(ValueToJson(value));
            }
        }
        sb.Append("}}");
        return sb.ToString();
    }

    private static string ValueToJson(object? value) => value switch
    {
        null => "null",
        string s => "\"" + Escape(s) + "\"",
        bool b => b ? "true" : "false",
        int i => i.ToString(),
        double d => d.ToString(System.Globalization.CultureInfo.InvariantCulture),
        long l => l.ToString(),
        System.Collections.IEnumerable list => EnumerableToJson(list),
        _ => "\"" + Escape(value.ToString() ?? "") + "\""
    };

    private static string EnumerableToJson(System.Collections.IEnumerable list)
    {
        var sb = new StringBuilder("[");
        var first = true;
        foreach (var item in list)
        {
            if (!first) sb.Append(',');
            first = false;
            sb.Append(ValueToJson(item));
        }
        sb.Append(']');
        return sb.ToString();
    }

    private static string Escape(string s)
    {
        if (!s.Contains('\\') && !s.Contains('"') && !s.Contains('\n') && !s.Contains('\r') && !s.Contains('\t'))
            return s;
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
    }

    private static string Prettify(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            using var ms = new MemoryStream();
            using (var jw = new Utf8JsonWriter(ms, new JsonWriterOptions { Indented = true }))
            {
                doc.WriteTo(jw);
            }
            return Encoding.UTF8.GetString(ms.ToArray());
        }
        catch { return json; }
    }

    public void Dispose() => _http.Dispose();
}
