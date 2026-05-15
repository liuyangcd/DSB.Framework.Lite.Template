using DSB.Framework.Lite.Core;
using DSB.Framework.Lite.WebApi.Extensions.Attributes;
using DSB.Framework.Lite.WebApi.Extensions.Http.JwtBearer;
using DSB.Framework.Lite.WebApi.Extensions.SwaggerConfig.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolutionName.Application.Contracts.Bos.Systems.UploadFIles;
using SolutionName.Application.Contracts.Dtos.Systems.UploadFIles;
using SolutionName.Application.Services.Systems;
using SolutionName.Domain;
using SolutionName.Domain.Options;
using System.ComponentModel.DataAnnotations;

namespace SolutionName.HttpApi.Host.Controllers.Systems
{
    /// <summary>
    /// 文件管理
    /// </summary>
    [ApiExplorer(ModuleEnum.File)]
    public class FileController(
        IJwtService jwtService,
        UploadFileService uploadFileService) : ManagerControllerBase(jwtService)
    {
        /// <summary>
        /// 获取上传文件配置
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [IdentifierAuthorize(SolutionNameConsts.PermissionCodes.FileConfig)]
        public Task<FileUploadOptions> GetOptions()
        {
            return Task.FromResult(uploadFileService.GetOptions());
        }

        #region 上传文件

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="file">上传文件</param>
        /// <returns></returns>
        [HttpPost]
        [RequestLogIgnore]
        [IdentifierAuthorize(SolutionNameConsts.PermissionCodes.FileUpload)]
        public async Task<ApiResult<UploadOutputDto>> Upload([Required(ErrorMessage = "上传文件不能为空")] IFormFile file)
        {
            using var fileStream = file.OpenReadStream();
            return ApiResult<UploadOutputDto>.GetSuccess(await uploadFileService.UploadAsync(new UploadInputBo()
            {
                FileName = file.FileName,
                FileStream = fileStream,
                FileSize = file.Length
            }));
        }

        #endregion

        #region 下载文件

        /// <summary>
        /// 获取文件
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <param name="isDownload">是否告诉浏览器以下载方式打开</param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [RequestLogIgnore]
        [Route("{fileName}")]
        public async Task<IActionResult> Get(
            [Required(ErrorMessage = "文件名称不能为空")] string fileName,
            bool isDownload = false)
        {
            var (filePath, originName) = await uploadFileService.GetDownloadInfoAsync(fileName);
            if (isDownload)
            {
                return PhysicalFile(filePath, MimeTypes.Get(originName), originName);
            }
            else
            {
                return PhysicalFile(filePath, MimeTypes.Get(originName));
            }
        }

        #endregion
    }
}
