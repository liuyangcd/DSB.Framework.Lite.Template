using DSB.Framework.Lite.Data.EFCore.Repository;
using SolutionName.Domain.Entities.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionName.EntityFrameworkCore.IRepositories.Systems
{
    /// <summary>
    /// 权限仓储
    /// </summary>
    public interface IPermissionRepository : IEntityFrameworkCoreRepository<SolutionNameContext, SystemPermissionEntity, Guid>
    {

    }
}
