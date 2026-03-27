using DSB.Framework.Lite.WebApi.Extensions.Http;
using DSB.Framework.Lite.WebApi.Extensions.Http.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using SolutionName.Application.Contracts.UserContext;

namespace SolutionName.HttpApi.Host.Controllers
{
    /// <summary>
    /// 基于默认管理后台用户的控制器基类
    /// </summary>
    [Route("api/[controller]/[action]")]
    public class ManagerControllerBase : JwtControllerBase<JwtUserContext, Guid>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="jwtService"></param>
        public ManagerControllerBase(IJwtService jwtService) : base(jwtService)
        {
            
        }
    }
}