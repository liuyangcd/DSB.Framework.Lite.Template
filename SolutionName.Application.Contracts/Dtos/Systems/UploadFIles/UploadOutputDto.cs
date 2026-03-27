using SolutionName.Domain.Enums.Systems;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionName.Application.Contracts.Dtos.Systems.UploadFIles
{
    public class UploadOutputDto
    {
        /// <summary>
        /// 上传文件Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 文件类型
        /// </summary>
        public UploadFileType Type { get; set; }

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
        /// 文件大小
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// 文件访问路径
        /// </summary>
        [StringLength(500, ErrorMessage = "文件访问路径不能超过500个字符")]
        public required string Url { get; set; }
    }
}
