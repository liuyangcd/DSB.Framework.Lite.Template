using Xunit;

namespace SolutionName.Tests.Integration.MySql;

/// <summary>
/// MySQL 仓储集成测试夹具：整个测试运行期间只删除/重建一次测试库。
/// </summary>
public sealed class MySqlRepositoryFixture : IAsyncLifetime
{
    /// <summary>
    /// 删除并重建测试库，保证每次运行从干净结构开始。
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        await using var context = MySqlTestSettings.CreateContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }

    /// <summary>
    /// 不删除测试库，方便失败后查看数据。
    /// </summary>
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    /// <summary>
    /// 创建一个独立的上下文、工作单元和仓储作用域。
    /// </summary>
    public MySqlRepositoryScope CreateScope() => new();
}

/// <summary>
/// 定义 MySQL 仓储测试集合，使同一集合内的测试串行执行，避免测试库数据互相干扰。
/// </summary>
[CollectionDefinition(Name)]
public sealed class MySqlRepositoryCollection : ICollectionFixture<MySqlRepositoryFixture>
{
    /// <summary>
    /// 集合名称。
    /// </summary>
    public const string Name = "MySqlRepository";
}
