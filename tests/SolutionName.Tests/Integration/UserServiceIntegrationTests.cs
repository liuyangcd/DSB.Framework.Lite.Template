using DSB.Framework.Lite.Core;
using Microsoft.EntityFrameworkCore;
using CreateRoleInputDto = SolutionName.Application.Contracts.Dtos.Systems.Roles.CreateInputDto;
using CreateUserInputDto = SolutionName.Application.Contracts.Dtos.Systems.Users.CreateInputDto;
using GetUserPageListInputDto = SolutionName.Application.Contracts.Dtos.Systems.Users.GetPageListInputDto;
using UpdateUserStatusInputDto = SolutionName.Application.Contracts.Dtos.Systems.Users.UpdateStatusInputDto;
using SolutionName.Domain;
using SolutionName.Domain.Entities.Systems;
using SolutionName.Domain.Enums;
using SolutionName.Domain.Enums.Systems;
using Xunit;

namespace SolutionName.Tests.Integration;

/// <summary>
/// 用户、角色、权限业务集成测试
/// </summary>
public class UserServiceIntegrationTests : IntegrationTestBase
{
    /// <summary>
    /// 创建用户并分配角色后，应能查询到用户详情、角色关系和角色权限码
    /// </summary>
    [Fact]
    public async Task CreateUser_WithRole_PersistsUserRoleAndPermissionCodes()
    {
        var permissions = await SeedPermissionsAsync();
        var role = await CreateRoleAsync(permissions.Select(x => x.Id).ToList());
        var user = await CreateUserAsync(role.Id, "zhangsan", "张三");

        var detail = await UserService.GetDetailAsync(user.Id);
        Assert.Equal("张三", detail.Name);
        Assert.Contains(role.Id, detail.RoleIds);

        var permissionCodes = await UserRepository.GetUserPermissionCodesAsync(user.Id);
        Assert.Contains(PermissionCodes.User.Detail, permissionCodes);
        Assert.Contains(PermissionCodes.User.Page, permissionCodes);
    }

    /// <summary>
    /// 分页查询应按条件过滤用户，并返回投影后的分页结果
    /// </summary>
    [Fact]
    public async Task GetPageList_ReturnsFilteredUsers()
    {
        var role = await CreateRoleAsync([]);
        await CreateUserAsync(role.Id, "zhangsan", "张三");
        await CreateUserAsync(role.Id, "lisi", "李四");

        var page = await UserService.GetPageListAsync(new GetUserPageListInputDto
        {
            PageIndex = 1,
            PageSize = 10,
            Name = "张"
        });

        Assert.Equal(1, page.Count);
        var item = Assert.Single(page.Items);
        Assert.Equal("张三", item.Name);
    }

    /// <summary>
    /// 通过 ExecuteUpdate 修改用户状态后，数据库中的状态应立即生效
    /// </summary>
    [Fact]
    public async Task UpdateUserStatus_ByExecuteUpdate_PersistsStatus()
    {
        var role = await CreateRoleAsync([]);
        var user = await CreateUserAsync(role.Id, "wangwu", "王五");

        await UserService.UpdateStatusAsync(new UpdateUserStatusInputDto
        {
            Id = user.Id,
            Status = RecordStatus.Disabled
        });

        Context.ChangeTracker.Clear();
        var updated = await UserRepository.GetSingleAsync(user.Id, cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(updated);
        Assert.Equal(RecordStatus.Disabled, updated.Status);
    }

    /// <summary>
    /// 删除用户应执行逻辑删除，默认查询应过滤已删除数据，忽略过滤器后仍可查询
    /// </summary>
    [Fact]
    public async Task DeleteUser_AppliesLogicalDeleteFilter()
    {
        var role = await CreateRoleAsync([]);
        var user = await CreateUserAsync(role.Id, "zhaoliu", "赵六");

        await UserService.DeleteAsync(user.Id);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        Context.ChangeTracker.Clear();

        Assert.Null(await UserRepository.GetSingleAsync(user.Id, cancellationToken: TestContext.Current.CancellationToken));

        var deleted = await UserRepository.NoFilterQueryable.SingleOrDefaultAsync(
            x => x.Id == user.Id,
            TestContext.Current.CancellationToken);
        Assert.NotNull(deleted);
        Assert.False(deleted.EntityStatus);
    }

    /// <summary>
    /// 创建重复账户的用户时，应抛出业务异常
    /// </summary>
    [Fact]
    public async Task CreateUser_WithDuplicateAccount_ThrowsBusinessException()
    {
        var role = await CreateRoleAsync([]);
        await CreateUserAsync(role.Id, "duplicate", "用户一");

        var inputDto = new CreateUserInputDto
        {
            Name = "用户二",
            Account = "duplicate",
            PasswordText = "123456",
            Status = RecordStatus.Normally,
            RoleIds = [role.Id]
        };

        await Assert.ThrowsAsync<BusinessException>(() => UserService.CreateAsync(inputDto, Guid.NewGuid()));
    }

    /// <summary>
    /// 初始化两条用户权限
    /// </summary>
    /// <returns>已保存的权限实体集合</returns>
    private async Task<List<SystemPermissionEntity>> SeedPermissionsAsync()
    {
        var permissions = new List<SystemPermissionEntity>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Code = PermissionCodes.User.Detail,
                Name = "查看用户",
                Type = PermissionType.Api,
                Status = RecordStatus.Normally,
                Sort = 1
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = PermissionCodes.User.Page,
                Name = "用户列表",
                Type = PermissionType.Api,
                Status = RecordStatus.Normally,
                Sort = 2
            }
        };

        await PermissionRepository.InsertRangeAsync(permissions, cancellationToken: TestContext.Current.CancellationToken);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        return permissions;
    }

    /// <summary>
    /// 创建测试角色并分配权限
    /// </summary>
    /// <param name="permissionIds">权限Id集合</param>
    /// <returns>已保存的角色实体</returns>
    private async Task<SystemRoleEntity> CreateRoleAsync(List<Guid> permissionIds)
    {
        var code = $"test-role-{Guid.NewGuid():N}";
        var inputDto = new CreateRoleInputDto
        {
            Code = code,
            Name = "测试角色",
            Status = RecordStatus.Normally,
            Sort = 1,
            PermissionIds = permissionIds
        };

        await RoleService.CreateAsync(inputDto, Guid.NewGuid());
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        return await RoleRepository.GetSingleAsync(x => x.Code == code, cancellationToken: TestContext.Current.CancellationToken)
            ?? throw new InvalidOperationException("测试角色创建失败");
    }

    /// <summary>
    /// 创建测试用户并分配角色
    /// </summary>
    /// <param name="roleId">角色Id</param>
    /// <param name="account">账户</param>
    /// <param name="name">姓名</param>
    /// <returns>已保存的用户实体</returns>
    private async Task<SystemUserEntity> CreateUserAsync(Guid roleId, string account, string name)
    {
        var inputDto = new CreateUserInputDto
        {
            Name = name,
            Account = account,
            PasswordText = "123456",
            Status = RecordStatus.Normally,
            RoleIds = [roleId]
        };

        await UserService.CreateAsync(inputDto, Guid.NewGuid());
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        return await UserRepository.GetSingleAsync(x => x.Account == account, cancellationToken: TestContext.Current.CancellationToken)
            ?? throw new InvalidOperationException("测试用户创建失败");
    }
}
