using System.Net;
using Xunit;

#pragma warning disable xUnit1051 // 集成测试直连数据库，逐调用传入取消令牌会显著降低用例可读性

namespace SolutionName.Tests.Integration.MySql;

/// <summary>
/// 对照 WebApi 集成包文档，通过真实的 MVC 管道验证工作单元过滤器的提交与异常中断行为。
/// </summary>
[Collection(MySqlRepositoryCollection.Name)]
public sealed class UnitOfWorkFilterTests(
    MySqlRepositoryFixture fixture,
    UnitOfWorkFilterWebAppFixture webApp) : IClassFixture<UnitOfWorkFilterWebAppFixture>
{
    /// <summary>
    /// Action 正常返回且未手动保存时，过滤器在 Action 之后统一提交。
    /// </summary>
    [Fact]
    public async Task ActionWithoutManualSave_IsCommittedByFilter()
    {
        var account = MySqlTestData.Unique("f-auto");

        var response = await PostAsync("create", account);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(await ExistsAsync(account));
    }

    /// <summary>
    /// Action 抛出异常时，过滤器不提交待保存的变更，数据不会落库。
    /// </summary>
    [Fact]
    public async Task ActionThatThrows_DoesNotCommitPendingChanges()
    {
        var account = MySqlTestData.Unique("f-throw");

        var response = await PostAsync("create-then-throw", account);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.False(await ExistsAsync(account));
    }

    /// <summary>
    /// Action 抛出 BusinessException 时，工作单元过滤器同样跳过提交；
    /// 业务异常按 GlobalExceptionMiddleware 约定返回 HTTP 200，但数据不能落库。
    /// </summary>
    [Fact]
    public async Task BusinessException_DoesNotCommitPendingChanges()
    {
        var account = MySqlTestData.Unique("f-biz");

        var response = await PostAsync("create-business-throw", account);
        var body = await response.Content.ReadAsStringAsync();

        Assert.True(response.StatusCode == HttpStatusCode.OK, body);
        Assert.False(await ExistsAsync(account), body);
    }

    /// <summary>
    /// 标记 UnitOfWorkIgnore 且不手动保存时，请求结束后待保存变更随上下文释放而丢弃。
    /// </summary>
    [Fact]
    public async Task UnitOfWorkIgnore_WithoutManualSave_DoesNotCommit()
    {
        var account = MySqlTestData.Unique("f-ignore");

        var response = await PostAsync("create-ignore", account);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(await ExistsAsync(account));
    }

    /// <summary>
    /// 标记 UnitOfWorkIgnore 时由业务代码控制保存节点，手动 SaveChanges 后正常落库。
    /// </summary>
    [Fact]
    public async Task UnitOfWorkIgnore_WithManualSave_Persists()
    {
        var account = MySqlTestData.Unique("f-ignore-save");

        var response = await PostAsync("create-ignore-save", account);
        var body = await response.Content.ReadAsStringAsync();

        Assert.True(response.StatusCode == HttpStatusCode.OK, body);
        Assert.True(await ExistsAsync(account));
    }

    /// <summary>
    /// 使用 autoSave: true 提前保存后抛出异常：过滤器只跳过自身提交，无法撤销已保存的数据。
    /// 该用例固定文档中的使用约定，提醒统一使用 autoSave: false 交给过滤器提交。
    /// </summary>
    [Fact]
    public async Task AutoSaveBeforeThrow_LeavesAlreadySavedData()
    {
        var account = MySqlTestData.Unique("f-as-throw");

        var response = await PostAsync("create-autosave-then-throw", account);
        var body = await response.Content.ReadAsStringAsync();

        Assert.True(response.StatusCode == HttpStatusCode.InternalServerError, body);
        Assert.True(await ExistsAsync(account), body);
    }

    private Task<HttpResponseMessage> PostAsync(string action, string account)
    {
        return webApp.Client.PostAsync(
            $"/api/unit-of-work-probe/{action}?account={Uri.EscapeDataString(account)}",
            content: null);
    }

    private async Task<bool> ExistsAsync(string account)
    {
        await using var scope = fixture.CreateScope();
        return await scope.Users.IsExistsAsync(x => x.Account == account);
    }
}