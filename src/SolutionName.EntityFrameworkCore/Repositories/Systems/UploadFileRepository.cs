using DSB.Framework.Lite.Data.EFCore.Repository;
using SolutionName.Domain.Entities.Systems;
using SolutionName.EntityFrameworkCore.IRepositories.Systems;

namespace SolutionName.EntityFrameworkCore.Repositories.Systems
{
    /// <summary>
    /// 上传文件管理仓储
    /// </summary>
    public class UploadFileRepository(IUnitOfWork<SolutionNameContext> unitOfWork) : EntityFrameworkCoreRepository<SolutionNameContext, SystemUploadFileEntity, Guid>(unitOfWork), IUploadFileRepository
    {

    }
}
