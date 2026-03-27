using DSB.Framework.Lite.Data.EFCore.Extensions.SequentialGuid;
using DSB.Framework.Lite.Data.EFCore.Repository;
using Microsoft.Extensions.Logging;
using SolutionName.Domain.Entities.Systems;
using SolutionName.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionName.Application.BackgroundJob.Jobs
{
    /// <summary>
    /// 测试实时后台任务
    /// </summary>
    public class TestJob : BackgroundJobBase<Guid>
    {
        private readonly ILogger<TestJob> logger;
        private readonly IEntityFrameworkCoreRepository<SolutionNameContext, SystemUserEntity, Guid> userRepository;
        private readonly IGuidGenerator guidGenerator;
        private readonly IUnitOfWork<SolutionNameContext> unitOfWork;

        public TestJob(ILogger<TestJob> logger,
                       IEntityFrameworkCoreRepository<SolutionNameContext, SystemUserEntity, Guid> userRepository,
                       IGuidGenerator guidGenerator,
                       IUnitOfWork<SolutionNameContext> unitOfWork)
        {
            this.logger = logger;
            this.userRepository = userRepository;
            this.guidGenerator = guidGenerator;
            this.unitOfWork = unitOfWork;
        }

        public override async Task ExecuteAsync(Guid parameter)
        {
            await Task.Delay(3000); // 模拟延时
            var user = await userRepository.GetAsync(x => x.Id == parameter); 

            //后台任务一定要手动提交工作单元，因为没有工作单元中间件自动提交，或者使用autosave=true提交也行
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("TestJob is executing at {Time}", DateTimeOffset.Now);
        }
    }
}
