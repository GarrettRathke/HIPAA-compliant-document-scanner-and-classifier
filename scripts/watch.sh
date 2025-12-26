#!/bin/bash

# HelloWorld Monorepo Watch Script
# This script starts development environment with file watching for hot reload

set -e

echo "👁️  Starting HelloWorld Development Environment with Hot Reload..."
echo "Backend: .NET 10 API with dotnet watch on http://localhost:8080"
echo "Frontend: Angular App with ng serve on http://localhost:4200"
echo "File changes will automatically trigger rebuilds"
echo ""

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker and try again."
    exit 1
fi

# Navigate to project root
PROJECT_ROOT=$(dirname $(dirname $(realpath $0)))
cd $PROJECT_ROOT

echo "📁 Working directory: $PROJECT_ROOT"
echo ""

# Start services with watch
echo "🔄 Starting services with file watching enabled..."
docker-compose -f docker-compose.dev.yml watch
