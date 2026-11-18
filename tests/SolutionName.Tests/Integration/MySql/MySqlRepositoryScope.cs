using DSB.Framework.Lite.Data.EFCore.Repository;
using SolutionName.Domain.Entities.Systems;
using SolutionName.EntityFrameworkCore;

namespace SolutionName.Tests.Integration.MySql;

/// <summary>
/// 一次独立的作用域，包含 DbContext、工作单元和泛型仓储。
/// </summary>
public sealed class MySqlRepositoryScope : IAsyncDisposable
{
    /// <summary>
    /// 创建上下文、工作单元以及各实体的泛型仓储。
    /// </summary>
    public MySqlRepositoryScope()
    {
        Context = MySqlTestSettings.CreateContext();
        UnitOfWork = new UnitOfWork<SolutionNameContext>(Context);

        Users = new EntityFrameworkCoreRepository<SolutionNameContext, SystemUserEntity, Guid>(UnitOfWork);
        Roles = new EntityFrameworkCoreRepository<SolutionNameContext, SystemRoleEntity, Guid>(UnitOfWork);
        Permissions = new EntityFrameworkCoreRepository<SolutionNameContext, SystemPermissionEntity, Guid>(UnitOfWork);
        UserRoles = new EntityFrameworkCoreRepository<SolutionNameContext, SystemUserRoleEntity, Guid>(UnitOfWork);
        RolePermissions = new EntityFrameworkCoreRepository<SolutionNameContext, SystemRolePermissionEntity, Guid>(UnitOfWork);
    }

    /// <summary>
    /// 当前数据库上下文。
    /// </summary>
    public SolutionNameContext Context { get; }

    /// <summary>
    /// 当前工作单元。
    /// </summary>
    public IUnitOfWork<SolutionNameContext> UnitOfWork { get; }

    /// <summary>
    /// 用户仓储。
    /// </summary>
    public IEntityFrameworkCoreRepository<SolutionNameContext, SystemUserEntity, Guid> Users { get; }

    /// <summary>
    /// 角色仓储。
    /// </summary>
    public IEntityFrameworkCoreRepository<SolutionNameContext, SystemRoleEntity, Guid> Roles { get; }

    /// <summary>
    /// 权限仓储。
    /// </summary>
    public IEntityFrameworkCoreRepository<SolutionNameContext, SystemPermissionEntity, Guid> Permissions { get; }

    /// <summary>
    /// 用户角色关系仓储。
    /// </summary>
    public IEntityFrameworkCoreRepository<SolutionNameContext, SystemUserRoleEntity, Guid> UserRoles { get; }

    /// <summary>
    /// 角色权限关系仓储。
    /// </summary>
    public IEntityFrameworkCoreRepository<SolutionNameContext, SystemRolePermissionEntity, Guid> RolePermissions { get; }

    /// <summary>
    /// 释放工作单元及数据库上下文。
    /// </summary>
    public ValueTask DisposeAsync()
    {
        UnitOfWork.Dispose();
        return ValueTask.CompletedTask;
    }
}
