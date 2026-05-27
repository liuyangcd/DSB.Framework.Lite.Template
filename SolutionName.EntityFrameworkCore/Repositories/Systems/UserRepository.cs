using DSB.Framework.Lite.Core;
using DSB.Framework.Lite.Data.EFCore.Extensions.SequentialGuid;
using DSB.Framework.Lite.Data.EFCore.Repository;
using Microsoft.EntityFrameworkCore;
using SolutionName.Domain.Entities.Systems;
using SolutionName.Domain.Enums;
using SolutionName.EntityFrameworkCore.IRepositories.Systems;
using System.Linq.Expressions;

namespace SolutionName.EntityFrameworkCore.Repositories.Systems
{
    /// <summary>
    /// 用户仓储
    /// </summary>
    public class UserRepository(
        IUnitOfWork<SolutionNameContext> unitOfWork,
        IGuidGenerator guidGenerator,
        IRoleRepository roleRepository,
        IEntityFrameworkCoreRepository<SolutionNameContext, SystemUserRoleEntity, Guid> userRoleRepository,
        IEntityFrameworkCoreRepository<SolutionNameContext, SystemRolePermissionEntity, Guid> rolePermissionRepository,
        IPermissionRepository permissionRepository) : EntityFrameworkCoreRepository<SolutionNameContext, SystemUserEntity, Guid>(unitOfWork), IUserRepository
    {

        /// <summary>
        /// 获取用户有效的角色列表
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="userId">用户Id</param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public async Task<List<TResult>> GetUserRoleListAsync<TResult>(Guid userId, Expression<Func<SystemUserRoleEntity, TResult>> selector)
        {
            var query = from userRole in userRoleRepository.Queryable
                        join role in roleRepository.Queryable on userRole.RoleId equals role.Id
                        where userRole.UserId == userId && role.Status == RecordStatus.Normally
                        orderby role.Sort, role.CreateDateAt descending
                        select userRole;
            var result = await query.Select(selector).ToListAsync();
            return result;
        }

        /// <summary>
        /// 获取用户有效的角色列表
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <returns></returns>
        public async Task<List<SystemUserRoleEntity>> GetUserRoleListAsync(Guid userId)
        {
            var query = from userRole in userRoleRepository.Queryable
                        join role in roleRepository.Queryable on userRole.RoleId equals role.Id
                        where userRole.UserId == userId && role.Status == RecordStatus.Normally
                        orderby role.Sort, role.CreateDateAt descending
                        select userRole;
            var result = await query.ToListAsync();
            return result;
        }

        /// <summary>
        /// 初始化用户的角色信息
        /// <para>注意：不会对用户信息和对应的角色信息进行校验，所以仅限初始化用户使用</para>
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="roleIds">角色Id集合</param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task InitUserRolesAsync(Guid userId, List<Guid>? roleIds)
        {
            if (roleIds.IsNotNullOrEmpty())
            {
                roleIds = [.. roleIds!.Distinct()];
                if (await roleRepository.CountAsync(x => roleIds.Contains(x.Id)) != roleIds.Count) throw new BusinessException("角色信息异常");

                var userRoleList = roleIds.Select(roleId => new SystemUserRoleEntity
                {
                    Id = guidGenerator.Create(),
                    UserId = userId,
                    RoleId = roleId
                }).ToList();

                await userRoleRepository.InsertRangeAsync(userRoleList);
            }
        }

        /// <summary>
        /// 更新用户的角色信息
        /// <para>注意：不会对用户信息进行校验</para>
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="roleIds">角色Id集合</param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task UpdateUserRolesAsync(Guid userId, List<Guid>? roleIds)
        {
            if (roleIds.IsNotNullOrEmpty())
            {
                roleIds = [.. roleIds!.Distinct()];
                var oldUserRoles = await GetUserRoleListAsync(userId);
                var deleteUserRoles = oldUserRoles.Where(x => !roleIds.Contains(x.RoleId)).ToList();
                if (deleteUserRoles.Count > 0)
                {
                    await userRoleRepository.DeprecateRangeAsync(deleteUserRoles);
                }

                var insertRoleIds = roleIds.Except(oldUserRoles.Select(x => x.RoleId)).ToList();
                if (insertRoleIds.Count > 0)
                {
                    if (await roleRepository.CountAsync(x => insertRoleIds.Contains(x.Id)) != insertRoleIds.Count) throw new BusinessException("角色信息异常");

                    var userRoleList = insertRoleIds.Select(roleId => new SystemUserRoleEntity
                    {
                        Id = guidGenerator.Create(),
                        UserId = userId,
                        RoleId = roleId
                    }).ToList();

                    await userRoleRepository.InsertRangeAsync(userRoleList);
                }
            }
            else
            {
                var oldUserRoles = await GetUserRoleListAsync(userId);
                if (oldUserRoles.Count > 0)
                {
                    await userRoleRepository.DeprecateRangeAsync(oldUserRoles);
                }
            }
        }

        /// <summary>
        /// 获取用户对应的有效权限码列表
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <returns></returns>
        public async Task<List<string>> GetUserPermissionCodesAsync(Guid userId)
        {
            var query = from userRole in userRoleRepository.Queryable
                        join role in roleRepository.Queryable on userRole.RoleId equals role.Id
                        join rolePermission in rolePermissionRepository.Queryable on role.Id equals rolePermission.RoleId
                        join permission in permissionRepository.Queryable on rolePermission.PermissionId equals permission.Id
                        where userRole.UserId == userId && role.Status == RecordStatus.Normally && permission.Status == RecordStatus.Normally
                        orderby role.Sort, role.CreateDateAt descending, permission.Sort, permission.CreateDateAt descending
                        select permission.Code;
            var result = await query.Distinct().ToListAsync();
            return result;
        }
    }
}
