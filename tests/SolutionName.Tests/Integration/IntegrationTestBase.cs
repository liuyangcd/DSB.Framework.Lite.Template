using DSB.Framework.Lite.Application.Password.Abstractions;
using DSB.Framework.Lite.Application.PBKDF2Password;
using DSB.Framework.Lite.Data.EFCore.Extensions.SequentialGuid;
using DSB.Framework.Lite.Data.EFCore.Repository;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SolutionName.Application.Services.Systems;
using SolutionName.Domain.Entities.Systems;
using SolutionName.EntityFrameworkCore;
using SolutionName.EntityFrameworkCore.IRepositories.Systems;
using SolutionName.EntityFrameworkCore.Repositories.Systems;

namespace SolutionName.Tests.Integration;

/// <summary>
/// 业务集成测试基类：使用 SQLite 内存数据库搭建真实的 DbContext、仓储和服务。
/// </summary>
public abstract class IntegrationTestBase : IDisposable
{
    private readonly SqliteConnection connection;

    /// <summary>
    /// 初始化 SQLite 内存数据库、工作单元、仓储和应用服务
    /// </summary>
    protected IntegrationTestBase()
    {
        connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<SolutionNameContext>()
            .UseSqlite(connection)
            .Options;

        Context = new SolutionNameContext(options);
        Context.Database.EnsureCreated();

        UnitOfWork = new UnitOfWork<SolutionNameContext>(Context);

        var rolePermissionRepository = new EntityFrameworkCoreRepository<SolutionNameContext, SystemRolePermissionEntity, Guid>(UnitOfWork);
        var userRoleRepository = new EntityFrameworkCoreRepository<SolutionNameContext, SystemUserRoleEntity, Guid>(UnitOfWork);

        PermissionRepository = new PermissionRepository(UnitOfWork);
        RoleRepository = new RoleRepository(UnitOfWork, GuidGenerator, PermissionRepository, rolePermissionRepository);
        UserRepository = new UserRepository(
            UnitOfWork,
            GuidGenerator,
            RoleRepository,
            userRoleRepository,
            rolePermissionRepository,
            PermissionRepository);

        PasswordService = new PBKDF2PasswordService(10000, 16, 32, KeyDerivationPrf.HMACSHA256);
        UserService = new UserService(GuidGenerator, PasswordService, UserRepository);
        RoleService = new RoleService(RoleRepository, GuidGenerator);
    }

    /// <summary>
    /// 测试数据库上下文
    /// </summary>
    protected SolutionNameContext Context { get; }

    /// <summary>
    /// 工作单元，用于提交测试中的数据库变更
    /// </summary>
    protected IUnitOfWork<SolutionNameContext> UnitOfWork { get; }

    /// <summary>
    /// 测试用 Guid 生成器
    /// </summary>
    protected IGuidGenerator GuidGenerator { get; } = new TestGuidGenerator();

    /// <summary>
    /// 权限仓储
    /// </summary>
    protected IPermissionRepository PermissionRepository { get; }

    /// <summary>
    /// 角色仓储
    /// </summary>
    protected IRoleRepository RoleRepository { get; }

    /// <summary>
    /// 用户仓储
    /// </summary>
    protected IUserRepository UserRepository { get; }

    /// <summary>
    /// 密码服务
    /// </summary>
    protected IPasswordService PasswordService { get; }

    /// <summary>
    /// 用户应用服务
    /// </summary>
    protected UserService UserService { get; }

    /// <summary>
    /// 角色应用服务
    /// </summary>
    protected RoleService RoleService { get; }

    /// <summary>
    /// 释放数据库上下文和 SQLite 连接
    /// </summary>
    public void Dispose()
    {
        Context.Dispose();
        connection.Dispose();
    }

    /// <summary>
    /// 测试用 Guid 生成器
    /// </summary>
    private sealed class TestGuidGenerator : IGuidGenerator
    {
        /// <summary>
        /// 生成一个新的 Guid
        /// </summary>
        /// <returns>新的 Guid</returns>
        public Guid Create() => Guid.NewGuid();
    }
}
