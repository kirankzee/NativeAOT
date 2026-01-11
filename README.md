# .NET 10 Native AOT vs JIT Benchmark Suite

A comprehensive benchmarking application to compare .NET 10 Web APIs compiled with Just-In-Time (JIT) compilation versus Native AOT (Ahead-Of-Time) compilation. This project demonstrates performance differences in cold start time, CRUD latency, throughput, memory usage, and binary size.

## 🎯 Project Overview

This benchmark suite consists of:

1. **Api.Jit** - Standard .NET 10 Minimal API with JIT compilation
2. **Api.Aot** - .NET 10 Minimal API with Native AOT compilation
3. **BenchmarkRunner** - Performance testing tool that collects metrics
4. **React Dashboard** - Web UI for visualizing benchmark results

Both APIs perform identical CRUD operations against a PostgreSQL database, ensuring fair comparison.

## 📋 Prerequisites

- .NET 10 SDK
- PostgreSQL 16+ (or Docker)
- Node.js 18+ (for React dashboard)
- PowerShell 7+ (for Windows scripts) or Bash (for Linux/macOS)

## 🚀 Quick Start

### Option 1: Docker Compose (Recommended)

```bash
# Start all services (PostgreSQL + both APIs)
docker-compose up -d

# Wait for services to be healthy
docker-compose ps

# Setup database
docker-compose exec postgres psql -U postgres -d benchmarkdb -f /docker-entrypoint-initdb.d/init.sql

# Seed database (1k, 10k, or 100k products)
docker-compose exec postgres psql -U postgres -d benchmarkdb -c "INSERT INTO products ..." # Use seed script
```

### Option 2: Manual Setup

1. **Start PostgreSQL**

```bash
# Linux/macOS
docker run -d \
  --name benchmark-postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=benchmarkdb \
  -p 5432:5432 \
  postgres:16-alpine

# Windows PowerShell
docker run -d --name benchmark-postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=benchmarkdb -p 5432:5432 postgres:16-alpine
```

2. **Setup Database**

```bash
# Linux/macOS
bash scripts/setup-database.sh 1000  # 1k products

# Windows PowerShell
.\scripts\setup-database.ps1 -DatasetSize 1000
```

3. **Publish APIs**

```bash
# Linux/macOS
bash scripts/publish-apis.sh

# Windows PowerShell
.\scripts\publish-apis.ps1
```

4. **Run APIs**

```bash
# Terminal 1 - JIT API
cd publish/jit
dotnet Api.Jit.dll

# Terminal 2 - AOT API
cd publish/aot
./Api.Aot
```

5. **Run Benchmarks**

```bash
cd src/BenchmarkRunner
dotnet run
```

6. **View Dashboard**

```bash
cd frontend
npm install
npm run dev
```

Open http://localhost:3000 and upload the benchmark results JSON file.

## 📁 Project Structure

```
NativeAOT/
├── src/
│   ├── Api.Jit/              # JIT-compiled API
│   ├── Api.Aot/              # Native AOT-compiled API
│   ├── BenchmarkRunner/      # Performance testing tool
│   └── Shared/
│       ├── Models/           # Shared data models
│       └── DataAccess/       # PostgreSQL repository (AOT-safe)
├── frontend/                 # React dashboard
├── scripts/                  # Setup and utility scripts
├── docker-compose.yml        # Docker orchestration
├── Dockerfile.jit            # JIT API container
├── Dockerfile.aot            # AOT API container
└── README.md
```

## 🔧 Configuration

### API Configuration

Both APIs use the same configuration in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Port=5432;Database=benchmarkdb;Username=postgres;Password=postgres"
  }
}
```

### Benchmark Configuration

Edit `src/BenchmarkRunner/appsettings.json`:

```json
{
  "JitApiUrl": "http://localhost:5000",
  "AotApiUrl": "http://localhost:5001",
  "OutputPath": "./benchmark-results"
}
```

## 📊 API Endpoints

Both APIs expose identical endpoints:

- `POST /products` - Create product
- `GET /products/{id}` - Get product by ID
- `GET /products?page=1&pageSize=20` - Paginated list
- `PUT /products/{id}` - Update product
- `DELETE /products/{id}` - Delete product
- `GET /products/bulk?limit=1000` - Bulk read
- `GET /health` - Health check

## 📈 Benchmark Metrics

The benchmark suite collects the following metrics:

- **Startup Time** - Cold start latency
- **Average Latency** - Mean response time
- **P50/P90/P99 Latency** - Percentile latencies
- **Throughput** - Requests per second (RPS)
- **Memory Usage** - RSS/GC heap size
- **CPU Usage** - CPU percentage
- **Error Rate** - Failed requests percentage
- **Binary Size** - Published executable size

## 🎨 Dashboard Features

The React dashboard provides:

- **Summary Cards** - Average improvements across all benchmarks
- **Latency Charts** - Side-by-side comparison of JIT vs AOT latency
- **Throughput Charts** - RPS comparison
- **Memory Charts** - Memory usage comparison
- **Filters** - Filter by operation type and dataset size
- **File Upload** - Load custom benchmark results

## 🔬 Benchmark Scenarios

The BenchmarkRunner tests:

1. **CREATE** - Product creation
2. **READ** - Product retrieval
3. **UPDATE** - Product updates
4. **DELETE** - Product deletion
5. **BULK_READ** - Large dataset retrieval

Each operation is tested with dataset sizes: 1k, 10k, and 100k products.

## 🐳 Docker Commands

```bash
# Build all images
docker-compose build

# Start services
docker-compose up -d

# View logs
docker-compose logs -f api-jit
docker-compose logs -f api-aot

# Stop services
docker-compose down

# Stop and remove volumes
docker-compose down -v
```

## 📝 Database Schema

```sql
CREATE TABLE products (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name TEXT NOT NULL,
    description TEXT NOT NULL,
    price NUMERIC(10, 2) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);
```

## 🛠️ Development

### Build Solution

```bash
dotnet build
```

### Run Tests (if applicable)

```bash
dotnet test
```

### Update Dependencies

```bash
dotnet restore
cd frontend && npm install
```

## 📄 Results Format

Benchmark results are saved as JSON with the following structure:

```json
[
  {
    "apiType": "JIT",
    "operation": "CREATE",
    "datasetSize": 1000,
    "startupTimeMs": 150.5,
    "avgLatencyMs": 12.3,
    "p50": 10.1,
    "p90": 18.5,
    "p99": 25.2,
    "throughputRps": 81.3,
    "memoryMb": 125.5,
    "cpuPercent": 15.2,
    "dbQueryTimeMs": 5.1,
    "serializationTimeMs": 2.3,
    "binarySizeMb": 45.2,
    "timestamp": "2024-01-15T10:30:00Z",
    "errorRate": 0.1
  }
]
```

## 🎯 Expected Results

Based on typical Native AOT performance characteristics, you should observe:

- ✅ **Faster Cold Start** - AOT starts in milliseconds vs seconds
- ✅ **Lower Memory Footprint** - Reduced RSS memory usage
- ✅ **Smaller Binary Size** - Single executable without runtime
- ✅ **Predictable Performance** - More consistent latency
- ⚠️ **Similar Hot Path Performance** - Both perform similarly under load

## 🐛 Troubleshooting

### APIs won't start

- Check PostgreSQL is running and accessible
- Verify connection string in `appsettings.json`
- Check ports 5000 and 5001 are available

### Database connection errors

- Ensure PostgreSQL is running: `docker ps`
- Test connection: `psql -h localhost -U postgres -d benchmarkdb`
- Check firewall rules if using remote database

### Native AOT build fails

- Ensure you have .NET 10 SDK installed
- Check for AOT-incompatible code (reflection, dynamic code generation)
- Review build warnings for AOT compatibility

### Dashboard won't load

- Ensure Node.js 18+ is installed
- Run `npm install` in the `frontend` directory
- Check browser console for errors

## 📚 Additional Resources

- [.NET Native AOT Documentation](https://learn.microsoft.com/dotnet/core/deploying/native-aot/)
- [Npgsql Documentation](https://www.npgsql.org/doc/)
- [Recharts Documentation](https://recharts.org/)

## 🤝 Contributing

This is a benchmark suite project. Feel free to:
- Report issues
- Suggest improvements
- Add additional benchmark scenarios
- Enhance the dashboard

## 📄 License

This project is provided as-is for benchmarking purposes.

## 🙏 Acknowledgments

Built to demonstrate the performance characteristics of .NET 10 Native AOT compilation compared to traditional JIT compilation.

