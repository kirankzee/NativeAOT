export interface BenchmarkResult {
  apiType: 'JIT' | 'AOT';
  operation: string;
  datasetSize: number;
  startupTimeMs: number;
  avgLatencyMs: number;
  p50: number;
  p90: number;
  p99: number;
  throughputRps: number;
  memoryMb: number;
  cpuPercent: number;
  dbQueryTimeMs: number;
  serializationTimeMs: number;
  binarySizeMb: number;
  timestamp: string;
  errorRate: number;
}

export interface ComparisonData {
  jit: BenchmarkResult;
  aot: BenchmarkResult;
  improvement: {
    avgLatency: number;
    p99: number;
    throughput: number;
    memory: number;
  };
}

