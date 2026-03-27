using SolutionName.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionName.Application.Contracts.Dtos.Systems.Users
{
    public class CreateInputDto
    {
        /// <summary>
        /// 姓名
        /// </summary>
        [StringLength(50, ErrorMessage = "姓名长度不超过50个字符")]
        public required string Name { get; set; }

        /// <summary>
        /// 账户
        /// </summary>
        [StringLength(50, ErrorMessage = "账户长度不超过50个字符")]
        public required string Account { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [StringLength(50, ErrorMessage = "密码长度不超过50个字符")]
        public string? PasswordText { get; set; }

        /// <summary>
        /// 出生日期
        /// </summary>
        public DateTime? Birth { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [StringLength(100, ErrorMessage = "邮箱长度不超过100")]
        [EmailAddress(ErrorMessage = "邮箱格式不正确")]
        public string? Email { get; set; }

        /// <summary>
        /// 电话号码
        /// </summary>
        [StringLength(16, ErrorMessage = "电话号码长度不超过16")]
        public string? Phone { get; set; }

        /// <summary>
        /// 头像URL
        /// </summary>
        [StringLength(200, ErrorMessage = "头像URL长度不超过200")]
        public string? Avatar { get; set; }

        /// <summary>
        /// 用户状态
        /// </summary>
        public RecordStatus Status { get; set; } = RecordStatus.Normally;

        /// <summary>
        /// 备注信息
        /// </summary>
        [StringLength(2000, ErrorMessage = "备注信息长度不超过2000")]
        public string? Remark { get; set; }

        /// <summary>
        /// 角色Id集合
        /// </summary>
        public List<Guid>? RoleIds { get; set; }
    }
}
