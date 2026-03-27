using DSB.Framework.Lite.Core.Attributes;
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
    public class LoginInputDto
    {
        /// <summary>
        /// 账户
        /// </summary>
        [Required(ErrorMessage = "账户不能为空")]
        [StringLength(50, ErrorMessage = "账户长度不超过50个字符")]
        public required string Account { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Required(ErrorMessage = "密码不能为空")]
        [StringLength(50, ErrorMessage = "密码长度不超过50个字符")]
        public required string Password { get; set; }

        /// <summary>
        /// 验证码Id
        /// </summary>
        [GuidRequired(ErrorMessage = "请获取验证码")]
        public Guid CaptchaId { get; set; }

        /// <summary>
        /// 验证码
        /// </summary>
        [Required(ErrorMessage = "请输入验证码")]
        [StringLength(50, ErrorMessage = "验证码长度不超过50个字符")]
        public required string CaptchaCode { get; set; }
    }
}
