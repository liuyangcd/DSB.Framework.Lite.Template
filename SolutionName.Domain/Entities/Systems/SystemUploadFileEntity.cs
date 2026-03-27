using DSB.Framework.Lite.Data.EFCore.Repository.Abstractions.Entity;
using Microsoft.VisualBasic.FileIO;
using SolutionName.Domain.Enums.Systems;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionName.Domain.Entities.Systems
{
    /// <summary>
    /// 文件管理
    /// </summary>
    public class SystemUploadFileEntity : EntityBase<Guid>
    {
        /// <summary>
        /// 文件类型
        /// </summary>
        public UploadFileType Type { get; set; }

        /// <summary>
        /// 服务器保存物理相对路径
        /// </summary>
        [StringLength(1000, ErrorMessage = "服务器保存路径不能超过1000个字符")]
        public required string SavePath { get; set; }

        /// <summary>
        /// 服务器保存文件名称
        /// </summary>
        [StringLength(50, ErrorMessage = "服务器保存文件名称不能超过50个字符")]
        public required string SaveName { get; set; }

        /// <summary>
        /// 文件上传原始名称
        /// </summary>
        [StringLength(500, ErrorMessage = "文件上传原始名称不能超过500个字符")]
        public required string OriginName { get; set; }

        /// <summary>
        /// 文件扩展名
        /// </summary>
        [StringLength(50, ErrorMessage = "文件扩展名不能超过50个字符")]
        public required string Extension { get; set; }

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// 文件访问路径
        /// </summary>
        [StringLength(500, ErrorMessage = "文件访问路径不能超过500个字符")]
        public required string Url { get; set; }
    }
}
