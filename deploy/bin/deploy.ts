#!/usr/bin/env node
import 'source-map-support/register';
import * as cdk from 'aws-cdk-lib';
import { DeployStack } from '../lib/deploy-stack';

const app = new cdk.App();
new DeployStack(app, 'books-api-lambda-dotnet-aot-based-stack', {

  env: { region: 'eu-north-1' },

});