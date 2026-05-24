using DSB.Framework.Lite.Data.EFCore.Extensions;
using DSB.Framework.Lite.Data.EFCore.Extensions.Setting;
using DSB.Framework.Lite.Data.EFCore.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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

            services.AddEntityFrameworkCoreDbContext<SolutionNameContext>(efDbContextOptions, 1024, true, optionsBuilder =>
            {
#if DEBUG
                optionsBuilder.LogTo(Console.WriteLine, new[] { Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.CommandExecuting }).EnableSensitiveDataLogging();
#endif
            });
            services.AddGuidGenerator(efDbContextOptions.DbType);
            services.AddEntityFrameworkCoreRepository(typeof(SolutionNameContext));

            #endregion
        }
    }
}