using SolutionName.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionName.Application.Contracts.Dtos.Systems.Roles
{
    public class GetPageListOutputDto
    {
        /// <summary>
        /// 角色Id
        /// </summary>
        public Guid Id { get; set; }

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
        /// 角色状态
        /// </summary>
        public RecordStatus Status { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreateDateAt { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }
    }
}
