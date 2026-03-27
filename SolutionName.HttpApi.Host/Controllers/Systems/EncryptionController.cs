using DSB.Framework.Lite.WebApi.EncryptionApi;
using DSB.Framework.Lite.WebApi.Extensions.Http.JwtBearer;
using DSB.Framework.Lite.WebApi.Extensions.SwaggerConfig.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SolutionName.HttpApi.Host.Controllers.Systems
{
    public class EncryptionApiTestDto
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// 加密接口示例
    /// </summary>
    [ApiExplorer(ModuleEnum.System)]
    public class EncryptionController : AnonymousControllerBase
    {
        /// <summary>
        /// 加密请求接口测试
        /// </summary>
        /// <param name="inputDto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<EncryptionApiResult<EncryptionApiTestDto>> Test(EncryptionApiInput<EncryptionApiTestDto> inputDto)
        {
            return EncryptionApiResult<EncryptionApiTestDto>.GetSuccess(inputDto.Data, inputDto.ApiOption);
        }
    }
}
