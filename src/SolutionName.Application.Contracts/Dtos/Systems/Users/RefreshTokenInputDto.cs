using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionName.Application.Contracts.Dtos.Systems.Users
{
    public class RefreshTokenInputDto
    {
        /// <summary>
        /// 刷新Token
        /// </summary>
        [Required(ErrorMessage = "刷新Token不能为空")]
        public string? RefreshToken { get; set; }
    }
}
