using Amazon.DynamoDBv2;
using Amazon.EventBridge;
using Amazon.SimpleNotificationService;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Configuration;
using Cashrewards3API.Common.Context;
using Cashrewards3API.Common.Dto;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Common.Services.Interfaces;
using Cashrewards3API.Common.Utils;
using Cashrewards3API.Features.Banners.Interface;
using Cashrewards3API.Features.Banners.Service;
using Cashrewards3API.Features.CardLinkedMerchant;
using Cashrewards3API.Features.Category;
using Cashrewards3API.Features.Category.Interface;
using Cashrewards3API.Features.Feeds.Service;
using Cashrewards3API.Features.GiftCard.Interface;
using Cashrewards3API.Features.GiftCard.Service;
using Cashrewards3API.Features.Member.Interface;
using Cashrewards3API.Features.Member.Repository;
using Cashrewards3API.Features.Member.Service;
using Cashrewards3API.Features.MemberClick;
using Cashrewards3API.Features.Merchant;
using Cashrewards3API.Features.Merchant.Repository;
using Cashrewards3API.Features.Offers;
using Cashrewards3API.Features.Person.Interface;
using Cashrewards3API.Features.Person.Service;
using Cashrewards3API.Features.Promotion;
using Cashrewards3API.Features.Proxies;
using Cashrewards3API.Features.ReferAFriend;
using Cashrewards3API.Features.ShopGoClient;
using Cashrewards3API.Features.ShopGoNetwork.Repository;
using Cashrewards3API.Features.ShopGoNetwork.Service;
using Cashrewards3API.Features.Transaction;
using Cashrewards3API.FeaturesToggle;
using Cashrewards3API.Internals.BonusTransaction;
using Cashrewards3API.Middlewares;
using Cashrewards3API.Options;
using Cashrewards3API.Security;
using Common.Health;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.AlwaysEncrypted.AzureKeyVaultProvider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Unleash;
using Unleash.ClientFactory;

namespace Cashrewards3API
{
    public class Startup
    {
        private static ClientCredential _clientCredential;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private static async Task<string> GetToken(string authority, string resource, string scope)
        {
            var authContext = new AuthenticationContext(authority);
            AuthenticationResult result = await authContext.AcquireTokenAsync(resource, _clientCredential);

            if (result == null)
                throw new InvalidOperationException("Failed to obtain the access token");
            return result.AccessToken;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            _clientCredential = new ClientCredential(Configuration["AzureAADClientId"], Configuration["AzureAADClientSecret"]);

            SqlColumnEncryptionAzureKeyVaultProvider azureKeyVaultProvider =
              new SqlColumnEncryptionAzureKeyVaultProvider(GetToken);

            Dictionary<string, SqlColumnEncryptionKeyStoreProvider> providers =
              new Dictionary<string, SqlColumnEncryptionKeyStoreProvider>
              {
                  { SqlColumnEncryptionAzureKeyVaultProvider.ProviderName, azureKeyVaultProvider }
              };
            SqlConnection.RegisterColumnEncryptionKeyStoreProviders(providers);

            services.AddControllers(options => { options.RespectBrowserAcceptHeader = true; })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new RequestContractResolver(new HttpContextAccessor());
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                })
                .AddJsonOptions(options =>
                {
                    // Use the default property (Pascal) casing.
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            services.AddMemoryCache();
            services.AddHttpClient("search", c => { c.BaseAddress = new Uri(Configuration["Config:Proxies:Search"]); });
            services.AddHttpClient("addcard",
                c => { c.BaseAddress = new Uri(Configuration["Config:Proxies:AddCard"]); });

            services.AddHttpClient("merchantmap-authid",
                c => { c.BaseAddress = new Uri(Configuration["Config:Proxies:MerchantmapByAuthId"]); });
            services.AddHttpClient("merchantmap-locationid",
                c => { c.BaseAddress = new Uri(Configuration["Config:Proxies:MerchantmapByLocationId"]); });

            CommonConfig config = new();
            Configuration.GetSection("Config").Bind(config);

            services.AddHttpClient("talkable", c =>
            {
                c.BaseAddress = new Uri(config.Talkable.ApiBaseAddress);
            });

            services.AddHttpClient("promoapp", c => { c.BaseAddress = new Uri(Configuration["Config:PromoApp:ApiBaseAddress"]); });

            services.AddHttpClient("truerewards", c =>
            {
                c.BaseAddress = new Uri(config.TrueRewards.TokenIssuer);
            });

            services.AddHttpClient("strapi", c =>
            {
                c.BaseAddress = new Uri(config.Strapi.ApiBaseAddress);
            });

            services.AddHttpClient("strapiv4", c =>
            {
                c.BaseAddress = new Uri(config.Strapi.ApiBaseAddressV4);
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // aws
            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
            services.AddAWSService<IAmazonDynamoDB>();
            services.AddAWSService<IAmazonEventBridge>();
            services.AddAWSService<IAmazonSimpleNotificationService>();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddSingleton<IAuthorizationHandler, CrApplicationKeyValidationHandler>();
            services.AddTransient<IAuthorizationHandler, AccessTokenAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, ClientCredentialsTokenAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, AccessTokenPresentAuthorizationHandler>();

            services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer();

            services.AddHttpContextAccessor();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("InternalPolicy",
                    policy => policy.Requirements.Add(new CrApplicationKeyValidationRequirement()));

                options.AddPolicy("CR-AccessToken",
                    policy => policy.Requirements.Add(new AccessTokenRequirement(Constants.Clients.CashRewards)));

                options.AddPolicy("CR-ClientCredentials",
                   policy => policy.Requirements.Add(new ClientCredentialsTokenRequirement(Constants.Clients.CashRewards)));

                options.AddPolicy(Constants.PolicyNames.AllowAnonymousOrToken
                    , policy => policy.Requirements.Add(new AccessTokenPresentRequirement()));
            });

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Cashrewards 3.0 API",
                    Version = "v1",
                    Description = "Web API for cashrewards 3.0"
                });
                //options.OperationFilter<AddCustomHeaderParameter>();
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                     {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                        }
                });
            });

            services.ConfigureSwaggerGen(options => { options.CustomSchemaIds(x => x.FullName); });

            // services.AddHealthChecks();
            services.AddHealthChecks()
                .AddCheck<DbHealthCheck>(
                    "database_health_check",
                    failureStatus: HealthStatus.Degraded,
                    tags: new[] { "db" });

            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
            services.AddAWSService<IAmazonDynamoDB>();

            //app config
            var dbConfig = new DbConfig();
            Configuration.GetSection("ConnectionStrings").Bind(dbConfig);
            services.AddSingleton<DbConfig>(dbConfig);
            var commonConfig = new CommonConfig();
            Configuration.GetSection("Config").Bind(commonConfig);
            services.AddSingleton<CommonConfig>(commonConfig);
            var cacheConfig = new CacheConfig();
            Configuration.GetSection("Config:CacheConfig").Bind(cacheConfig);
            services.AddSingleton<CacheConfig>(cacheConfig);
            var redis = new RedisConnectionFactory(Configuration["Config:RedisMasters"]);

            services.AddSingleton(_ =>
            {
                AWSInfrastructureSettings instance = new AWSInfrastructureSettings();
                Configuration.GetSection("AWS").Bind(instance);
                return instance;
            });

            services.AddSingleton<IIdGeneratorService, Base62MemberClientUniqueGeneratorService>();
            services.AddSingleton<IIdGeneratorService, Base62UniqueGeneratorService>();
            services.AddSingleton<IIdGeneratorService, BupaUniqueGeneratorService>();
            services.AddSingleton<ICrApplicationKeyValidationService, CrApplicationKeyValidationService>();
            services.AddSingleton<IdGeneratorFactory>();
            services.AddSingleton<TrackingLinkGenerator>();
            services.AddSingleton<ShopgoDBContext>();
            services.AddSingleton<IDatabase>(s =>
            {
                var conn = redis.GetConnection();
                if (conn != null && conn.IsConnected)
                    return conn.GetDatabase();

                return null;
            });
            services.AddSingleton<IRedisSemaphore, RedisSemaphore>((IServiceProvider serviceProvider) =>
            {
                var semaphore = new RedisSemaphore();
                semaphore.StartHealthChecks(new CancellationTokenSource().Token);
                return semaphore;
            });

            // Repos
            services.AddScoped<IRequestContext, RequestContext>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IOfferService, OfferService>();
            services.AddScoped<IMerchantService, MerchantService>();
            services.AddScoped<ITrendingMerchantService, TrendingMerchantService>();
            services.AddScoped<IPopularMerchantService, PopularMerchantService>();
            services.AddScoped<IMerchantMappingService, MerchantMappingService>();
            services.AddScoped<ICardLinkedMerchantService, CardLinkedMerchantService>();
            services.AddScoped<IMemberClickService, MemberClickService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IWoolworthsEncryptionProvider, WoolworthsEncryptionProvider>();
            services.AddScoped<IMemberService, MemberService>();
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<IMerchantBundleService, MerchantBundleService>();
            services.AddScoped<IMemberTransactionService, MemberTransactionService>();
            services.AddScoped<IMemberClickHistoryService, MemberClickHistoryService>();
            services.AddScoped<ICacheKey, CacheKey>();
            services.AddScoped<IRedisUtil, RedisUtil>();
            services.AddScoped<ISearchService, SearchService>();
            services.AddScoped<IAddCardService, AddCardService>();
            services.AddScoped<IProxyApiService, ProxyApiService>();
            services.AddScoped<INetworkExtension, NetworkExtension>();
            services.AddScoped<IBonusTransactionService, BonusTransactionService>();
            services.AddScoped<IMerchantInternalService, MerchantInternalService>();
            services.AddScoped<IRafService, RafService>();
            services.AddScoped<ISaleAdjustmentTransactionService, SaleAdjustmentTransactionService>();
            services.AddScoped<IShopGoClientService, ShopGoClientService>();
            services.AddScoped<IMemberRegistrationService, MemberRegistrationService>();
            services.AddScoped<IPerson, PersonService>();
            services.AddScoped<IEncryption, SHACryptor>();
            services.AddTransient<IDateTimeProvider, MachineDateTime>();
            services.AddScoped<IMessage, SQSService>();
            services.AddScoped<IPremiumService, PremiumService>();
            services.AddScoped<IBanner, BannerService>();
            services.AddScoped<IGiftCard, GiftCardService>();
            services.AddScoped<ITalkableService, TalkableService>();
            services.AddScoped<ITokenService, TrueRewardsService>();
            services.AddScoped<IAwsS3Service, AwsS3Service>();
            services.AddScoped<IReadOnlyRepository, ReadOnlyRepository>();
            services.AddScoped<IRepository, Repository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IMerchantRepository, MerchantRepository>();
            services.AddScoped<IPopularMerchantRepository, PopularMerchantRepository>();
            services.AddScoped<ITrendingMerchantRepository, TrendingMerchantRepository>();
            services.AddScoped<IPromotionService, PromotionService>();
            services.AddScoped<IPromotionCacheService, PromotionCacheService>();
            services.AddScoped<IPromotionDefinitionService, PromotionDefinitionService>();
            services.AddScoped<IMemberBonusService, MemberBonusService>();
            services.AddScoped<IPromoAppService, PromoAppService>();
            services.AddScoped<IFeatureToggle, FeatureToggleService>();
            services.AddScoped<IMemberRepository, MemberRepository>();
            services.AddScoped<ITokenValidation, TokenValidationService>();
            services.AddScoped<IPausedMerchantFeatureToggle, PausedMerchantFeatureToggle>();
            services.AddScoped<INetworkService, NetworkService>();
            services.AddScoped<INetworkRepository, NetworkRepository>();
            services.AddScoped<IMerchantFeedService, MerchantFeedService>();
            services.AddScoped<IStrapiService, StrapiService>();

            services.AddMvc()
                .AddFluentValidation(fv =>
                    fv.RegisterValidatorsFromAssemblyContaining<CreateBonusTransactionRequestModelValidator>());

            services.Configure<FeatureToggleOptions>(Configuration.GetSection($"{typeof(FeatureToggleOptions).Name}"));

            AddOpenTelementry(services);

            ConfigFeatureToggle(services);
        }

        public void AddOpenTelementry(IServiceCollection services)
        {
            services.AddOpenTelemetryTracing(b =>
            {
                b
                .AddSource("CR3Api")
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                        .AddService(serviceName: "CR3Api", serviceVersion: GetAssemblyVersion())
                        .AddTelemetrySdk())
                .AddXRayTraceId()
                .AddAWSInstrumentation()
                .AddOtlpExporter(options =>
                {
                    options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    options.Endpoint = new Uri("http://localhost:4317");
                })
                .AddSqlClientInstrumentation(options =>
                {
                    options.SetDbStatementForStoredProcedure = true;
                    options.SetDbStatementForText = true;
                })
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation();
            });
        }

        private static string GetAssemblyVersion()
        {
            return Assembly.GetEntryAssembly().GetName().Version.ToString();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Handle all exceptions and logging
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cashrewards 3.0 API");
                c.RoutePrefix = string.Empty;
            });

            app.UseMiddleware<CorrelationMiddleware>();
            // Logging
            app.UseSerilogRequestLogging();

            // Health checks
            app.UseHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var response = new HealthCheckReponse
                    {
                        Status = report.Status.ToString(),
                        HealthChecks = report.Entries.Select(x => new IndividualHealthCheckResponse
                        {
                            Component = x.Key,
                            Status = x.Value.Status.ToString(),
                            Description = x.Value.Description
                        }),
                        HealthCheckDuration = report.TotalDuration
                    };
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
                }
            });

            app.UseAuthentication();
            app.UseRouting();

            app.UseAuthorization();

            app.UseCors(options => options.SetIsOriginAllowed(origin => origin.EndsWith("cashrewards.com.au")).AllowAnyMethod().AllowAnyHeader());

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }

        private void  ConfigFeatureToggle(IServiceCollection services)
        {

            services.AddSingleton<IUnleash>(provider =>
            {
                var config =provider.GetService<IOptions<FeatureToggleOptions>>().Value?.UnleashConfig;
                var settings = new UnleashSettings()
                {
                    AppName = config.AppName,
                    UnleashApi = new Uri(config.UnleashApi),
                    Environment = config.Environment,
                    FetchTogglesInterval = TimeSpan.FromMinutes(config.FetchTogglesIntervalMin),
                    CustomHttpHeaders = new Dictionary<string, string>()
                    {
                        ["Authorization"] = config.UnleashApiKey
                    },
                    SendMetricsInterval = TimeSpan.FromSeconds(30)
                };

                var unleashFactory = new UnleashClientFactory();
                IUnleash unleash = unleashFactory.CreateClient(settings, synchronousInitialization: true);
                return unleash;
            });
           
        }
    }
}