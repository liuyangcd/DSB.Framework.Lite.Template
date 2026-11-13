using System.ComponentModel;

namespace SolutionName.Domain.Enums.Systems
{
    /// <summary>
    /// 上传文件类型
    /// </summary>
    public enum UploadFileType
    {
        /// <summary>
        /// 图片
        /// </summary>
        [Description("图片")]
        Pictrue = 1,

        /// <summary>
        /// 音频
        /// </summary>
        [Description("音频")]
        Audio,

        /// <summary>
        /// 视频
        /// </summary>
        [Description("视频")]
        Video,

        /// <summary>
        /// 文档
        /// </summary>
        [Description("文档")]
        Document,

        /// <summary>
        /// 附件
        /// </summary>
        [Description("附件")]
        Attachment
    }
}
