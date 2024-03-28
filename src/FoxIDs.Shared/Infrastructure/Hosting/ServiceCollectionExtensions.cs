using FoxIDs.Logic;
using FoxIDs.Repository;
using ITfoxtec.Identity.Discovery;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Npgsql.DocumentDB;
using StackExchange.Redis;
using System;
using System.Net.Http;

namespace FoxIDs.Infrastructure.Hosting
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSharedLogic(this IServiceCollection services)
        {
            services.AddTransient<PlanUsageLogic>();

            services.AddTransient<ExternalSecretLogic>();
            services.AddTransient<ExternalKeyLogic>();

            services.AddTransient<ClaimTransformValidationLogic>();

            services.AddNpgsqlDocumentDB("Host=localhost;Username=postgres;Password=postgres;Database=foxids_master", null, ServiceLifetime.Singleton, "master");
            services.AddNpgsqlDocumentDB("Host=localhost;Username=postgres;Password=postgres;Database=foxids_cache", null, ServiceLifetime.Singleton, "cache");
            services.AddTransient<IDistributedCacheProvider, PgCacheProvider>();
            services.AddTransient<PlanCacheLogic>();
            services.AddTransient<TenantCacheLogic>();
            services.AddTransient<TrackCacheLogic>();
            services.AddTransient<DownPartyCacheLogic>();
            services.AddTransient<UpPartyCacheLogic>();

            return services;
        }

        public static IServiceCollection AddSharedRepository(this IServiceCollection services)
        {            
            services.AddSingleton<IMasterRepository, PgMasterRepository>();
            services.AddSingleton<ITenantRepository, PgTenantRepository>();

            return services;
        }

        public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services, Models.Config.Settings settings)
        {
            IdentityModelEventSource.ShowPII = true;

            services.AddHsts(options =>
            {
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(365);
            });

            services.AddCors();

            services.AddSingleton<TelemetryLogger>();
            services.AddSingleton<TelemetryScopedStreamLogger>();
            services.AddScoped<TelemetryScopedLogger>();
            services.AddScoped<TelemetryScopedProperties>();

            services.AddHttpContextAccessor();
            services.AddHttpClient(nameof(HttpClient), options => 
            { 
                options.MaxResponseContentBufferSize = 500000; // 500kB 
                options.Timeout = TimeSpan.FromSeconds(30);
            });

            services.AddSingleton<OidcDiscoveryHandlerService>();
            services.AddHostedService<OidcDiscoveryBackgroundService>();

            return services;
        }
    }
}
