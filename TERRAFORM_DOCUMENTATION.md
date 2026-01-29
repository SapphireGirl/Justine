# Justine Project - Terraform Infrastructure Documentation

## Overview
This document provides comprehensive documentation for the Terraform infrastructure and deployment scripts implemented for the Justine Project, a customer-facing React application showcasing Products, Baskets, and Orders using AWS microservices architecture.

## Architecture Components
- **React Frontend**: Static hosting on S3
- **C# Lambda API**: Serverless API using .NET 8
- **DynamoDB**: NoSQL database for Products, Baskets, and Orders
- **API Gateway**: HTTP API for Lambda integration
- **IAM Roles**: Security and permissions management

## Directory Structure
```
Justine/
├── apiDeploy/           # Terraform for API infrastructure
│   ├── main.tf         # Core infrastructure resources
│   ├── locals.tf       # Local variables and configurations
│   ├── outputs.tf      # Output values
│   ├── versions.tf     # Terraform version constraints
│   └── Makefile        # Build and deployment automation
├── BuildCICD/          # CI/CD pipeline configuration
└── .github/workflows/  # GitHub Actions workflows
```

## Terraform Resources

### 1. Lambda Function (`aws_lambda_function.my_lambda`)
**Purpose**: Hosts the .NET 8 Web API as a serverless function

**Configuration**:
- **Runtime**: `dotnet8`
- **Handler**: `Justine.LambdaWebApi::Justine.LambdaWebApi.LambdaEntryPoint::FunctionHandlerAsync`
- **Memory**: 256 MB
- **Timeout**: 15 seconds
- **Package**: Deployed from ZIP file specified in `var.lambda_package`

**Key Features**:
- Source code hash validation for deployment updates
- Integration with ASP.NET Core serverless framework

### 2. IAM Role (`aws_iam_role.lambda_exec`)
**Purpose**: Provides execution permissions for Lambda function

**Permissions**:
- Basic Lambda execution role (`AWSLambdaBasicExecutionRole`)
- CloudWatch Logs access for monitoring
- Assume role policy for Lambda service

### 3. API Gateway HTTP API (`aws_apigatewayv2_api.http_api`)
**Purpose**: Provides HTTP endpoint for Lambda function access

**Configuration**:
- **Protocol**: HTTP (modern, cost-effective)
- **Integration**: AWS_PROXY with Lambda
- **Payload Format**: 2.0 (optimized for HTTP API)
- **Routing**: Catch-all route `ANY /{proxy+}`

**Features**:
- Auto-deployment enabled
- Default stage configuration
- Lambda permission for API Gateway invocation

## Local Variables (`locals.tf`)

### CORS Configuration
```hcl
cors_origin = var.account_alias == "dev" ? "*" : "https://${var.portal_domain_name}"
```
- **Development**: Allows all origins (`*`)
- **Production**: Restricts to specific domain

### DynamoDB Table Names
```hcl
Products_table_name = "Products"
Baskets_table_name  = "Baskets"
Orders_table_name   = "Orders"
```

## Outputs (`outputs.tf`)

### Lambda Function Name
```hcl
output "lambda_function_name" {
  value = aws_lambda_function.my_lambda.function_name
}
```

### API Endpoint
```hcl
output "api_endpoint" {
  value = aws_apigatewayv2_api.http_api.api_endpoint
}
```

## Version Constraints (`versions.tf`)
- **Terraform**: `>= 1.13.3`
- **AWS Provider**: `~> 5.0`

## Deployment Automation (`Makefile`)

### Available Commands
```makefile
make all     # Plan and apply Terraform changes
make clean   # Remove terraform.tfplan file
make test    # Validate Terraform configuration
```

### Build Process
1. **Validation**: Runs `terraform validate`
2. **Planning**: Creates `terraform.tfplan`
3. **Application**: Auto-applies the plan
4. **Dependencies**: Monitors `.tf` files and ZIP packages

## Required Variables
The following variables must be defined (typically in `terraform.tfvars`):

```hcl
aws_region           = "us-east-1"
lambda_package       = "path/to/lambda.zip"
account_alias        = "dev|staging|prod"
portal_domain_name   = "justine-developer.net"
```

## Security Considerations

### IAM Least Privilege
- Lambda execution role has minimal required permissions
- API Gateway permissions scoped to specific Lambda function

### CORS Policy
- Environment-specific CORS configuration
- Production restricts origins to authorized domains

### Lambda Security
- Function timeout prevents runaway executions
- Memory limits control resource usage

## Deployment Process

### Prerequisites
1. AWS CLI configured with appropriate credentials
2. Terraform >= 1.14.3 installed
3. Lambda deployment package (ZIP) available
4. Required variables defined

### Steps
1. **Package Lambda**:
   ```bash
   cd Justine.LambdaWebApi/src/Justine.LambdaWebApi
   dotnet lambda package
   ```

2. **Deploy Infrastructure**:
   ```bash
   cd apiDeploy
   make all
   ```

3. **Verify Deployment**:
   - Check Lambda function in AWS Console
   - Test API endpoint from outputs
   - Verify CloudWatch logs

## Environment Management

### Development
- CORS allows all origins
- Relaxed security for testing
- Local development support

### Production
- Restricted CORS policy
- Enhanced security measures
- Domain-specific configurations

## Monitoring and Logging

### CloudWatch Integration
- Lambda execution logs automatically captured
- API Gateway access logs available
- Error tracking and alerting capabilities

### Recommended Monitoring
- Lambda duration and error rates
- API Gateway 4xx/5xx responses
- DynamoDB throttling metrics

## Troubleshooting

### Common Issues
1. **Lambda Package Not Found**: Ensure ZIP file exists at specified path
2. **Permission Denied**: Verify AWS credentials and IAM permissions
3. **API Gateway 502 Errors**: Check Lambda function logs in CloudWatch

### Debugging Steps
1. Validate Terraform configuration: `terraform validate`
2. Check AWS credentials: `aws sts get-caller-identity`
3. Review CloudWatch logs for Lambda execution errors
4. Test Lambda function directly in AWS Console

## Future Enhancements

### Planned Additions
- DynamoDB table definitions
- S3 bucket for frontend hosting
- CloudFront distribution
- Route 53 DNS configuration
- CI/CD pipeline integration

### Scalability Considerations
- Lambda provisioned concurrency for consistent performance
- API Gateway caching for improved response times
- DynamoDB auto-scaling configuration
- Multi-region deployment support

## Cost Optimization

### Current Configuration
- HTTP API (lower cost than REST API)
- On-demand Lambda pricing
- Minimal memory allocation (256 MB)
- Short timeout (15 seconds)

### Recommendations
- Monitor Lambda duration for right-sizing
- Consider reserved capacity for predictable workloads
- Implement API Gateway caching for frequently accessed endpoints
- Use DynamoDB on-demand billing for variable workloads