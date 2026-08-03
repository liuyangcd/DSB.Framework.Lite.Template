using DSB.Framework.Lite.WebApi.ApiKeyAuthorization;
using Microsoft.AspNetCore.Mvc;

namespace SolutionName.HttpApi.Host.Controllers
{
    /// <summary>
    /// 基于ApiKey验证的控制器基类
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiKeyAuthorization]
    public class ApiKeyControllerBase : ControllerBase
    {
        /// <summary>
        /// 读取ClaimTypes.Name
        /// </summary>
        protected string? ApiKeyName => User?.Identity?.Name;
    }
}
