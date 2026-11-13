using DSB.Framework.Lite.Data.EFCore.Repository;
using SolutionName.Domain.Entities.Systems;

namespace SolutionName.EntityFrameworkCore.IRepositories.Systems
{
    /// <summary>
    /// 上传文件管理仓储
    /// </summary>
    public interface IUploadFileRepository : IEntityFrameworkCoreRepository<SolutionNameContext, SystemUploadFileEntity, Guid>
    {

    }
}
