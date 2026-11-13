using DSB.Framework.Lite.Data.EFCore.Repository.Abstractions.Entity;
using SolutionName.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionName.Domain.Entities.Systems
{
    /// <summary>
    /// 角色
    /// </summary>
    public class SystemRoleEntity : EntityBase<Guid>
    {
        /// <summary>
        /// 编码
        /// </summary>
        [StringLength(100, ErrorMessage = "角色编码长度不超过100")]
        [Required(ErrorMessage = "角色编码不能为空")]
        public required string Code { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [StringLength(200, ErrorMessage = "角色名称长度不超过200")]
        [Required(ErrorMessage = "角色名称不能为空")]
        public required string Name { get; set; }

        /// <summary>
        /// 备注信息
        /// </summary>
        [StringLength(2000, ErrorMessage = "备注信息长度不超过2000")]
        public string? Remark { get; set; }

        /// <summary>
        /// 角色状态
        /// </summary>
        public RecordStatus Status { get; set; } = RecordStatus.Normally;

        /// <summary>
        /// 创建人Id
        /// </summary>
        public Guid? CreatedUserId { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }
    }
}
