using DSB.Framework.Lite.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionName.Application.Contracts.Dtos.Systems.Roles
{
    public class UpdateInputDto : CreateInputDto
    {
        /// <summary>
        /// 角色Id
        /// </summary>
        [GuidRequired(ErrorMessage = "角色Id不能为空")]
        public Guid Id { get; set; }
    }
}
