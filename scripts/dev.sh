#!/bin/bash

# HelloWorld Monorepo Development Script
# This script starts the entire development environment with Docker Compose

set -e

echo "🚀 Starting HelloWorld Development Environment..."
echo "Backend: .NET 10 API on http://localhost:8080"
echo "Frontend: Angular App on http://localhost:4200"
echo ""

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker and try again."
    exit 1
fi

# Check if Docker Compose is available
if ! command -v docker-compose > /dev/null 2>&1; then
    echo "❌ docker-compose is not available. Please install Docker Compose."
    exit 1
fi

# Navigate to project root
PROJECT_ROOT=$(dirname $(dirname $(realpath $0)))
cd $PROJECT_ROOT

echo "📁 Working directory: $PROJECT_ROOT"
echo ""

# Start services
echo "🏗️  Building and starting services..."
docker-compose -f docker-compose.dev.yml up --build
