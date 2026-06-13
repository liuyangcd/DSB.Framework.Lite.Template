using DSB.Framework.Lite.Core;
using DSB.Framework.Lite.WebApi.EncryptionApi;
using DSB.Framework.Lite.WebApi.Extensions.SwaggerConfig.Attributes;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using SolutionName.Application.BackgroundJobs.Jobs;

namespace SolutionName.HttpApi.Host.Controllers.Systems
{
    /// <summary>
    /// 
    /// </summary>
    public class EncryptionApiTestDto
    {
        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// 示例控制器
    /// </summary>
    [ApiExplorer(ModuleEnum.System)]
    public class TestController(IBackgroundJobClient backgroundJobClient) : AnonymousControllerBase
    {
        /// <summary>
        /// 加密请求接口示例
        /// </summary>
        /// <param name="inputDto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<EncryptionApiResult<EncryptionApiTestDto>> EncryptionApi([FromBody] EncryptionApiInput<EncryptionApiTestDto> inputDto)
        {
            return EncryptionApiResult<EncryptionApiTestDto>.GetSuccess(inputDto.Data, inputDto.ApiOption);
        }

        /// <summary>
        /// 创建一个后台耗时任务示例
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<string>> CreateBackgroundJob()
        {
            var oneTimeJobId = backgroundJobClient.Enqueue<OneTimeJob>(job => job.ExecuteAsync(Guid.Empty, CancellationToken.None));
            return ApiResult<string>.GetSuccess(oneTimeJobId);
        }

        /// <summary>
        /// 取消后台任务示例
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<string>> CancelBackgroundJob(string jobId)
        {
            backgroundJobClient.Delete(jobId);
            return ApiResult<string>.GetSuccess("Background job cancelled successfully.");
        }
    }
}
