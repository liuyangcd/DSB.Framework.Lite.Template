using DSB.Framework.Lite.WebApi.Extensions.Http;
using DSB.Framework.Lite.WebApi.Extensions.Http.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolutionName.Application.Contracts.UserContext;

namespace SolutionName.HttpApi.Host.Controllers
{
    /// <summary>
    /// 基于系统默认的匿名控制器基类
    /// </summary>
    [Route("api/[controller]/[action]")]
    [AllowAnonymous]
    public class AnonymousControllerBase : ControllerBase
    {

    }
}