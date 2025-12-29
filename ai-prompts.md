# AI Prompts

This is a high level overview of what prompts were used to create the project.

## Project Scaffolding
### Plan Phase 
__Prompt__: ```Create a HelloWorld monorepo configuration having a C# backend with .NET 10 and an Angular frontend. Also add Docker support for local development.```

__Planning Agent Response__: ```Further Considerations
1. Port configuration: Backend on 5000 (external) mapping to 8080 (internal .NET 10 default), frontend on 4200 - need other port mappings?
2. Development containers: Include full VS Code devcontainer setup with extensions, or prefer manual Docker Compose workflow?
3. Database integration: Plan for future database service in Docker Compose, or keep simple for HelloWorld scope?```

__Additional Prompt Instructions__:
1. The default port mappings are sufficient.
2. Use a Docker Compose workflow.
3. Keep it simple for the HelloWorld scope.


## Project Main Features

### Open AI Vision API for OCR
__Prompt__: ```Create a plan for integrating the Open AI Vision API into the backend.```

__Planning Agent Response__: ```Further Considerations
1. Structured data format - Should extracted data follow a specific schema (vendor, date, items array, totals) or flexible key-value pairs?
2. OCR accuracy handling - How to handle partial extractions, unclear text, or multiple interpretation possibilities?
3. Image preprocessing - Auto-rotate, enhance contrast, or resize images before sending to OpenAI for better OCR results?```

__Additional Prompt Instructions__:
1. Use flexible key-value pairs for extracted data.
2. Include all extracted data, even partial or unclear. In the case of multiple interpretation possibilities, choose the highest probable option.
3. Image preprocessing is not needed.


__Prompt__: ```Using Terraform, prepare an AWS environment for the application.```

__Planning Agent Response__: ```Plan: AWS Infrastructure for Receipt-Invoice-Parser with Terraform
Deploy a containerized receipt-invoice-parser application on AWS using Terraform. The plan provisions an ECS Fargate cluster with containerized .NET backend and Angular frontend, ALB for routing, VPC networking, ECR for container images, and AWS Secrets Manager for the OpenAI API key. No database or persistent storage is needed since images are processed in-memory by the OpenAI Vision API.

Steps
Create core AWS infrastructure modules — VPC with public/private subnets, Internet Gateway, NAT Gateway, and security groups for ALB and ECS tasks.

Set up container registry and build pipeline — ECR repositories for backend and frontend images, with instructions for pushing Docker images from your local docker-compose setup.

Configure secrets management — AWS Secrets Manager to store the OpenAI API key, with IAM role permissions for ECS tasks to access it.

Create ECS Fargate cluster and task definitions — Separate task definitions for backend (.NET 10 on port 8080) and frontend (Angular on port 4200), with environment variable injection.

Deploy Application Load Balancer — ALB with target groups routing /api/* to backend and root / to frontend, with health checks configured.

Add monitoring and logging — CloudWatch log groups for both services, with application logs streamed from containers.

Output essential values — Terraform outputs for ALB DNS name, ECR repository URIs, and task definition ARNs for reference.

Further Considerations
Scaling strategy — Fixed task count for MVP or auto-scaling policies based on CPU/memory metrics? Need to decide minimum/maximum task counts.

Environment separation — Single staging environment or separate dev/staging/production VPCs? Consider using Terraform workspaces or separate state files per environment.

SSL/TLS and domain — Use ACM certificate with a custom domain (Route 53) or start with ALB DNS name? Consider HTTPS redirect from HTTP.

CI/CD integration — Push images to ECR automatically via GitHub Actions or CodePipeline, or build locally and push manually before Terraform apply?```


__Additional Prompt Instructions__:
```Simplify the AWS system architecture plan. Containerization is not needed for the cloud. Instead, the frontend can be hosted in an S3 bucket, and the backend can be a Lambda function behind an API Gateway.```


__Planning Agent Response__: ```
Plan: Serverless AWS Infrastructure for Receipt-Invoice-Parser
Deploy the application using AWS serverless services: S3 + CloudFront for the Angular frontend, Lambda for the .NET backend, and API Gateway for routing. This eliminates container overhead and reduces operational complexity. The OpenAI API key is stored securely in AWS Secrets Manager, and CloudWatch provides logging.

Steps
Create S3 bucket for frontend — Store built Angular app with CloudFront distribution for CDN caching and HTTPS, with custom error handling for SPA routing (404 → index.html).

Create Lambda function for backend — Build a Node.js wrapper or .NET isolated function that proxies requests to the core backend logic, with environment variables for OpenAI API key reference.

Set up API Gateway — Create REST API with proxy integration to Lambda, routing /api/* requests with proper CORS headers.

Configure Secrets Manager — Store OpenAI API key and grant Lambda IAM role read access via policy.

Build deployment pipeline — Create scripts to build Angular app, zip Lambda code, and deploy via Terraform.

Define outputs — Export CloudFront domain, API Gateway URL, and S3 bucket name for reference.

Further Considerations
Backend runtime choice — Should Lambda wrap the .NET backend (requiring a .NET runtime layer/container image) or should the backend logic be rewritten in Node.js for simplicity? Node.js is simpler but requires refactoring your C# code.

Build and deployment — Do you want automatic builds in CI/CD (GitHub Actions → ECR/Lambda) or manual local builds before terraform apply?

Custom domain — Use CloudFront's default domain or integrate Route53 for example.com with ACM certificate?

State management — Store Terraform state locally or in S3 with DynamoDB locking for team collaboration?
```


__Additional Prompt Instructions__:
1. Lambda should wrap the .NET backend.
2. Manual local builds before applying Terraform deployment is sufficient.
3. Use CloudFront's default domain.
4. Store Terraform state locally.



__Planning Agent Response__: ```
Plan: Serverless AWS Infrastructure with .NET Lambda Wrapper
Deploy the receipt-invoice-parser as serverless components: Angular frontend on S3 + CloudFront, .NET backend wrapped as a Lambda function via a Node.js proxy handler, and API Gateway routing. Build locally, zip artifacts, and deploy with Terraform using local state storage. Secrets Manager stores the OpenAI API key securely.

Steps
Create Node.js Lambda wrapper — Build a thin proxy handler in infrastructure/lambda/index.js that invokes the .NET backend process (using Lambda Layers or container image), forwards requests/responses, and manages environment variables.

Create Terraform configuration — Define provider, variables, and resources in terraform for S3 bucket, CloudFront, API Gateway, Lambda function, IAM roles, and Secrets Manager.

Add build scripts — Create scripts in scripts to build Angular frontend (npm run build), zip Lambda code, and prepare deployment artifacts.

Configure local state — Define Terraform backend as local (default) in infrastructure/terraform/provider.tf.

Create deployment documentation — Add instructions for environment setup, secret injection, and terraform apply workflow in DEPLOYMENT.md.

Define outputs — Export CloudFront domain URL, API Gateway endpoint, and S3 bucket name from infrastructure/terraform/outputs.tf.

Further Considerations
.NET runtime in Lambda — Use AWS Lambda .NET 10 runtime (if available) or container image? Container image approach is more flexible but slightly more complex.

Frontend build output — Where should the built Angular app (from npm run build) be stored before S3 upload—temporary directory or committed to repo?

CORS configuration — Should API Gateway enforce specific CORS origins or allow all (*)?
```


__Additional Prompt Instructions__:
1. Use AWS Lambda .NET 10 runtime if available. Otherwise, use .NET 8.
2. Before S3 upload, store the built Angular app in a temporary directory.
3. The API Gateway can allow all origins.