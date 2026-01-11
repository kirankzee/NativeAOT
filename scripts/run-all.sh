#!/bin/bash

# Script to run the complete benchmark suite
# Usage: ./run-all.sh [dataset_size]

set -e

DATASET_SIZE=${1:-1000}
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

echo "=== .NET 10 Native AOT Benchmark Suite ==="
echo ""

# Step 1: Setup database
echo "Step 1: Setting up database with $DATASET_SIZE products..."
bash "$SCRIPT_DIR/setup-database.sh" $DATASET_SIZE

# Step 2: Publish APIs
echo ""
echo "Step 2: Publishing APIs..."
bash "$SCRIPT_DIR/publish-apis.sh"

# Step 3: Start APIs (in background)
echo ""
echo "Step 3: Starting APIs..."
cd "$PROJECT_ROOT/publish/jit"
dotnet Api.Jit.dll &
JIT_PID=$!

cd "$PROJECT_ROOT/publish/aot"
./Api.Aot &
AOT_PID=$!

# Wait for APIs to start
echo "Waiting for APIs to start..."
sleep 10

# Check health
for i in {1..30}; do
    if curl -f -s http://localhost:5000/health > /dev/null && \
       curl -f -s http://localhost:5001/health > /dev/null; then
        echo "APIs are healthy!"
        break
    fi
    if [ $i -eq 30 ]; then
        echo "ERROR: APIs failed to start"
        kill $JIT_PID $AOT_PID 2>/dev/null || true
        exit 1
    fi
    sleep 2
done

# Step 4: Run benchmarks
echo ""
echo "Step 4: Running benchmarks..."
cd "$PROJECT_ROOT/src/BenchmarkRunner"
dotnet run --configuration Release

# Step 5: Stop APIs
echo ""
echo "Step 5: Stopping APIs..."
kill $JIT_PID $AOT_PID 2>/dev/null || true

echo ""
echo "=== Benchmark suite completed! ==="
echo "Results saved to: $PROJECT_ROOT/benchmark-results"

