using DSB.Framework.Lite.Core.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSB.Framework.Lite.Core;

namespace SolutionName.Application.Contracts.UserContext
{
    /// <summary>
    /// 用户上下文实体
    /// </summary>
    public class JwtUserContext : IUserContext<Guid>
    {
        /// <summary>
        /// Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        [Required(ErrorMessage = "姓名不能为空")]
        public required string Name { get; set; }

        /// <summary>
        /// 账户
        /// </summary>
        [StringLength(50)]
        public required string Account { get; set; }

        /// <summary>
        /// 角色集合
        /// </summary>
        public List<string> Roles { get; set; }

        /// <summary>
        /// 出生日期
        /// </summary>
        public DateTime? Birth { get; set; }

        /// <summary>
        /// 电子邮箱
        /// </summary>
        [StringLength(100)]
        public string? Email { get; set; }

        /// <summary>
        /// 电话号码
        /// </summary>
        [StringLength(16)]
        public string? Phone { get; set; }

        /// <summary>
        /// 头像URL
        /// </summary>
        [StringLength(200)]
        public string? Avatar { get; set; }
    }
}