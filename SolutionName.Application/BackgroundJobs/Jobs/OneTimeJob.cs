using DSB.Framework.Lite.Data.EFCore.Repository;
using Hangfire;
using Microsoft.Extensions.Logging;
using SolutionName.EntityFrameworkCore;
using SolutionName.EntityFrameworkCore.IRepositories.Systems;

namespace SolutionName.Application.BackgroundJobs.Jobs
{
    /// <summary>
    /// 测试执行一次的后台任务
    /// </summary>
    public class OneTimeJob(
        ILogger<OneTimeJob> logger,
        IUserRepository userRepository,
        IUnitOfWork<SolutionNameContext> unitOfWork) : BackgroundJobBase<Guid>
    {
        /// <summary>
        /// 测试一次性任务 - 演示任务取消处理
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [JobDisplayName("测试一次性任务 - 演示任务取消处理")]
        public override async Task ExecuteAsync(Guid parameter, CancellationToken ct)
        {
            logger.LogInformation("OneTimeJob is starting at {Time}", DateTimeOffset.Now);

            try
            {
                // 模拟一个耗时的操作，持续30秒，每秒检查一次是否取消
                for (var i = 0; i < 30; i++)
                {
                    // 这种方式在取消时会抛出异常
                    // await Task.Delay(1000, ct);

                    // 这是手动检查取消的方式
                    await Task.Delay(1000);
                    ct.ThrowIfCancellationRequested();

                    logger.LogInformation("OneTimeJob is running... {Seconds} seconds elapsed", i + 1);
                }

                var user = await userRepository.GetAsync(x => x.Id == parameter, cancellationToken: ct);

                //后台任务一定要手动提交工作单元，因为没有工作单元中间件自动提交，或者使用autosave=true提交也行
                await unitOfWork.SaveChangesAsync(ct);

                logger.LogInformation("OneTimeJob is executed at {Time}", DateTimeOffset.Now);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("OneTimeJob was cancelled during execution at {Time}", DateTimeOffset.Now);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "OneTimeJob encountered an error at {Time}", DateTimeOffset.Now);
            }
        }
    }
}
