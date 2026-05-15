using DSB.Framework.Lite.Data.EFCore.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SolutionName.Domain;
using SolutionName.EntityFrameworkCore;

namespace SolutionName.Application
{
    /// <summary>
    /// 数据种子托管服务，在应用启动时通过 UnitOfWorkProvider 创建作用域执行所有 IDataSeedContributor
    /// </summary>
    public class DataSeedHostedService(IUnitOfWorkProvider<SolutionNameContext> unitOfWorkProvider) : IHostedService
    {
        /// <summary>
        /// 启动时执行种子数据初始化
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var uowScope = unitOfWorkProvider.CreateScope();
            var contributors = uowScope.ServiceScope.ServiceProvider.GetServices<IDataSeedContributor>();
            foreach (var contributor in contributors)
            {
                await contributor.SeedAsync();
            }
            await uowScope.Current.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// 停止时无需操作
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
