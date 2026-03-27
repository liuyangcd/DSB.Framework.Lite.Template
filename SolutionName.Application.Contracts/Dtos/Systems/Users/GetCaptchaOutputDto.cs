using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionName.Application.Contracts.Dtos.Systems.Users
{
    /// <summary>
    /// 
    /// </summary>
    public class GetCaptchaOutputDto
    {
        /// <summary>
        /// 验证码Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 验证码图片Base64字符串
        /// </summary>
        public required string ImageBase64 { get; set; }
    }
}
