using DSB.Framework.Lite.Data.EFCore.Extensions;
using DSB.Framework.Lite.Data.EFCore.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SolutionName.Domain.Entities.Systems;
using SolutionName.EntityFrameworkCore;
using Xunit;
using DbType = DSB.Framework.Lite.Data.EFCore.Extensions.Setting.DbType;

#pragma warning disable xUnit1051 // 集成测试直连数据库，逐调用传入取消令牌会显著降低用例可读性

namespace SolutionName.Tests.Integration.MySql;

/// <summary>
/// 对照仓储文档第 4 章，验证工作单元的提交、显式事务和独立作用域。
/// </summary>
[Collection(MySqlRepositoryCollection.Name)]
public sealed class UnitOfWorkTests(MySqlRepositoryFixture fixture)
{
    /// <summary>
    /// 4.1：一次 SaveChanges 提交上下文中全部待保存变更。
    /// </summary>
    [Fact]
    public async Task SaveChangesAsync_CommitsAllPendingChanges()
    {
        var account = MySqlTestData.Unique("uow-user");
        var code = MySqlTestData.Unique("uow-role");
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var userRoleId = Guid.NewGuid();

        await using var scope = fixture.CreateScope();
        await scope.Users.InsertAsync(new SystemUserEntity
        {
            Id = userId,
            Account = account,
            Name = "工作单元用户"
        });
        await scope.Roles.InsertAsync(new SystemRoleEntity
        {
            Id = roleId,
            Code = code,
            Name = "工作单元角色"
        });
        await scope.UserRoles.InsertAsync(new SystemUserRoleEntity
        {
            Id = userRoleId,
            UserId = userId,
            RoleId = roleId
        });

        await scope.UnitOfWork.SaveChangesAsync();

        await using var reader = fixture.CreateScope();
        Assert.NotNull(await reader.Users.GetSingleAsync(userId));
        Assert.NotNull(await reader.Roles.GetSingleAsync(roleId));
        Assert.True(await reader.UserRoles.IsExistsAsync(x => x.UserId == userId && x.RoleId == roleId));
    }

    /// <summary>
    /// 4.1：未调用 SaveChanges 前，其他上下文看不到当前上下文中的待保存数据。
    /// </summary>
    [Fact]
    public async Task SaveChangesAsync_PendingChangesAreIsolated()
    {
        var account = MySqlTestData.Unique("uow-pending");
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
    /// 4.1/3.14：显式事务提交后，事务内已保存的数据对外可见。
    /// </summary>
    [Fact]
    public async Task BeginTransactionAsync_Commit_PersistsChanges()
    {
        var user = MySqlTestData.CreateUser(MySqlTestData.Unique("tx-commit"));

        await using var scope = fixture.CreateScope();
        await using (var transaction = await scope.UnitOfWork.BeginTransactionAsync())
        {
            await scope.Users.InsertAsync(user, autoSave: true);
            await transaction.CommitAsync();
        }

        await using var reader = fixture.CreateScope();
        Assert.True(await reader.Users.IsExistsAsync(x => x.Id == user.Id));
    }

    /// <summary>
    /// 4.1/3.14：显式事务回滚后，事务内已保存的数据不会落库。
    /// </summary>
    [Fact]
    public async Task BeginTransactionAsync_Rollback_DiscardsChanges()
    {
        var user = MySqlTestData.CreateUser(MySqlTestData.Unique("tx-rollback"));

        await using var scope = fixture.CreateScope();
        await using (var transaction = await scope.UnitOfWork.BeginTransactionAsync())
        {
            await scope.Users.InsertAsync(user, autoSave: true);
            await transaction.RollbackAsync();
        }

        await using var reader = fixture.CreateScope();
        Assert.False(await reader.Users.IsExistsAsync(x => x.Id == user.Id));
    }

    /// <summary>
    /// 4.2：IUnitOfWorkProvider 在请求作用域之外创建独立作用域，并由该作用域自行保存。
    /// </summary>
    [Fact]
    public async Task UnitOfWorkProvider_CreatesIndependentScope()
    {
        var account = MySqlTestData.Unique("provider");
        var user = MySqlTestData.CreateUser(account);

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));
        services.AddEntityFrameworkCoreDbContext<SolutionNameContext>(MySqlTestSettings.GetOptions(), false);
        services.AddGuidGenerator(DbType.MySql);
        services.AddEntityFrameworkCoreRepository(typeof(SolutionNameContext));

        await using var serviceProvider = services.BuildServiceProvider();
        var unitOfWorkProvider = serviceProvider.GetRequiredService<IUnitOfWorkProvider<SolutionNameContext>>();

        using (var unitOfWorkScope = unitOfWorkProvider.CreateScope())
        {
            var repository = unitOfWorkScope.ServiceScope.ServiceProvider
                .GetRequiredService<IEntityFrameworkCoreRepository<SolutionNameContext, SystemUserEntity, Guid>>();

            await repository.InsertAsync(user);
            await unitOfWorkScope.Current.SaveChangesAsync();
        }

        await using var reader = fixture.CreateScope();
        Assert.True(await reader.Users.IsExistsAsync(x => x.Account == account));
    }
}