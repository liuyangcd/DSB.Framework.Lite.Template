using DSB.Framework.Lite.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionName.Application.Contracts.Dtos.Systems.Users
{
    public class UpdateInputDto : CreateInputDto
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        [GuidRequired(ErrorMessage = "用户Id不能为空")]
        public Guid Id { get; set; }
    }
}
