#!/bin/bash

# HelloWorld Monorepo Cleanup Script
# This script stops all services and cleans up Docker resources

set -e

echo "🧹 Cleaning up HelloWorld Development Environment..."

# Navigate to project root
PROJECT_ROOT=$(dirname $(dirname $(realpath $0)))
cd $PROJECT_ROOT

echo "📁 Working directory: $PROJECT_ROOT"

# Stop and remove containers
echo "🛑 Stopping services..."
docker-compose -f docker-compose.dev.yml down

# Remove volumes and unused resources
echo "🗑️  Cleaning up volumes and unused resources..."
docker-compose -f docker-compose.dev.yml down -v --remove-orphans
docker system prune -f

echo "✅ Cleanup completed!"
