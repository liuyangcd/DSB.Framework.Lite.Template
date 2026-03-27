using DSB.Framework.Lite.Application.HttpClient.Binder;
using DSB.Framework.Lite.Application.PBKDF2Password;
using DSB.Framework.Lite.Caching.Extensions;
using DSB.Framework.Lite.Service.Extensions;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SolutionName.Application.BackgroundJob.Jobs;
using SolutionName.Domain.Options;

namespace SolutionName.Application
{
    /// <summary>
    /// 应用模块扩展
    /// </summary>
    public static class SolutionNameApplicationModule
    {
        /// <summary>
        /// 注册应用服务模块
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddApplicationModule(this IServiceCollection services, IConfiguration configuration)
        {
            #region 缓存

            var redisConnectionString = configuration.GetConnectionString("Redis");
            if (!string.IsNullOrEmpty(redisConnectionString))
            {
                services.AddCache(options =>
                {
                    options.Configuration = redisConnectionString;
                    options.InstanceName = "SolutionName:";
                });
                services.AddCSRedis(redisConnectionString);
                services.AddRangeExclusiveLock("SolutionName");
            }

            #endregion

            #region 注册密码管理服务

            services.AddPBKDF2Password();

            #endregion

            #region HttpClient注册

            var httpClientOptions = configuration.GetSection("HttpClientOptions").Get<HttpClientOptions>();
            services.ConfigureHttpClient(httpClientOptions);

            #endregion

            #region 批量自动注册应用服务

            services.AddApplicationService<SolutionNameApplicationService>();

            #endregion
        }

        /// <summary>
        /// 注册自启动周期循环执行的后台任务
        /// </summary>
        /// <param name="serviceProvider"></param>
        public static void UseHangfireBackgroundJobs(this IServiceProvider serviceProvider)
        {
            var recurringJobManager = serviceProvider.GetRequiredService<IRecurringJobManager>();
            recurringJobManager.AddOrUpdate<CycleJob>(nameof(CycleJob), job => job.ExecuteAsync(), Cron.Minutely);
        }
    }
}