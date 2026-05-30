using System.Text.Json;
using System.Text.Json.Serialization;

namespace FusionCli;

[JsonSerializable(typeof(Dictionary<string, object?>))]
[JsonSerializable(typeof(List<object>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public sealed partial class FusionJsonContext : JsonSerializerContext;
