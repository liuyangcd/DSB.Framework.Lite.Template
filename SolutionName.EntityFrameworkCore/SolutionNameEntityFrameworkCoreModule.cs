using DSB.Framework.Lite.Data.EFCore.Extensions;
using DSB.Framework.Lite.Data.EFCore.Extensions.Setting;
using DSB.Framework.Lite.Data.EFCore.Repository;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SolutionName.EntityFrameworkCore
{
    /// <summary>
    /// 数据库模块扩展
    /// </summary>
    public static class SolutionNameEntityFrameworkCoreModule
    {
        /// <summary>
        /// 注册EFCore服务模块
        /// </summary>
        /// <param name="services"></param>
        /// <param name="efDbContextOptions">efcore配置对象</param>
        public static void AddEntityFrameworkCoreModule(this IServiceCollection services, EntityFrameworkCoreDbContextOptions efDbContextOptions)
        {
            #region EntityFrameworkCore数据库

            // 注册EF Core数据库上下文，并配置相关选项：配置全局无跟踪查询模式，并在调试模式下输出SQL日志和启用敏感数据日志记录
            services.AddEntityFrameworkCoreDbContext<SolutionNameContext>(efDbContextOptions, false, optionsBuilder =>
            {
#if DEBUG
                optionsBuilder.LogTo(Console.WriteLine, [RelationalEventId.CommandExecuted], LogLevel.Information)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
#endif
            });
            services.AddGuidGenerator(efDbContextOptions.DbType);
            services.AddEntityFrameworkCoreRepository(typeof(SolutionNameContext));

            #endregion
        }
    }
}