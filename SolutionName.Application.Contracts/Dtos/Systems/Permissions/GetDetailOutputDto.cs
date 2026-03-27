using SolutionName.Domain.Enums;
using SolutionName.Domain.Enums.Systems;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionName.Application.Contracts.Dtos.Systems.Permissions
{
    public class GetDetailOutputDto
    {
        /// <summary>
        /// 权限Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 父级Id
        /// </summary>
        public Guid? ParentId { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        [StringLength(100, ErrorMessage = "权限编码长度不超过100")]
        [Required(ErrorMessage = "权限编码不能为空")]
        public required string Code { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [StringLength(200, ErrorMessage = "权限名称长度不超过200")]
        [Required(ErrorMessage = "权限名称不能为空")]
        public required string Name { get; set; }

        /// <summary>
        /// 权限类型
        /// </summary>
        public PermissionType Type { get; set; }

        /// <summary>
        /// 请求路径/菜单路径
        /// </summary>
        [StringLength(200, ErrorMessage = "路径长度不超过200")]
        public string? Url { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        [StringLength(200, ErrorMessage = "图标长度不超过200")]
        public string? Icon { get; set; }

        /// <summary>
        /// 权限状态
        /// </summary>
        public RecordStatus Status { get; set; } = RecordStatus.Normally;

        /// <summary>
        /// 备注信息
        /// </summary>
        [StringLength(2000, ErrorMessage = "备注信息长度不超过2000")]
        public string? Remark { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreateDateAt { get; set; }

        /// <summary>
        /// 更新日期
        /// </summary>
        public DateTime? UpdateDateAt { get; set; }
    }
}
