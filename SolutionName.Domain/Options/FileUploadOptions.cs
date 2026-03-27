using Microsoft.VisualBasic.FileIO;
using SolutionName.Domain.Enums.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SolutionName.Domain.Options
{
    /// <summary>
    /// 文件上传配置项
    /// </summary>
    public class FileUploadOptions
    {
        /// <summary>
        /// 请求文件上传大小限制，单位M
        /// </summary>
        public int FileMaxSize { get; set; } = 1024;

        /// <summary>
        /// 上传文件保存路径
        /// </summary>
        [JsonIgnore]
        public string SavePath { get; set; } = "UploadFiles";

        /// <summary>
        /// 文件类型
        /// </summary>
        public Dictionary<UploadFileType, FileTypeOptions>? FileTypeOptions { get; set; }

        /// <summary>
        /// 判断是否支持上传的文件类型
        /// </summary>
        /// <param name="fileName">文件名，含后缀名</param>
        /// <returns></returns>
        public bool IsSupport(string fileName)
        {
            var fileExtentsion = Path.GetExtension(fileName)?.ToLower();
            return FileTypeOptions?.Values.SelectMany(x => x.Extensions.Split(',', StringSplitOptions.RemoveEmptyEntries)).Contains(fileExtentsion, StringComparer.OrdinalIgnoreCase) ?? false;
        }

        /// <summary>
        /// 获取文件对应的配置
        /// </summary>
        /// <param name="fileName">文件名，含后缀名</param>
        /// <returns></returns>
        public KeyValuePair<UploadFileType, FileTypeOptions>? GetOptions(string fileName)
        {
            var fileExtentsion = Path.GetExtension(fileName)?.ToLower();
            return FileTypeOptions?.FirstOrDefault(x => x.Value.Extensions?.ToLower().Split(',', StringSplitOptions.RemoveEmptyEntries).Contains(fileExtentsion) ?? false) ?? null;
        }
    }

    /// <summary>
    /// 上传文件类型配置
    /// </summary>
    public class FileTypeOptions
    {
        /// <summary>
        /// 后缀名集合，逗号隔开
        /// </summary>
        public required string Extensions { get; set; }

        /// <summary>
        /// 上传文件大小最大值，单位M
        /// </summary>
        public int MaxSize { get; set; }
    }
}
