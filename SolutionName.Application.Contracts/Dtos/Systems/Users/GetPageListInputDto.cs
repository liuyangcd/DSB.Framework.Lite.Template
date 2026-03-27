using DSB.Framework.Lite.Data.EFCore.Repository.Abstractions;
using SolutionName.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionName.Application.Contracts.Dtos.Systems.Users
{
    /// <summary>
    /// 
    /// </summary>
    public class GetPageListInputDto : PagedQueryBase
    {
        /// <summary>
        /// 姓名
        /// </summary>
        [StringLength(50, ErrorMessage = "姓名长度不超过50个字符")]
        public string? Name { get; set; }

        /// <summary>
        /// 账户
        /// </summary>
        [StringLength(50, ErrorMessage = "账户长度不超过50个字符")]
        public string? Account { get; set; }

        /// <summary>
        /// 电话号码
        /// </summary>
        [StringLength(16, ErrorMessage = "电话号码长度不超过16")]
        public string? Phone { get; set; }

        /// <summary>
        /// 用户状态
        /// </summary>
        public RecordStatus? Status { get; set; }
    }
}
