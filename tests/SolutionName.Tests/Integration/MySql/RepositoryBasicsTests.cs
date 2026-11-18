using DSB.Framework.Lite.Data.Repository.Abstractions;
using Microsoft.EntityFrameworkCore;
using SolutionName.Domain.Entities.Systems;
using SolutionName.Domain.Enums;
using Xunit;

#pragma warning disable xUnit1051 // 集成测试直连数据库，逐调用传入取消令牌会显著降低用例可读性

namespace SolutionName.Tests.Integration.MySql;

/// <summary>
/// 对照 DSB.Framework.Lite.Data.EFCore.Repository 文档第 3 章，验证仓储基础方法的封装行为：
/// 新增、查询、投影、分页、聚合、更新、删除与逻辑删除。
/// </summary>
[Collection(MySqlRepositoryCollection.Name)]
public sealed class RepositoryBasicsTests(MySqlRepositoryFixture fixture)
{
    /// <summary>
    /// 3.8/3.13：autoSave 默认为 false，只加入跟踪；必须调用工作单元 SaveChanges 才会落库。
    /// </summary>
    [Fact]
    public async Task InsertAsync_WithoutAutoSave_RequiresSaveChanges()
    {
        var account = MySqlTestData.Unique("insert");
        var user = MySqlTestData.CreateUser(account);

        await using var scope = fixture.CreateScope();
        await scope.Users.InsertAsync(user);

        await using (var reader = fixture.CreateScope())
        {
            Assert.False(await reader.Users.IsExistsAsync(x => x.Account == account));
        }

        await scope.UnitOfWork.SaveChangesAsync();

        await using (var reader = fixture.CreateScope())
        {
            Assert.True(await reader.Users.IsExistsAsync(x => x.Account == account));
        }
    }

    /// <summary>
    /// 3.8：autoSave 为 true 时立即保存；批量新增返回受影响行数是否等于集合数量。
    /// </summary>
    [Fact]
    public async Task InsertAsync_WithAutoSave_PersistsImmediately()
    {
        var account = MySqlTestData.Unique("autosave");
        var user = MySqlTestData.CreateUser(account);

        await using var scope = fixture.CreateScope();
        await scope.Users.InsertAsync(user, autoSave: true);

        await using var reader = fixture.CreateScope();
        Assert.True(await reader.Users.IsExistsAsync(x => x.Account == account));
    }

    /// <summary>
    /// 3.8：批量新增在 autoSave: false 时返回 true（仅表示已加入跟踪），保存后全部落库。
    /// </summary>
    [Fact]
    public async Task InsertRangeAsync_WithoutAutoSave_ReturnsTrueAndPersistsAfterSave()
    {
        var prefix = MySqlTestData.Unique("range");
        var roles = new List<SystemRoleEntity>
        {
            MySqlTestData.CreateRole($"{prefix}-1", 1),
            MySqlTestData.CreateRole($"{prefix}-2", 2)
        };

        await using var scope = fixture.CreateScope();
        var tracked = await scope.Roles.InsertRangeAsync(roles);
        Assert.True(tracked);

        await scope.UnitOfWork.SaveChangesAsync();

        await using var reader = fixture.CreateScope();
        Assert.Equal(2, await reader.Roles.CountAsync(x => x.Code.StartsWith(prefix)));
    }

    /// <summary>
    /// 3.4：按主键集合查询，并按 SortSpec 排序；topNumber 限制返回条数。
    /// </summary>
    [Fact]
    public async Task GetListAsync_ByIds_AppliesSortAndTopNumber()
    {
        var prefix = MySqlTestData.Unique("byids");
        var roles = new List<SystemRoleEntity>
        {
            MySqlTestData.CreateRole($"{prefix}-1", 30),
            MySqlTestData.CreateRole($"{prefix}-2", 10),
            MySqlTestData.CreateRole($"{prefix}-3", 20)
        };

        await using var scope = fixture.CreateScope();
        await scope.Roles.InsertRangeAsync(roles, autoSave: true);

        var ids = roles.Select(x => x.Id).ToList();
        var result = await scope.Roles.GetListAsync(
            ids,
            new List<SortSpec<SystemRoleEntity>> { new SortSpecAsc<SystemRoleEntity>(x => x.Sort) });

        Assert.Equal(3, result.Count);
        Assert.Equal(10, result[0].Sort);
        Assert.Equal(20, result[1].Sort);
        Assert.Equal(30, result[2].Sort);
    }

    /// <summary>
    /// 3.3：GetSingleAsync 支持按条件查询，GetAsync 按排序取第一条。
    /// </summary>
    [Fact]
    public async Task GetSingleAndGetAsync_ReturnExpectedEntity()
    {
        var prefix = MySqlTestData.Unique("single");
        var roles = new List<SystemRoleEntity>
        {
            MySqlTestData.CreateRole($"{prefix}-1", 1),
            MySqlTestData.CreateRole($"{prefix}-2", 2)
        };

        await using var scope = fixture.CreateScope();
        await scope.Roles.InsertRangeAsync(roles, autoSave: true);

        var single = await scope.Roles.GetSingleAsync(x => x.Code == $"{prefix}-1");
        Assert.NotNull(single);
        Assert.Equal(1, single.Sort);

        var first = await scope.Roles.GetAsync(
            x => x.Code.StartsWith(prefix),
            new SortSpecDesc<SystemRoleEntity>(x => x.Sort));
        Assert.NotNull(first);
        Assert.Equal(2, first.Sort);
    }

    /// <summary>
    /// 3.3/3.6：按主键投影只查询指定列，返回类型不需要继承 EntityBase。
    /// </summary>
    [Fact]
    public async Task GetSingleAsync_WithProjection_ReturnsMappedResult()
    {
        var code = MySqlTestData.Unique("projection");
        var role = MySqlTestData.CreateRole(code, 8);

        await using var scope = fixture.CreateScope();
        await scope.Roles.InsertAsync(role, autoSave: true);

        var summary = await scope.Roles.GetSingleAsync(
            role.Id,
            x => new RoleSummary
            {
                Id = x.Id,
                Code = x.Code,
                Sort = x.Sort
            });

        Assert.NotNull(summary);
        Assert.Equal(role.Id, summary.Id);
        Assert.Equal(code, summary.Code);
        Assert.Equal(8, summary.Sort);
    }

    /// <summary>
    /// 3.6：列表投影与 topNumber 组合使用。
    /// </summary>
    [Fact]
    public async Task GetListAsync_WithProjection_ReturnsLimitedResult()
    {
        var prefix = MySqlTestData.Unique("projection-list");
        var roles = new List<SystemRoleEntity>
        {
            MySqlTestData.CreateRole($"{prefix}-1", 1),
            MySqlTestData.CreateRole($"{prefix}-2", 2),
            MySqlTestData.CreateRole($"{prefix}-3", 3)
        };

        await using var scope = fixture.CreateScope();
        await scope.Roles.InsertRangeAsync(roles, autoSave: true);

        var result = await scope.Roles.GetListAsync(
            x => x.Code.StartsWith(prefix),
            x => new RoleSummary { Id = x.Id, Code = x.Code, Sort = x.Sort },
            topNumber: 2);

        Assert.Equal(2, result.Count);
        Assert.All(result, item => Assert.StartsWith(prefix, item.Code));
    }

    /// <summary>
    /// 3.2：验证存在性、计数、最大/最小/求和聚合方法。
    /// </summary>
    [Fact]
    public async Task AggregateMethods_ReturnExpectedValues()
    {
        var prefix = MySqlTestData.Unique("agg");
        var roles = new List<SystemRoleEntity>
        {
            MySqlTestData.CreateRole($"{prefix}-1", 5),
            MySqlTestData.CreateRole($"{prefix}-2", 7),
            MySqlTestData.CreateRole($"{prefix}-3", 9)
        };

        await using var scope = fixture.CreateScope();
        await scope.Roles.InsertRangeAsync(roles, autoSave: true);

        Assert.True(await scope.Roles.IsExistsAsync(x => x.Code == $"{prefix}-2"));
        Assert.Equal(3, await scope.Roles.CountAsync(x => x.Code.StartsWith(prefix)));
        Assert.Equal(3L, await scope.Roles.LongCountAsync(x => x.Code.StartsWith(prefix)));
        Assert.Equal(9, await scope.Roles.MaxAsync(x => x.Code.StartsWith(prefix), x => x.Sort));
        Assert.Equal(5, await scope.Roles.MinAsync(x => x.Code.StartsWith(prefix), x => x.Sort));
        Assert.Equal(21, await scope.Roles.SumAsync(x => x.Code.StartsWith(prefix), x => x.Sort));
    }

    /// <summary>
    /// 3.5：分页从 1 开始，返回总数；isCount: false 时不执行计数 SQL，Count 为 -1。
    /// </summary>
    [Fact]
    public async Task GetPagedListAsync_ReturnsPageAndSupportsNoCount()
    {
        var prefix = MySqlTestData.Unique("page");
        var roles = new List<SystemRoleEntity>
        {
            MySqlTestData.CreateRole($"{prefix}-1", 1),
            MySqlTestData.CreateRole($"{prefix}-2", 2),
            MySqlTestData.CreateRole($"{prefix}-3", 3),
            MySqlTestData.CreateRole($"{prefix}-4", 4),
            MySqlTestData.CreateRole($"{prefix}-5", 5)
        };

        await using var scope = fixture.CreateScope();
        await scope.Roles.InsertRangeAsync(roles, autoSave: true);

        var page = await scope.Roles.GetPagedListAsync(
            new PagedQueryBase { PageIndex = 2, PageSize = 2 },
            x => x.Code.StartsWith(prefix),
            new SortSpecAsc<SystemRoleEntity>(x => x.Sort));

        Assert.Equal(5L, page.Count);
        Assert.Equal(2, page.PageIndex);
        Assert.Equal(2, page.PageSize);
        Assert.Equal(2, page.Items.Count);
        Assert.Equal(3, page.Items[0].Sort);
        Assert.Equal(4, page.Items[1].Sort);

        var withoutCount = await scope.Roles.GetPagedListAsync(
            new PagedQueryBase { PageIndex = 1, PageSize = 2 },
            x => x.Code.StartsWith(prefix),
            new SortSpecAsc<SystemRoleEntity>(x => x.Sort),
            isCount: false);

        Assert.Equal(-1L, withoutCount.Count);
        Assert.Equal(2, withoutCount.Items.Count);
    }

    /// <summary>
    /// 3.9：UpdateAsync(id, action) 内部使用跟踪查询，保存后只更新变化字段，并维护 UpdateDateAt。
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ById_UpdatesFieldsAndUpdateDateAt()
    {
        var code = MySqlTestData.Unique("update");
        var role = MySqlTestData.CreateRole(code, 1, "旧名称");

        await using var scope = fixture.CreateScope();
        await scope.Roles.InsertAsync(role, autoSave: true);

        var updated = await scope.Roles.UpdateAsync(role.Id, x =>
        {
            x.Name = "新名称";
            x.Sort = 42;
        }, autoSave: true);

        Assert.True(updated);

        await using var reader = fixture.CreateScope();
        var reloaded = await reader.Roles.GetSingleAsync(role.Id);

        Assert.NotNull(reloaded);
        Assert.Equal("新名称", reloaded.Name);
        Assert.Equal(42, reloaded.Sort);
        Assert.NotNull(reloaded.UpdateDateAt);
    }

    /// <summary>
    /// 3.10：ExecuteUpdateAsync 直接下发 UPDATE，不需要 SaveChanges，并且立即生效。
    /// </summary>
    [Fact]
    public async Task ExecuteUpdateAsync_ById_UpdatesWithoutSaveChanges()
    {
        var role = MySqlTestData.CreateRole(MySqlTestData.Unique("execute-update"), 1);

        await using var scope = fixture.CreateScope();
        await scope.Roles.InsertAsync(role, autoSave: true);

        var updated = await scope.Roles.ExecuteUpdateAsync(
            role.Id,
            setters => setters.SetProperty(x => x.Name, "服务端更新"));

        Assert.True(updated);

        await using var reader = fixture.CreateScope();
        var reloaded = await reader.Roles.GetSingleAsync(role.Id);
        Assert.NotNull(reloaded);
        Assert.Equal("服务端更新", reloaded.Name);
    }

    /// <summary>
    /// 3.10：按条件批量 ExecuteUpdateAsync 返回受影响行数，并自动追加 UpdateDateAt。
    /// </summary>
    [Fact]
    public async Task ExecuteUpdateAsync_ByPredicate_ReturnsAffectedCount()
    {
        var prefix = MySqlTestData.Unique("batch-update");
        var roles = new List<SystemRoleEntity>
        {
            MySqlTestData.CreateRole($"{prefix}-1", 1),
            MySqlTestData.CreateRole($"{prefix}-2", 2)
        };

        await using var scope = fixture.CreateScope();
        await scope.Roles.InsertRangeAsync(roles, autoSave: true);

        var affected = await scope.Roles.ExecuteUpdateAsync(
            x => x.Code.StartsWith(prefix),
            setters => setters.SetProperty(x => x.Name, "批量更新"));

        Assert.Equal(2, affected);

        await using var reader = fixture.CreateScope();
        var reloaded = await reader.Roles.GetListAsync(x => x.Code.StartsWith(prefix));
        Assert.Equal(2, reloaded.Count);
        Assert.All(reloaded, item => Assert.Equal("批量更新", item.Name));
        Assert.All(reloaded, item => Assert.NotNull(item.UpdateDateAt));
    }

    /// <summary>
    /// 3.12：DeprecateAsync 只置 EntityStatus=false；
    /// 默认查询看不到，NoFilterQueryable 可以查到。
    /// </summary>
    [Fact]
    public async Task DeprecateAsync_AppliesLogicalDeleteFilter()
    {
        var role = MySqlTestData.CreateRole(MySqlTestData.Unique("deprecate"), 1);

        await using (var seed = fixture.CreateScope())
        {
            await seed.Roles.InsertAsync(role, autoSave: true);
        }

        await using (var scope = fixture.CreateScope())
        {
            await scope.Roles.DeprecateAsync(role.Id, autoSave: true);
        }

        await using var reader = fixture.CreateScope();

        Assert.Null(await reader.Roles.GetSingleAsync(role.Id));
        var deprecated = await reader.Roles.NoFilterQueryable
            .SingleOrDefaultAsync(x => x.Id == role.Id);
        Assert.NotNull(deprecated);
        Assert.False(deprecated.EntityStatus);
    }

    /// <summary>
    /// 3.12：ExecuteDeprecateAsync 按条件立即逻辑删除，返回受影响行数。
    /// </summary>
    [Fact]
    public async Task ExecuteDeprecateAsync_ByPredicate_ReturnsAffectedCount()
    {
        var prefix = MySqlTestData.Unique("execute-deprecate");
        var roles = new List<SystemRoleEntity>
        {
            MySqlTestData.CreateRole($"{prefix}-1", 1),
            MySqlTestData.CreateRole($"{prefix}-2", 2),
            MySqlTestData.CreateRole($"{prefix}-3", 3)
        };

        await using var scope = fixture.CreateScope();
        await scope.Roles.InsertRangeAsync(roles, autoSave: true);

        var affected = await scope.Roles.ExecuteDeprecateAsync(x => x.Code.StartsWith(prefix));
        Assert.Equal(3, affected);

        await using var reader = fixture.CreateScope();
        Assert.Equal(0, await reader.Roles.CountAsync(x => x.Code.StartsWith(prefix)));
        Assert.Equal(3, await reader.Roles.NoFilterQueryable.CountAsync(x => x.Code.StartsWith(prefix)));
    }

    /// <summary>
    /// 3.11：RemoveAsync 物理删除，NoFilterQueryable 也查不到。
    /// </summary>
    [Fact]
    public async Task RemoveAsync_DeletesRowPhysically()
    {
        var role = MySqlTestData.CreateRole(MySqlTestData.Unique("remove"), 1);

        await using (var seed = fixture.CreateScope())
        {
            await seed.Roles.InsertAsync(role, autoSave: true);
        }

        await using var scope = fixture.CreateScope();
        var removed = await scope.Roles.RemoveAsync(role.Id, autoSave: true);
        Assert.True(removed);

        await using var reader = fixture.CreateScope();
        Assert.Null(await reader.Roles.NoFilterQueryable.SingleOrDefaultAsync(x => x.Id == role.Id));
    }

    /// <summary>
    /// 3.11：ExecuteDeleteAsync 按条件立即物理删除，返回受影响行数。
    /// </summary>
    [Fact]
    public async Task ExecuteDeleteAsync_ByPredicate_ReturnsAffectedCount()
    {
        var prefix = MySqlTestData.Unique("execute-delete");
        var roles = new List<SystemRoleEntity>
        {
            MySqlTestData.CreateRole($"{prefix}-1", 1),
            MySqlTestData.CreateRole($"{prefix}-2", 2)
        };

        await using var scope = fixture.CreateScope();
        await scope.Roles.InsertRangeAsync(roles, autoSave: true);

        var affected = await scope.Roles.ExecuteDeleteAsync(x => x.Code.StartsWith(prefix));
        Assert.Equal(2, affected);

        await using var reader = fixture.CreateScope();
        Assert.Equal(0, await reader.Roles.NoFilterQueryable.CountAsync(x => x.Code.StartsWith(prefix)));
    }

    /// <summary>
    /// 3.7：上下文默认无跟踪，直接修改查询结果后 SaveChanges 不会更新数据库。
    /// </summary>
    [Fact]
    public async Task NoTrackingQuery_Modification_IsNotPersisted()
    {
        var account = MySqlTestData.Unique("no-tracking");
        var user = MySqlTestData.CreateUser(account, "原始姓名");

        await using (var seed = fixture.CreateScope())
        {
            await seed.Users.InsertAsync(user, autoSave: true);
        }

        await using (var scope = fixture.CreateScope())
        {
            var loaded = await scope.Users.GetSingleAsync(user.Id);
            Assert.NotNull(loaded);

            loaded.Name = "未跟踪的修改";
            await scope.UnitOfWork.SaveChangesAsync();
        }

        await using var reader = fixture.CreateScope();
        var reloaded = await reader.Users.GetSingleAsync(user.Id);
        Assert.NotNull(reloaded);
        Assert.Equal("原始姓名", reloaded.Name);
    }

    /// <summary>
    /// 3.7：isTracking: true 查询后修改实体，SaveChanges 只更新发生变化的列。
    /// </summary>
    [Fact]
    public async Task TrackingQuery_Modification_IsPersisted()
    {
        var account = MySqlTestData.Unique("tracking");
        var user = MySqlTestData.CreateUser(account, "原始姓名");

        await using (var seed = fixture.CreateScope())
        {
            await seed.Users.InsertAsync(user, autoSave: true);
        }

        await using (var scope = fixture.CreateScope())
        {
            var loaded = await scope.Users.GetSingleAsync(user.Id, isTracking: true);
            Assert.NotNull(loaded);

            loaded.Name = "跟踪后的修改";
            await scope.UnitOfWork.SaveChangesAsync();
        }

        await using var reader = fixture.CreateScope();
        var reloaded = await reader.Users.GetSingleAsync(user.Id);
        Assert.NotNull(reloaded);
        Assert.Equal("跟踪后的修改", reloaded.Name);
    }
}