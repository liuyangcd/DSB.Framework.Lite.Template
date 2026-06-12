using DSB.Framework.Lite.Data.EFCore.Repository;
using Hangfire;
using Microsoft.Extensions.Logging;
using SolutionName.EntityFrameworkCore;
using SolutionName.EntityFrameworkCore.IRepositories.Systems;

namespace SolutionName.Application.BackgroundJobs.Jobs
{
    /// <summary>
    /// 周期循环执行的后台任务示例
    /// </summary>
    public class CycleJob(
        ILogger<CycleJob> logger,
        IUnitOfWork<SolutionNameContext> unitOfWork,
        IUserRepository userRepository) : BackgroundJobBase
    {
        /// <summary>
        /// 周期循环执行的后台任务示例
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        [JobDisplayName("周期循环执行的后台任务示例")]
        public override async Task ExecuteAsync(CancellationToken ct)
        {
            var user = await userRepository.GetAsync(null, cancellationToken: ct);

            // 如果有数据库非查询类操作，必须通过单元工作进行提交
            await unitOfWork.SaveChangesAsync(ct);

            logger.LogInformation("CycleJob is executing at {Time}", DateTimeOffset.Now);
        }
    }
}
