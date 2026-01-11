using BenchmarkRunner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables();

builder.Services.AddLogging(configure => configure.AddConsole());
builder.Services.AddHttpClient<BenchmarkRunnerService>();

var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
var config = host.Services.GetRequiredService<IConfiguration>();

var jitUrl = config["JitApiUrl"] ?? "http://localhost:5000";
var aotUrl = config["AotApiUrl"] ?? "http://localhost:5001";
var outputPath = config["OutputPath"] ?? "./benchmark-results";

logger.LogInformation("Benchmark Runner Starting");
logger.LogInformation("JIT API URL: {JitUrl}", jitUrl);
logger.LogInformation("AOT API URL: {AotUrl}", aotUrl);
logger.LogInformation("Output Path: {OutputPath}", outputPath);

var httpClientFactory = host.Services.GetRequiredService<IHttpClientFactory>();
var httpClient = httpClientFactory.CreateClient();
httpClient.Timeout = TimeSpan.FromMinutes(5);

var results = new List<BenchmarkResult>();
var datasetSizes = new[] { 1000, 10000, 100000 };
var operations = new[] { "CREATE", "READ", "UPDATE", "DELETE", "BULK_READ" };

foreach (var datasetSize in datasetSizes)
{
    logger.LogInformation("=== Starting benchmarks for dataset size: {DatasetSize} ===", datasetSize);

    foreach (var operation in operations)
    {
        try
        {
            var runner = new BenchmarkRunnerService(httpClient, logger, outputPath);

            // Run JIT benchmark
            logger.LogInformation("Running JIT benchmark for {Operation}...", operation);
            var jitResult = await runner.RunBenchmarkAsync(
                "JIT", jitUrl, operation, datasetSize, durationSeconds: 30, requestsPerSecond: 100);
            results.Add(jitResult);
            await Task.Delay(5000); // Cooldown between benchmarks

            // Run AOT benchmark
            logger.LogInformation("Running AOT benchmark for {Operation}...", operation);
            var aotResult = await runner.RunBenchmarkAsync(
                "AOT", aotUrl, operation, datasetSize, durationSeconds: 30, requestsPerSecond: 100);
            results.Add(aotResult);
            await Task.Delay(5000); // Cooldown between benchmarks
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to run benchmark for {Operation} with dataset {DatasetSize}", operation, datasetSize);
        }
    }
}

// Save all results
var finalRunner = new BenchmarkRunnerService(httpClient, logger, outputPath);
await finalRunner.SaveResultsAsync(results);

logger.LogInformation("Benchmark Runner Completed. Total results: {Count}", results.Count);

