using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using BenchmarkRunner;

namespace BenchmarkRunner;

public class BenchmarkRunnerService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BenchmarkRunnerService> _logger;
    private readonly string _outputPath;

    public BenchmarkRunnerService(
        HttpClient httpClient,
        ILogger<BenchmarkRunnerService> logger,
        string outputPath)
    {
        _httpClient = httpClient;
        _logger = logger;
        _outputPath = outputPath;
    }

    public async Task<BenchmarkResult> RunBenchmarkAsync(
        string apiType,
        string apiUrl,
        string operation,
        int datasetSize,
        int durationSeconds = 30,
        int requestsPerSecond = 100,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Starting benchmark for {ApiType} - {Operation} with dataset size {DatasetSize}",
            apiType, operation, datasetSize);

        var startTime = DateTime.UtcNow;
        var latencies = new List<double>();
        var errors = 0L;
        var totalRequests = 0L;
        var sw = Stopwatch.StartNew();

        // Warmup
        await WarmupAsync(apiUrl, operation, ct);

        sw.Restart();
        var endTime = startTime.AddSeconds(durationSeconds);

        while (DateTime.UtcNow < endTime && !ct.IsCancellationRequested)
        {
            var batchSize = requestsPerSecond / 10; // 10 batches per second
            var tasks = new List<Task>();

            for (int i = 0; i < batchSize; i++)
            {
                tasks.Add(RunRequestAsync(apiUrl, operation, latencies, ref errors, ref totalRequests, ct));
            }

            await Task.WhenAll(tasks);
            await Task.Delay(100, ct); // ~10 batches per second
        }

        sw.Stop();

        if (latencies.Count == 0)
        {
            throw new InvalidOperationException("No successful requests recorded");
        }

        latencies.Sort();
        var p50 = latencies[(int)(latencies.Count * 0.50)];
        var p90 = latencies[(int)(latencies.Count * 0.90)];
        var p99 = latencies[(int)(latencies.Count * 0.99)];

        var result = new BenchmarkResult
        {
            ApiType = apiType,
            Operation = operation,
            DatasetSize = datasetSize,
            StartupTimeMs = 0, // Would need to measure separately
            AvgLatencyMs = latencies.Average(),
            P50 = p50,
            P90 = p90,
            P99 = p99,
            ThroughputRps = totalRequests / sw.Elapsed.TotalSeconds,
            MemoryMb = GetMemoryUsage(),
            CpuPercent = 0, // Would need separate monitoring
            DbQueryTimeMs = 0, // Would need instrumentation
            SerializationTimeMs = 0, // Would need instrumentation
            BinarySizeMb = 0, // Would need to measure from file system
            Timestamp = DateTime.UtcNow,
            ErrorRate = totalRequests > 0 ? errors / (double)totalRequests * 100 : 0
        };

        _logger.LogInformation(
            "Benchmark completed: Avg={Avg}ms, P99={P99}ms, Throughput={Throughput}RPS, Errors={Errors}",
            result.AvgLatencyMs, result.P99, result.ThroughputRps, errors);

        return result;
    }

    private async Task WarmupAsync(string apiUrl, string operation, CancellationToken ct)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{apiUrl}/health", ct);
            response.EnsureSuccessStatusCode();
            await Task.Delay(1000, ct); // Give the API time to initialize
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Warmup failed, continuing anyway");
        }
    }

    private async Task RunRequestAsync(
        string apiUrl,
        string operation,
        List<double> latencies,
        ref long errors,
        ref long totalRequests,
        CancellationToken ct)
    {
        try
        {
            var sw = Stopwatch.StartNew();
            HttpResponseMessage? response = null;

            try
            {
                response = operation switch
                {
                    "CREATE" => await _httpClient.PostAsJsonAsync($"{apiUrl}/products", new
                    {
                        name = $"Product {Guid.NewGuid()}",
                        description = "Benchmark product",
                        price = 99.99m
                    }, ct),
                    "READ" => await _httpClient.GetAsync($"{apiUrl}/products", ct),
                    "UPDATE" => await _httpClient.PutAsJsonAsync($"{apiUrl}/products/{Guid.NewGuid()}", new
                    {
                        name = "Updated Product",
                        description = "Updated description",
                        price = 149.99m
                    }, ct),
                    "DELETE" => await _httpClient.DeleteAsync($"{apiUrl}/products/{Guid.NewGuid()}", ct),
                    "BULK_READ" => await _httpClient.GetAsync($"{apiUrl}/products/bulk?limit=1000", ct),
                    _ => throw new ArgumentException($"Unknown operation: {operation}")
                };

                response.EnsureSuccessStatusCode();
                sw.Stop();
                lock (latencies)
                {
                    latencies.Add(sw.Elapsed.TotalMilliseconds);
                }
            }
            finally
            {
                response?.Dispose();
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Request failed for operation {Operation}", operation);
            Interlocked.Increment(ref errors);
        }
        finally
        {
            Interlocked.Increment(ref totalRequests);
        }
    }

    private double GetMemoryUsage()
    {
        var process = Process.GetCurrentProcess();
        return process.WorkingSet64 / (1024.0 * 1024.0);
    }

    public async Task SaveResultsAsync(IEnumerable<BenchmarkResult> results, CancellationToken ct = default)
    {
        var resultsList = results.ToList();
        var json = JsonSerializer.Serialize(resultsList, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        var fileName = $"benchmark-results-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json";
        var filePath = Path.Combine(_outputPath, fileName);

        Directory.CreateDirectory(_outputPath);
        await File.WriteAllTextAsync(filePath, json, ct);

        _logger.LogInformation("Results saved to {FilePath}", filePath);
    }
}

