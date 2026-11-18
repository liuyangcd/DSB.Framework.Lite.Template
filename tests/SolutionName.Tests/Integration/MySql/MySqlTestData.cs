using SolutionName.Domain.Entities.Systems;
using SolutionName.Domain.Enums;

namespace SolutionName.Tests.Integration.MySql;

/// <summary>
/// 测试实体工厂，使用随机后缀保证数据唯一。
/// </summary>
internal static class MySqlTestData
{
    /// <summary>
    /// 生成带随机后缀的唯一字符串。
    /// </summary>
    /// <param name="prefix">前缀</param>
    public static string Unique(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    /// <summary>
    /// 创建一个测试用户。
    /// </summary>
    /// <param name="account">账户，为空时自动生成</param>
    /// <param name="name">姓名</param>
    /// <param name="status">状态</param>
    public static SystemUserEntity CreateUser(string? account = null, string name = "测试用户", RecordStatus status = RecordStatus.Normally)
    {
        return new SystemUserEntity
        {
            Id = Guid.NewGuid(),
            Account = account ?? Unique("user"),
            Name = name,
            Status = status
        };
    }

    /// <summary>
    /// 创建一个测试角色。
    /// </summary>
    /// <param name="code">编码，为空时自动生成</param>
    /// <param name="sort">排序</param>
    /// <param name="name">名称</param>
    public static SystemRoleEntity CreateRole(string? code = null, int sort = 0, string name = "测试角色")
    {
        return new SystemRoleEntity
        {
            Id = Guid.NewGuid(),
            Code = code ?? Unique("role"),
            Name = name,
            Sort = sort,
            Status = RecordStatus.Normally
        };
    }
}

/// <summary>
/// 仓储投影测试使用的角色摘要。
/// </summary>
public sealed class RoleSummary
{
    /// <summary>
    /// 主键。
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 角色编码。
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 排序。
    /// </summary>
    public int Sort { get; set; }
}
