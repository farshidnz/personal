import { CfnElement } from "aws-cdk-lib";
import { Template } from "aws-cdk-lib/assertions";
import { CfnTaskDefinition } from "aws-cdk-lib/aws-ecs";
import { Cr3ApiApp } from "../bin/cr3api-app";

describe("Should Cr3 api cdk stack", () => {
  const environmentName = process.env.ENVIRONMENT_NAME || "";
  const account = process.env.AWS_ACCOUNT_ID;
  const region = process.env.AWS_REGION;

  const app = new Cr3ApiApp();
  const stack = app.cr3ApiStack;
  const service = app.cr3ApiStack.ecsConstruct;
  const template = Template.fromStack(stack);

  test("Should create ecs fargate service", () => {
    template.hasResourceProperties("AWS::ECS::Service", {
      Cluster: `${environmentName}-ecs`,
      ServiceName: `${environmentName}-cr3api-ecsService`,
      DeploymentConfiguration: {
        MaximumPercent: 200,
        MinimumHealthyPercent: 50,
      },
      DesiredCount: 1,
      EnableECSManagedTags: false,
      HealthCheckGracePeriodSeconds: 60,
      LaunchType: "FARGATE",
      LoadBalancers: [
        {
          ContainerName: `${environmentName}-cr3api-container`,
          ContainerPort: 80,
          TargetGroupArn: {
            Ref: stack.getLogicalId(
              service.targetGroup.node.defaultChild as CfnElement
            ),
          },
        },
      ],
      TaskDefinition: {
        Ref: stack.getLogicalId(
          service.serviceTaskDefinition.node.defaultChild as CfnTaskDefinition
        ),
      },
    });
  });

  test("Should create auto scaling group", () => {
    template.hasResourceProperties(
      "AWS::ApplicationAutoScaling::ScalableTarget",
      {
        MaxCapacity: 10,
        MinCapacity: 1,
        ResourceId: {
          "Fn::Join": [
            "",
            [
              `service/${environmentName}-ecs/`,
              {
                "Fn::GetAtt": [
                  stack.getLogicalId(
                    service.fargateService.node.defaultChild as CfnElement
                  ),
                  "Name",
                ],
              },
            ],
          ],
        },
        RoleARN: {
          "Fn::Join": [
            "",
            [
              "arn:",
              {
                Ref: "AWS::Partition",
              },
              `:iam::${account}:role/aws-service-role/ecs.application-autoscaling.amazonaws.com/AWSServiceRoleForApplicationAutoScaling_ECSService`,
            ],
          ],
        },
        ScalableDimension: "ecs:service:DesiredCount",
        ServiceNamespace: "ecs",
      }
    );
  });

  test("Should create ecs task IAM role", () => {
    template.hasResourceProperties("AWS::IAM::Role", {
      AssumeRolePolicyDocument: {
        Statement: [
          {
            Action: "sts:AssumeRole",
            Effect: "Allow",
            Principal: {
              Service: "ecs-tasks.amazonaws.com",
            },
          },
        ],
        Version: "2012-10-17",
      },
    });
  });

  test("Should create IAM managed policy", () => {
    template.hasResourceProperties("AWS::IAM::ManagedPolicy", {
      PolicyDocument: {
        Statement: [
          {
            Action: [
              "ssm:GetParameter",
              "ssm:GetParameters",
              "ssm:GetParametersByPath",
              "secretsmanager:GetSecretValue",
              "kms:Decrypt",
              "dynamodb:List*",
              "dynamodb:Get*",
              "dynamodb:Query",
              "dynamodb:Scan",
              "dynamodb:DescribeTable",
              "events:Put*",
              "sns:Publish",
            ],
            Effect: "Allow",
            Resource: "*",
          },
          {
            Action: "sqs:*",
            Effect: "Allow",
            Resource: [
              `arn:aws:sqs:${region}:${account}:SubscriberMgmt-UpdateLeanplumEnrichment-Events-stg`,
              `arn:aws:sqs:${region}:${account}:EmailCommand`,
            ],
          },
        ],
        Version: "2012-10-17",
      },
      Description: "",
      Path: "/",
      Roles: [
        {
          Ref: stack.getLogicalId(
            service.taskRole.node.defaultChild as CfnElement
          ),
        },
      ],
    });
  });
});
