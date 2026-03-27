using DSB.Framework.Lite.Data.EFCore.Repository.Abstractions.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionName.Domain.Entities.Systems
{
    /// <summary>
    /// 角色权限关系
    /// </summary>
    public class SystemRolePermissionEntity : EntityBase<Guid>
    {
        /// <summary>
        /// 角色Id
        /// </summary>
        public Guid RoleId { get; set; }

        /// <summary>
        /// 权限Id
        /// </summary>
        public Guid PermissionId { get; set; }
    }
}
