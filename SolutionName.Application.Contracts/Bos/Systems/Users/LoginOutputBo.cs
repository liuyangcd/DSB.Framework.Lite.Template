using SolutionName.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace SolutionName.Application.Contracts.Bos.Systems.Users
{
    public class LoginOutputBo
    {
        /// <summary>
        /// Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// 账户
        /// </summary>
        public required string Account { get; set; }

        /// <summary>
        /// 密码摘要
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// 密码摘要盐
        /// </summary>
        public string? Salt { get; set; }

        /// <summary>
        /// 出生日期
        /// </summary>
        public DateTime? Birth { get; set; }

        /// <summary>
        /// 电子邮箱
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// 电话号码
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// 头像URL
        /// </summary>
        public string? Avatar { get; set; }

        /// <summary>
        /// 用户状态
        /// </summary>
        public RecordStatus Status { get; set; }
    }
}
