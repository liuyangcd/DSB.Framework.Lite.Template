using SolutionName.Domain.Enums.Systems;
using SolutionName.Domain.Options;
using Xunit;

namespace SolutionName.Tests;

/// <summary>
/// 文件上传配置单元测试
/// </summary>
public class FileUploadOptionsTests
{
    /// <summary>
    /// 创建测试用的上传配置：图片最大 20MB，文档最大 100MB
    /// </summary>
    /// <returns>包含图片和文档类型配置的上传选项</returns>
    private static FileUploadOptions CreateOptions()
    {
        return new FileUploadOptions
        {
            FileTypeOptions = new Dictionary<UploadFileType, FileTypeOptions>
            {
                [UploadFileType.Pictrue] = new() { Extensions = ".jpg,.jpeg,.png", MaxSize = 20 },
                [UploadFileType.Document] = new() { Extensions = ".pdf,.docx", MaxSize = 100 }
            }
        };
    }

    /// <summary>
    /// 验证文件扩展名是否在上传配置的支持范围内
    /// </summary>
    /// <param name="fileName">待判断的文件名，包含扩展名</param>
    /// <param name="expected">期望的支持结果</param>
    [Theory]
    [InlineData("photo.JPG", true)]
    [InlineData("photo.jpeg", true)]
    [InlineData("photo.png", true)]
    [InlineData("photo.gif", false)]
    [InlineData("document.pdf", true)]
    [InlineData("archive.zip", false)]
    public void IsSupport_ReturnsExpectedResult(string fileName, bool expected)
    {
        Assert.Equal(expected, CreateOptions().IsSupport(fileName));
    }

    /// <summary>
    /// 未配置文件类型配置时，任何文件都不应被支持
    /// </summary>
    [Fact]
    public void IsSupport_ReturnsFalse_WhenFileTypeOptionsIsNull()
    {
        var options = new FileUploadOptions { FileTypeOptions = null };

        Assert.False(options.IsSupport("photo.jpg"));
    }

    /// <summary>
    /// 获取文件配置时应忽略扩展名大小写，并返回匹配的类型和大小限制
    /// </summary>
    [Fact]
    public void GetOptions_ReturnsMatchingOptions_IgnoringCase()
    {
        var result = CreateOptions().GetOptions("REPORT.PDF");

        Assert.NotNull(result);
        Assert.Equal(UploadFileType.Document, result.Value.Key);
        Assert.Equal(100, result.Value.Value.MaxSize);
    }

    /// <summary>
    /// 获取未配置文件扩展名的选项时应返回 null，而不是默认 KeyValuePair
    /// </summary>
    [Fact]
    public void GetOptions_ReturnsNull_WhenExtensionIsNotConfigured()
    {
        Assert.Null(CreateOptions().GetOptions("archive.zip"));
    }
}
