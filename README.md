# HelloWorld Monorepo

A simple HelloWorld application demonstrating a modern monorepo setup with .NET 10 Web API backend and Angular frontend, fully containerized with Docker for seamless development.

## 🏗️ Architecture

- **Backend**: .NET 10 Web API with Swagger documentation
- **Frontend**: Angular 18+ with standalone components
- **Development**: Docker Compose with hot reload support
- **Communication**: RESTful API with CORS configuration

## 🚀 Quick Start

### Prerequisites

- **Docker** and **Docker Compose** installed
- **Node.js** 18+ (for local development)
- **.NET 10 SDK** (for local development)

### Getting Started

1. **Clone and navigate to the project:**
   ```bash
   cd /path/to/HIPAA-compliant-document-scanner-and-classifier
   ```

2. **Start the development environment:**
   ```bash
   npm run dev
   ```
   Or using the shell script:
   ```bash
   ./scripts/dev.sh
   ```

3. **Access the applications:**
   - **Frontend**: [http://localhost:4200](http://localhost:4200)
   - **Backend API**: [http://localhost:8080](http://localhost:8080)
   - **API Documentation**: [http://localhost:8080/swagger](http://localhost:8080/swagger)

## 📁 Project Structure

```
├── backend/                    # .NET 10 Web API
│   ├── src/HelloWorld.Api/    # Main API project
│   │   ├── Controllers/       # API controllers
│   │   ├── Models/           # Data models
│   │   ├── Program.cs        # Application entry point
│   │   └── *.csproj          # Project configuration
│   ├── HelloWorld.sln        # Solution file
│   ├── Dockerfile.dev        # Development Docker image
│   └── .dockerignore         # Docker ignore patterns
├── frontend/                  # Angular application
│   ├── src/app/
│   │   ├── components/       # Angular components
│   │   ├── services/         # API services
│   │   ├── models/          # TypeScript interfaces
│   │   └── app.*            # Main app files
│   ├── proxy.conf.json       # API proxy configuration
│   ├── Dockerfile.dev        # Development Docker image
│   └── package.json          # NPM dependencies
├── scripts/                   # Development scripts
│   ├── dev.sh               # Start development environment
│   ├── watch.sh             # Start with hot reload
│   └── cleanup.sh           # Clean up Docker resources
├── docker-compose.dev.yml    # Docker Compose configuration
├── package.json              # Root project scripts
└── README.md                 # This file
```

## 🛠️ Development Commands

### Docker-based Development (Recommended)

```bash
# Start all services with build
npm run dev

# Start with hot reload and file watching
npm run dev:watch

# View logs from all services
npm run dev:logs

# Stop all services
npm run dev:down

# Clean up everything (containers, volumes, images)
npm run dev:clean

# Start only backend service
npm run dev:backend

# Start only frontend service
npm run dev:frontend
```

### Local Development

```bash
# Backend (.NET 10)
cd backend
dotnet restore
dotnet run --project src/HelloWorld.Api

# Frontend (Angular)
cd frontend
npm install
npm start
```

## 🌐 API Endpoints

The backend provides the following endpoints:

- `GET /api/hello` - Returns a simple hello message
- `GET /api/hello/{name}` - Returns a personalized hello message
- `GET /swagger` - API documentation (development only)

### Example API Response

```json
{
  "message": "Hello World from .NET 10 API!",
  "timestamp": "2025-12-26T10:30:00.000Z"
}
```

## 🎯 Features

### Backend (.NET 10)
- ✅ Minimal API with controllers
- ✅ Swagger/OpenAPI documentation
- ✅ CORS configuration for frontend
- ✅ Hot reload with `dotnet watch`
- ✅ Docker support

### Frontend (Angular)
- ✅ Standalone components (Angular 18+)
- ✅ HTTP client with proxy configuration
- ✅ Responsive design with modern CSS
- ✅ Error handling and loading states
- ✅ Hot reload with Angular CLI
- ✅ Docker support

### Development Experience
- ✅ Docker Compose for complete environment
- ✅ Hot reload for both backend and frontend
- ✅ File watching with automatic rebuilds
- ✅ Unified scripts for common tasks
- ✅ Network isolation and proper service communication

## 🐳 Docker Configuration

The project uses Docker Compose for development with the following features:

- **Multi-stage builds** for optimized images
- **Volume mounting** for hot reload development
- **Network isolation** with custom bridge network
- **Named volumes** for performance optimization
- **File watching** with Docker Compose Watch feature

## 🔧 Troubleshooting

### Common Issues

1. **Port conflicts:**
   - Backend: Ensure port 8080 is not in use
   - Frontend: Ensure port 4200 is not in use

2. **Docker issues:**
   ```bash
   # Clean up all Docker resources
   npm run dev:clean
   
   # Restart Docker Desktop (if using Docker Desktop)
   ```

3. **Node.js version issues:**
   ```bash
   # Check Node.js version (should be 18+)
   node --version
   ```

4. **Permission issues on Linux/Mac:**
   ```bash
   # Make scripts executable
   chmod +x scripts/*.sh
   ```

### Development Tips

- Use `npm run dev:logs` to see logs from both services
- The Angular app proxies API calls to the backend automatically
- Hot reload is enabled for both .NET and Angular
- Access Swagger documentation at `http://localhost:8080/swagger`

## 🚀 Future Enhancements

This HelloWorld setup provides a foundation for building more complex applications. Consider adding:

- Database integration (PostgreSQL, SQL Server)
- Authentication and authorization (JWT, OAuth)
- Testing frameworks (xUnit for .NET, Jasmine/Karma for Angular)
- CI/CD pipelines (GitHub Actions, Azure DevOps)
- Production Docker configurations
- Monitoring and logging (Application Insights, Serilog)

---

**Happy coding! 🎉**
