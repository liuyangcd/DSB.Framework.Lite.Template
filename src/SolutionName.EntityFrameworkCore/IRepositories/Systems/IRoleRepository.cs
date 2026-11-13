using DSB.Framework.Lite.Data.EFCore.Repository;
using SolutionName.Domain.Entities.Systems;

namespace SolutionName.EntityFrameworkCore.IRepositories.Systems
{
    /// <summary>
    /// 角色仓储
    /// </summary>
    public interface IRoleRepository : IEntityFrameworkCoreRepository<SolutionNameContext, SystemRoleEntity, Guid>
    {
        /// <summary>
        /// 初始化角色的权限信息
        /// <para>注意：不会对角色信息和对应的权限信息进行校验，所以仅限初始化角色使用</para>
        /// </summary>
        /// <param name="roleId">角色Id</param>
        /// <param name="permissionIds">权限Id集合</param>
        /// <returns></returns>
        Task InitRolePermissionsAsync(Guid roleId, List<Guid>? permissionIds);

        /// <summary>
        /// 获取角色有效权限列表
        /// </summary>
        /// <param name="roleId">角色Id</param>
        /// <returns></returns>
        Task<List<SystemRolePermissionEntity>> GetRolePermissionListAsync(Guid roleId);

        /// <summary>
        /// 更新角色的权限信息
        /// <para>注意：不会对角色信息进行校验</para>
        /// </summary>
        /// <param name="roleId">角色Id</param>
        /// <param name="permissionIds">权限Id集合</param>
        /// <returns></returns>
        Task UpdateRolePermissionsAsync(Guid roleId, List<Guid>? permissionIds);
    }
}
