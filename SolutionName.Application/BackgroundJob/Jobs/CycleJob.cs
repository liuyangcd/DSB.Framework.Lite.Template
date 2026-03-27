using DSB.Framework.Lite.Data.EFCore.Repository;
using Microsoft.Extensions.Logging;
using SolutionName.Domain.Entities.Systems;
using SolutionName.EntityFrameworkCore;

namespace SolutionName.Application.BackgroundJob.Jobs
{
    /// <summary>
    /// 周期循环执行的后台任务示例
    /// </summary>
    public class CycleJob : BackgroundJobBase
    {
        private readonly ILogger<CycleJob> logger;
        private readonly IUnitOfWork<SolutionNameContext> unitOfWork;
        private readonly IEntityFrameworkCoreRepository<SolutionNameContext, SystemUserEntity, Guid> userRepository;

        public CycleJob(ILogger<CycleJob> logger,
                        IUnitOfWork<SolutionNameContext> unitOfWork,
                        IEntityFrameworkCoreRepository<SolutionNameContext, SystemUserEntity, Guid> userRepository)
        {
            this.logger = logger;
            this.unitOfWork = unitOfWork;
            this.userRepository = userRepository;
        }

        /// <inheritdoc/>
        public override async Task ExecuteAsync()
        {
            var user = await userRepository.GetAsync(null);
            logger.LogInformation("CycleJob is executing at {Time}", DateTimeOffset.Now);

            // 如果有数据库非查询类操作，必须通过单元工作进行提交
            await unitOfWork.SaveChangesAsync();
        }
    }
}
