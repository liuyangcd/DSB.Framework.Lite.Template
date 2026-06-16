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

        /// <summary>
        /// 客户端取消请求示例
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<bool>> ClientCancelRequest(CancellationToken cancellationToken)
        {
            for (int i = 0; i < 30; i++)
            {
                // 每轮循环检查一次客户端是否断开
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(1000); // 模拟一些工作
            }
            return ApiResult<bool>.GetSuccess(true);
        }

        /// <summary>
        /// 服务器取消请求示例
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<bool>> ServerCancelRequest()
        {
            var cts = new CancellationTokenSource();
            for (int i = 0; i < 20; i++)
            {
                if (i > 10)
                {
                    cts.Cancel(); // 模拟服务器在某个条件下取消请求
                }
                await Task.Delay(1000, cts.Token); // 模拟一些工作
            }
            return ApiResult<bool>.GetSuccess(true);
        }
    }
}
