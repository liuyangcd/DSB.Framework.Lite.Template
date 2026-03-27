using DSB.Framework.Lite.Core;
using DSB.Framework.Lite.Data.EFCore.Extensions.SequentialGuid;
using Microsoft.Extensions.Options;
using SolutionName.Application.Contracts.Bos.Systems.UploadFIles;
using SolutionName.Application.Contracts.Dtos.Systems.UploadFIles;
using SolutionName.Domain.Entities.Systems;
using SolutionName.Domain.Options;
using SolutionName.EntityFrameworkCore.Repositories.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SolutionName.Application.Services.Systems
{
    /// <summary>
    /// 上传文件管理服务
    /// </summary>
    public class UploadFileService(
        UploadFileRepository uploadFileRepository,
        IGuidGenerator guidGenerator,
        IOptions<FileUploadOptions> fileUploadOptions) : SolutionNameApplicationService
    {
        private readonly FileUploadOptions fileUploadOptions = fileUploadOptions.Value;

        /// <summary>
        /// 获取上传文件配置
        /// </summary>
        /// <returns></returns>
        public FileUploadOptions GetOptions() => fileUploadOptions;

        #region 上传文件
        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="file">上传文件</param>
        /// <returns></returns>
        public async Task<UploadOutputDto> UploadAsync(UploadInputBo file)
        {
            if (fileUploadOptions.IsSupport(file.FileName))
            {
                var options = fileUploadOptions.GetOptions(file.FileName)!.Value;
                if (file.FileSize > options.Value.MaxSize * 1024 * 1024)
                {
                    throw new BusinessException("上传文件大小超过限制");
                }
                else
                {
                    #region 保存文件
                    var extension = Path.GetExtension(file.FileName);
                    var fileId = guidGenerator.Create();
                    var saveName = $"{fileId}{extension}";
                    var savePath = Path.Combine(fileUploadOptions.SavePath, $"{DateTime.Now:yyyyMM}", $"{DateTime.Now:dd}", saveName);
                    var saveFullPath = Path.Combine(AppContext.BaseDirectory, savePath);
                    var saveDir = Path.GetDirectoryName(saveFullPath)!;
                    if (!Directory.Exists(saveDir))
                    {
                        Directory.CreateDirectory(saveDir);
                    }
                    var fs = new FileStream(saveFullPath, FileMode.CreateNew);
                    file.FileStream.CopyTo(fs);
                    fs.Close();
                    #endregion

                    var fileEntity = new SystemUploadFileEntity()
                    {
                        Extension = extension,
                        FileSize = file.FileSize,
                        Id = fileId,
                        OriginName = file.FileName,
                        Type = options.Key,
                        SaveName = saveName,
                        SavePath = savePath,
                        Url = $"api/file/get/{saveName}"
                    };
                    await uploadFileRepository.InsertAsync(fileEntity);
                    return fileEntity.TransObject<SystemUploadFileEntity, UploadOutputDto>();
                }
            }
            else
            {
                throw new BusinessException("不支持上传的文件类型");
            }
        }
        #endregion

        #region 下载文件
        /// <summary>
        /// 根据文件保存名称获取对应文件自定义实体
        /// </summary>
        /// <param name="fileName">文件保存名称</param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        private async Task<TResult> GetFileInfoBySaveNameAsync<TResult>(string fileName, Expression<Func<SystemUploadFileEntity, TResult>> selector)
        {
            if (Guid.TryParse(Path.GetFileNameWithoutExtension(fileName), out Guid fileId) && fileId != Guid.Empty)
            {
                var fileEntity = await uploadFileRepository.GetSingleAsync(fileId, selector);
                if (fileEntity != null)
                {
                    return fileEntity;
                }
                throw new BusinessException("文件不存在");
            }
            else
            {
                throw new BusinessException("文件名称错误");
            }
        }

        /// <summary>
        /// 根据文件保存名称获取对应文件保存信息
        /// </summary>
        /// <param name="fileName">文件保存名称</param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task<(string filePath, string originName)> GetDownloadInfoAsync(string fileName)
        {
            var fileEntity = await GetFileInfoBySaveNameAsync(fileName, x => new
            {
                x.OriginName,
                x.Id,
                x.SavePath
            });
            var getFileExtension = Path.GetExtension(fileName);
            var filePath = string.Empty;
            var originName = string.Empty;
            if (getFileExtension.Equals(Path.GetExtension(fileEntity.OriginName), StringComparison.OrdinalIgnoreCase))
            {
                filePath = fileEntity.SavePath;
                originName = fileEntity.OriginName;
            }
            else
            {
                throw new BusinessException("文件类型不一致");
            }
            if (filePath.IsNotNullOrEmpty())
            {
                filePath = Path.Combine(AppContext.BaseDirectory, filePath);
                if (File.Exists(filePath))
                {
                    return new(filePath, originName);
                }
            }
            throw new BusinessException("文件不存在");
        }
        #endregion

    }
}
