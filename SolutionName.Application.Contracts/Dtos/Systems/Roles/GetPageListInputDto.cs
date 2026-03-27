using DSB.Framework.Lite.Data.EFCore.Repository.Abstractions;
using SolutionName.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionName.Application.Contracts.Dtos.Systems.Roles
{
    public class GetPageListInputDto : PagedQueryBase
    {
        /// <summary>
        /// 编码
        /// </summary>
        [StringLength(100, ErrorMessage = "角色编码长度不超过100")]
        public string? Code { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [StringLength(200, ErrorMessage = "角色名称长度不超过200")]
        public string? Name { get; set; }

        /// <summary>
        /// 角色状态
        /// </summary>
        public RecordStatus? Status { get; set; }
    }
}
