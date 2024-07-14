import * as cdk from 'aws-cdk-lib';
import { Construct } from 'constructs';
import { Code, Function, Runtime, Architecture, Tracing } from 'aws-cdk-lib/aws-lambda';
import { CfnOutput } from 'aws-cdk-lib';
import * as path from 'path';
import * as dynamodb from 'aws-cdk-lib/aws-dynamodb';
import * as iam from 'aws-cdk-lib/aws-iam';
import * as apigatewayv2 from 'aws-cdk-lib/aws-apigatewayv2';
import * as integrations from 'aws-cdk-lib/aws-apigatewayv2-integrations';

export class DeployStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    // Parameters
    const lambdaArchitecture = new cdk.CfnParameter(this, 'LambdaArchitecture', {
      type: 'String',
      allowedValues: ['arm64', 'x86_64'],
      description: 'Enter arm64 or x86_64',
    });

    // DynamoDB Table
    const table = new dynamodb.Table(this, 'Table', {
      tableName: 'dotnet-aot-BooksTable',
      partitionKey: { name: 'PK', type: dynamodb.AttributeType.STRING },
      sortKey: { name: 'SK', type: dynamodb.AttributeType.STRING },
      billingMode: dynamodb.BillingMode.PAY_PER_REQUEST,
    });

    // Add Global Secondary Index (GSI) for Type
    table.addGlobalSecondaryIndex({
      indexName: 'TypeIndex',
      partitionKey: { name: 'TYPE', type: dynamodb.AttributeType.STRING },
      projectionType: dynamodb.ProjectionType.ALL,
    });

    // IAM Role for Lambda functions
    const lambdaRole = new iam.Role(this, 'LambdaRole', {
      assumedBy: new iam.ServicePrincipal('lambda.amazonaws.com'),
    });

    // Attach necessary policies to the role
    lambdaRole.addToPolicy(new iam.PolicyStatement({
      actions: [
        'logs:CreateLogGroup',
        'logs:CreateLogStream',
        'logs:PutLogEvents',
      ],
      resources: ['*'],
    }));

    table.grantReadData(lambdaRole);
    table.grantReadWriteData(lambdaRole);

    // HTTP API Gateway
    const httpApi = new apigatewayv2.HttpApi(this, 'HttpApi');

    // Lambda Functions
    const functions = [
      { name: 'dotnet-aot-GetBooksFunction', path: './BooksApiNative.Functions.GetBooks', method: apigatewayv2.HttpMethod.GET, route: '/' },
      { name: 'dotnet-aot-AddEditBooksFunction', path: './BooksApiNative.Functions.AddEditBook', method: apigatewayv2.HttpMethod.POST, route: '/{id}' },
      { name: 'dotnet-aot-GetBookFunction', path: './BooksApiNative.Functions.GetBook', method: apigatewayv2.HttpMethod.GET, route: '/{id}' },
      { name: 'dotnet-aot-DeleteBookFunction', path: './BooksApiNative.Functions.DeleteBook', method: apigatewayv2.HttpMethod.DELETE, route: '/{id}' },
    ];

    functions.forEach(func => {
      const lambdaFunction = new Function(this, func.name, {
        functionName: func.name,
        runtime: Runtime.PROVIDED_AL2,
        code: Code.fromAsset(path.join(__dirname, '..', '..', func.path, 'bin', 'Release','net8.0','linux-arm64')),
        handler: 'bootstrap',
        memorySize: 1024,
        timeout: cdk.Duration.seconds(30),
        tracing: Tracing.ACTIVE,
        environment: {
          PRODUCT_TABLE_NAME: table.tableName,
        },
        role: lambdaRole,
        architecture: Architecture.ARM_64, //lambdaArchitecture.valueAsString === 'arm64' ? Architecture.ARM_64 : Architecture.X86_64,
      });

      // Add route to the HTTP API Gateway
      httpApi.addRoutes({
        path: func.route,
        methods: [func.method],
        integration: new integrations.HttpLambdaIntegration(func.name, lambdaFunction),
      });

      // Define the log group for the Lambda function
      new cdk.aws_logs.LogGroup(this, `${func.name}LogGroup`, {
        logGroupName: `/aws/lambda/${lambdaFunction.functionName}`,
        removalPolicy: cdk.RemovalPolicy.DESTROY,
      });
    });

    // Output the API Gateway URL
    new CfnOutput(this, 'ApiUrl', {
      description: 'API Gateway endpoint URL',
      value: httpApi.url!,
    });
  }
}