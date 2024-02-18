import {
  EcsConstruct,
  getEnv,
  getResourceName,
  RestApiConstruct,
  ServiceVisibility,
} from "@cashrewards/cdk-lib";
import { Stack, StackProps } from "aws-cdk-lib";
import { RestApi } from "aws-cdk-lib/aws-apigateway";
import {
  Effect,
  IRole,
  ManagedPolicy,
  PolicyDocument,
  PolicyStatement,
} from "aws-cdk-lib/aws-iam";
import { Construct } from "constructs";

export class Cr3ApiStack extends Stack {
  public ecsConstruct: EcsConstruct;
  public ecsTaskRole: IRole;
  public restApiConstruct: RestApiConstruct;
  public restApi: RestApi;
  constructor(scope: Construct, id: string, props?: StackProps) {
    super(scope, id, props);

    const apiDNS = getEnv("APIDNS");

    this.ecsConstruct = new EcsConstruct(
      this,
      getResourceName(getEnv("PROJECT_NAME")),
      {
        environmentName: getEnv("ENVIRONMENT_NAME"),
        serviceName: getEnv("PROJECT_NAME"),
        visibility: ServiceVisibility.PUBLIC,
        imageTag: getEnv("VERSION"),
        listenerRulePriority: 50000,
        pathPattern: "api",
        healthCheckPath: "health",

        useOpenTelemetry: true,
        /* Scaling Rules */
        minCapacity: +getEnv("ScalingMinCapacity"),
        maxCapacity: +getEnv("ScalingMaxCapacity"),
        desiredCount: +getEnv("ScalingDesiredCount"),
        cpu: +getEnv("cpu"),
        memoryLimitMiB: +getEnv("memoryLimitMiB"),
        scalingRule: {
          cpuScaling: {
            targetUtilizationPercent: 70,
            scaleInCooldown: 300,
            scaleOutCooldown: 300,
            alarm: {
              enableSlackAlert: true,
            },
          },
          memoryScaling: {
            targetUtilizationPercent: 80,
            scaleInCooldown: 300,
            scaleOutCooldown: 300,
            alarm: {
              enableSlackAlert: true,
            },
          },
        },

        /* Routing Rules */
        customDomain: "cr3api",
        alternateDomains:
          getEnv("AlternateDNS").length > 0
            ? getEnv("AlternateDNS").split(",")
            : [],

        /* Configuration */
        environment: {
          Pipeline: getEnv("Pipeline"),
          LOG_LEVEL: getEnv("LOG_LEVEL"),
          UseStrapiV4: getEnv("UseStrapiV4"),
          Config__PopularStoreOrderIds: getEnv("Config_PopularStoreOrderIds"),
          Config__CustomTrackingMerchantList: getEnv(
            "Config_CustomTrackingMerchantList"
          ),
          Config__InStoreNetworkId: getEnv("Config_InStoreNetworkId"),
          Config__OnlineStoreNetworkId: getEnv("Config_OnlineStoreNetworkId"),
          Config__MobileSpecificNetworkId: getEnv(
            "Config_MobileSpecificNetworkId"
          ),
          Config__MerchantTierCommandTypeId: getEnv(
            "Config_MerchantTierCommandTypeId"
          ),
          Config__OfferBackgroundImageDefault: getEnv(
            "Config_OfferBackgroundImageDefault"
          ),
          Config__TrendingStoreInfoTable: getEnv(
            "Config_TrendingStoreInfoTable"
          ),
          Config__PopularStoreInfoTable: getEnv("Config_PopularStoreInfoTable"),
          Config__TrendingStoreS3BucketKey: getEnv(
            "Config_TrendingStoreS3BucketKey"
          ),
          Config__TrendingStoreS3BucketName: getEnv(
            "Config_TrendingStoreS3BucketName"
          ),
          Config__ClickCreateTopicArn: getEnv("Config_ClickCreateTopicArn"),
          Config__GiftCardBucketName: getEnv("Config_GiftCardBucketName"),
          Config__PromotionBucketName: getEnv("Config_PromotionBucketName"),
          Config__MemberCreateTopicArn: getEnv("Config_MemberCreateTopicArn"),
          Config__RedisMasters: getEnv("Config_RedisMasters"),
          Config__AllowedCorsOrigins: getEnv("Config_AllowedCorsOrigins"),
          Config__Transaction__CashrewardsReferAMateMerchantId: getEnv(
            "Config_Transaction_CashrewardsReferAMateMerchantId"
          ),
          Config__Transaction__CashrewardsBonusMerchantId: getEnv(
            "Config_Transaction_CashrewardsBonusMerchantId"
          ),
          Config__Transaction__CashrewardsActivationBonusMerchantId: getEnv(
            "Config_Transaction_CashrewardsActivationBonusMerchantId"
          ),
          Config__Transaction__CashrewardsLegacyBonusMerchantId: getEnv(
            "Config_Transaction_CashrewardsLegacyBonusMerchantId"
          ),
          Config__Transaction__GiftCardMerchantIds: getEnv(
            "Config_Transaction_GiftCardMerchantIds"
          ),
          Transaction__EventBusName: getEnv("Transaction_EventBusName"),
          Config__CacheConfig__CategoryDataExpiry: getEnv(
            "Config_CacheConfig_CategoryDataExpiry"
          ),
          Config__CacheConfig__OfferDataExpiry: getEnv(
            "Config_CacheConfig_OfferDataExpiry"
          ),
          Config__CacheConfig__CardLinkedMerchantDataExpiry: getEnv(
            "Config_CacheConfig_CardLinkedMerchantDataExpiry"
          ),
          Config__CacheConfig__MerchantDataExpiry: getEnv(
            "Config_CacheConfig_MerchantDataExpiry"
          ),
          Config__CacheConfig__CrApplicationKeyExpiry: getEnv(
            "Config_CacheConfig_CrApplicationKeyExpiry"
          ),
          Config__CacheConfig__EarlyCacheRefreshPercentage: getEnv(
            "Config_CacheConfig_EarlyCacheRefreshPercentage"
          ),
          Config__Promotion__TierTypePromotionId: getEnv(
            "Config_Promotion_TierTypePromotionId"
          ),
          Config__Proxies__Search: getEnv("Config_Proxies_Search"),
          Config__Proxies__AddCard: getEnv("Config_Proxies_AddCard"),
          Config__Proxies__MerchantmapByAuthId: getEnv(
            "Config_Proxies_MerchantmapByAuthId"
          ),
          Config__Proxies__MerchantmapByLocationId: getEnv(
            "Config_Proxies_MerchantmapByLocationId"
          ),
          Config__Talkable__Environment: getEnv("Config_Talkable_Environment"),
          Config__PromoApp__ApiBaseAddress: getEnv(
            "Config_PromoApp_ApiBaseAddress"
          ),
          Config__PromoApp__CouponValidationEndpoint: getEnv(
            "Config_PromoApp_CouponValidationEndpoint"
          ),
          Config__TrueRewards__ApiKey: getEnv("Config_TrueRewards_ApiKey"),
          Config__TrueRewards__App: getEnv("Config_TrueRewards_App"),
          Config__TrueRewards__TokenIssuer: getEnv(
            "Config_TrueRewards_TokenIssuer"
          ),
          Config__Strapi__ApiBaseAddress: getEnv("Config_Strapi_ApiBaseAddress"),
          Config__Strapi__ApiBaseAddressV4: getEnv("Config_Strapi_ApiBaseAddressV4"),
          AWS__Region: getEnv("AWS_REGION"),
          AWS__UserPoolClientId: getEnv("AWS_UserPoolClientId"),
          AWS__UserPoolId: getEnv("AWS_UserPoolId"),
          AWS__MainSiteApiKeyName: getEnv("AWS_MainSiteApiKeyName"),
          AWS__SQS__UpdateLeanplumMemberUrl: getEnv(
            "AWS_SQS_UpdateLeanplumMemberUrl"
          ),
          AWS__SQS__UpdateMemberLeanplumEventUrl: getEnv(
            "AWS_SQS_UpdateMemberLeanplumEventUrl"
          ),
          AWS__SNS__ClickCreateTopicArn: getEnv("AWS_SNS_ClickCreateTopicArn"),
          AWS__SQS__MemberFirstClickEventUrl: getEnv(
            "AWS_SQS_MemberFirstClickEventUrl"
          ),
          FeatureToggleOptions__Premium: getEnv("FeatureToggleOptions_Premium"),
          ShopGoDatabase: getEnv("ShopGoDatabase"),
          ShopGoUserName: getEnv("ShopGoUserName"),
          SQLServerHostWriter: getEnv("SQLServerHostWriter"),
          SQLServerHostReader: getEnv("SQLServerHostReader"),
          FeatureToggleOptions__UnleashConfig__AppName: getEnv("UnleashAPPName"),
          FeatureToggleOptions__UnleashConfig__UnleashApi: getEnv("UnleashAPI"),
          FeatureToggleOptions__UnleashConfig__Environment:
            getEnv("UnleashEnvironment"),
          FeatureToggleOptions__UnleashConfig__FetchTogglesIntervalMin: getEnv(
            "UnleashToggleInterval"
          ),
        },
        secrets: {
          ShopGoPassword: getEnv("ShopGoPassword"),
          Config__Talkable__ApiKey: getEnv("Config_Talkable_ApiKey"),
          AzureAADClientId: getEnv("AzureAADClientId"),
          AzureAADClientSecret: getEnv("AzureAADClientSecret"),
          FeatureToggleOptions__UnleashConfig__UnleashApiKey: getEnv("UnleashAPIKey"),
        },
      },
      
    );
    this.attachPermission();
  }

  attachPermission() {
    this.ecsConstruct.taskRole.addManagedPolicy({
      managedPolicyArn: "arn:aws:iam::aws:policy/AmazonS3ReadOnlyAccess",
    });
    const policyDoc = new PolicyDocument();

    // ssm, kms, dynamodb and sns permission
    policyDoc.addStatements(
      new PolicyStatement({
        actions: [
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
        resources: ["*"],
        effect: Effect.ALLOW,
      })
    );

    // sqs permission
    policyDoc.addStatements(
      new PolicyStatement({
        actions: ["sqs:*"],
        resources: [
          `arn:aws:sqs:${getEnv("AWS_REGION")}:${getEnv(
            "AWS_ACCOUNT_ID"
          )}:${getEnv("UpdateLeanplumMemberQueueName")}`,
          `arn:aws:sqs:${getEnv("AWS_REGION")}:${getEnv(
            "AWS_ACCOUNT_ID"
          )}:${getEnv("EmailCommandQueueName")}`,
        ],
        effect: Effect.ALLOW,
      })
    );
    const policy = new ManagedPolicy(this, getResourceName("managedPolicy"), {
      document: policyDoc,
    });

    policy.attachToRole(this.ecsConstruct.taskRole);
  }
}
