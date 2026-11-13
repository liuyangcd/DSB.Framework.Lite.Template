using DSB.Framework.Lite.Data.EFCore.Repository;
using SolutionName.Domain.Entities.Systems;
using System.Linq.Expressions;

namespace SolutionName.EntityFrameworkCore.IRepositories.Systems
{
    /// <summary>
    /// 用户仓储
    /// </summary>
    public interface IUserRepository : IEntityFrameworkCoreRepository<SolutionNameContext, SystemUserEntity, Guid>
    {
        /// <summary>
        /// 获取用户有效的角色列表
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="userId">用户Id</param>
        /// <param name="selector"></param>
        /// <returns></returns>
        Task<List<TResult>> GetUserRoleListAsync<TResult>(Guid userId, Expression<Func<SystemUserRoleEntity, TResult>> selector);

        /// <summary>
        /// 获取用户有效的角色列表
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <returns></returns>
        Task<List<SystemUserRoleEntity>> GetUserRoleListAsync(Guid userId);

        /// <summary>
        /// 初始化用户的角色信息
        /// <para>注意：不会对用户信息和对应的角色信息进行校验，所以仅限初始化用户使用</para>
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="roleIds">角色Id集合</param>
        /// <returns></returns>
        Task InitUserRolesAsync(Guid userId, List<Guid>? roleIds);

        /// <summary>
        /// 更新用户的角色信息
        /// <para>注意：不会对用户信息进行校验</para>
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="roleIds">角色Id集合</param>
        /// <returns></returns>
        Task UpdateUserRolesAsync(Guid userId, List<Guid>? roleIds);

        /// <summary>
        /// 获取用户对应的有效权限码列表
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <returns></returns>
        Task<List<string>> GetUserPermissionCodesAsync(Guid userId);
    }
}
