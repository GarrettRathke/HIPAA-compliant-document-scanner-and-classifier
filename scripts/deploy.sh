#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
TERRAFORM_DIR="$PROJECT_ROOT/infrastructure/terraform"

echo "🚀 Deploying Receipt Parser to AWS..."
echo ""

# Check if OpenAI API key is provided
if [ -z "$OPENAI_API_KEY" ]; then
    echo "❌ Error: OPENAI_API_KEY environment variable is not set"
    echo "Usage: OPENAI_API_KEY=sk-... ./deploy.sh"
    exit 1
fi

# Build frontend
echo "📦 Building frontend..."
bash "$SCRIPT_DIR/build-frontend.sh"
echo ""

# Build Lambda handler
echo "📦 Building Lambda handler..."
bash "$SCRIPT_DIR/build-lambda.sh"
echo ""

# Deploy with Terraform
cd "$TERRAFORM_DIR"

echo "🏗️  Initializing Terraform..."
terraform init

echo ""
echo "📋 Terraform plan..."
terraform plan -var="openai_api_key=$OPENAI_API_KEY" -out=tfplan

echo ""
read -p "Apply Terraform changes? (yes/no): " -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    echo "🚀 Applying Terraform configuration..."
    terraform apply tfplan
    
    echo ""
    echo "✅ Deployment complete!"
    echo ""
    echo "📊 Outputs:"
    terraform output -raw frontend_url 2>/dev/null && echo ""
    terraform output -raw api_endpoint 2>/dev/null && echo ""
    
    # Get outputs for next steps
    CLOUDFRONT_DIST=$(terraform output -raw cloudfront_distribution_id)
    S3_BUCKET=$(terraform output -raw s3_bucket_name)
    FRONTEND_DIST="$PROJECT_ROOT/frontend/dist"
    
    echo ""
    echo "📤 Uploading frontend to S3..."
    aws s3 sync "$PROJECT_ROOT/frontend/dist/hello-world-app/browser/" "s3://$S3_BUCKET" --delete
    
    echo ""
    echo "🔄 Invalidating CloudFront cache..."
    aws cloudfront create-invalidation --distribution-id "$CLOUDFRONT_DIST" --paths "/*" --query 'Invalidation.Id' --output text
    
    echo ""
    echo "✨ All done! Application is live."
else
    echo "Deployment cancelled."
    rm -f tfplan
    exit 0
fi
