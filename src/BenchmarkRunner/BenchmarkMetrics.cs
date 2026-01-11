using System.Text.Json.Serialization;

namespace BenchmarkRunner;

public record BenchmarkResult
{
    [JsonPropertyName("apiType")]
    public string ApiType { get; init; } = string.Empty;

    [JsonPropertyName("operation")]
    public string Operation { get; init; } = string.Empty;

    [JsonPropertyName("datasetSize")]
    public int DatasetSize { get; init; }

    [JsonPropertyName("startupTimeMs")]
    public double StartupTimeMs { get; init; }

    [JsonPropertyName("avgLatencyMs")]
    public double AvgLatencyMs { get; init; }

    [JsonPropertyName("p50")]
    public double P50 { get; init; }

    [JsonPropertyName("p90")]
    public double P90 { get; init; }

    [JsonPropertyName("p99")]
    public double P99 { get; init; }

    [JsonPropertyName("throughputRps")]
    public double ThroughputRps { get; init; }

    [JsonPropertyName("memoryMb")]
    public double MemoryMb { get; init; }

    [JsonPropertyName("cpuPercent")]
    public double CpuPercent { get; init; }

    [JsonPropertyName("dbQueryTimeMs")]
    public double DbQueryTimeMs { get; init; }

    [JsonPropertyName("serializationTimeMs")]
    public double SerializationTimeMs { get; init; }

    [JsonPropertyName("binarySizeMb")]
    public double BinarySizeMb { get; init; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; }

    [JsonPropertyName("errorRate")]
    public double ErrorRate { get; init; }
}

