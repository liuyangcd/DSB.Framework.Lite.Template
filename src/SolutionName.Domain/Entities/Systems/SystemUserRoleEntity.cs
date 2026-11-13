using DSB.Framework.Lite.Data.EFCore.Repository.Abstractions.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionName.Domain.Entities.Systems
{
    /// <summary>
    /// 用户角色关系
    /// </summary>
    public class SystemUserRoleEntity : EntityBase<Guid>
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 角色Id
        /// </summary>
        public Guid RoleId { get; set; }
    }
}
