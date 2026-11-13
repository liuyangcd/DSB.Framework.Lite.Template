using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionName.Application.Contracts.Bos.Systems.UploadFIles
{
    public class UploadInputBo
    {
        /// <summary>
        /// 上传文件名
        /// </summary>
        public required string FileName { get; set; }

        /// <summary>
        /// 上传文件流
        /// </summary>
        public required Stream FileStream { get; set; }

        /// <summary>
        /// 上传文件大小
        /// </summary>
        public required long FileSize { get; set; }
    }
}
