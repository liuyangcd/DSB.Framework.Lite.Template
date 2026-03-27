using DSB.Framework.Lite.Data.EFCore.Extensions.SequentialGuid;
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
    /// 权限仓储
    /// </summary>
    public class PermissionRepository(IUnitOfWork<SolutionNameContext> unitOfWork) : EntityFrameworkCoreRepository<SolutionNameContext, SystemPermissionEntity, Guid>(unitOfWork)
    {

    }
}
