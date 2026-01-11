#!/bin/bash

# Script to run benchmarks
# Usage: ./run-benchmarks.sh

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

echo "Starting benchmark run..."

# Check if APIs are running
echo "Checking if APIs are running..."
if ! curl -f -s http://localhost:5000/health > /dev/null; then
    echo "ERROR: JIT API is not running on http://localhost:5000"
    exit 1
fi

if ! curl -f -s http://localhost:5001/health > /dev/null; then
    echo "ERROR: AOT API is not running on http://localhost:5001"
    exit 1
fi

# Run benchmark runner
echo "Running benchmark runner..."
cd "$PROJECT_ROOT/src/BenchmarkRunner"
dotnet run --configuration Release

echo "Benchmarks completed! Results saved to ./benchmark-results"

