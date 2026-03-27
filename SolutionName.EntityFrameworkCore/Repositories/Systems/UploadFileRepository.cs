using DSB.Framework.Lite.Data.EFCore.Repository;
using SolutionName.Domain.Entities.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionName.EntityFrameworkCore.Repositories.Systems
{
    /// <summary>
    /// 上传文件管理仓储
    /// </summary>
    public class UploadFileRepository(IUnitOfWork<SolutionNameContext> unitOfWork) : EntityFrameworkCoreRepository<SolutionNameContext, SystemUploadFileEntity, Guid>(unitOfWork)
    {

    }
}
