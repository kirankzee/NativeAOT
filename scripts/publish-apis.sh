#!/bin/bash

# Script to publish both APIs
# Usage: ./publish-apis.sh

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

echo "Publishing APIs..."

# Publish JIT API
echo "Publishing JIT API..."
cd "$PROJECT_ROOT/src/Api.Jit"
dotnet publish -c Release -o ../../publish/jit

# Publish AOT API
echo "Publishing Native AOT API..."
cd "$PROJECT_ROOT/src/Api.Aot"
dotnet publish -c Release -o ../../publish/aot

echo "APIs published successfully!"
echo "JIT API: $PROJECT_ROOT/publish/jit"
echo "AOT API: $PROJECT_ROOT/publish/aot"

