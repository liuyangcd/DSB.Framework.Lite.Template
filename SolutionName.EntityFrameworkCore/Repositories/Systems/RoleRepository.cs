using DSB.Framework.Lite.Core;
using DSB.Framework.Lite.Data.EFCore.Extensions.SequentialGuid;
using DSB.Framework.Lite.Data.EFCore.Repository;
using Microsoft.EntityFrameworkCore;
using SolutionName.Domain.Entities.Systems;
using SolutionName.Domain.Enums;
using SolutionName.EntityFrameworkCore.IRepositories.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionName.EntityFrameworkCore.Repositories.Systems
{
    /// <summary>
    /// 角色仓储
    /// </summary>
    public class RoleRepository(
        IUnitOfWork<SolutionNameContext> unitOfWork,
        IGuidGenerator guidGenerator,
        IEntityFrameworkCoreRepository<SolutionNameContext, SystemPermissionEntity, Guid> permissionRepository,
        IEntityFrameworkCoreRepository<SolutionNameContext, SystemRolePermissionEntity, Guid> rolePermissionRepository) : EntityFrameworkCoreRepository<SolutionNameContext, SystemRoleEntity, Guid>(unitOfWork), IRoleRepository
    {
        /// <summary>
        /// 初始化角色的权限信息
        /// <para>注意：不会对角色信息和对应的权限信息进行校验，所以仅限初始化角色使用</para>
        /// </summary>
        /// <param name="roleId">角色Id</param>
        /// <param name="permissionIds">权限Id集合</param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task InitRolePermissionsAsync(Guid roleId, List<Guid>? permissionIds)
        {
            if (permissionIds.IsNotNullOrEmpty())
            {
                permissionIds = [.. permissionIds!.Distinct()];
                if (await permissionRepository.CountAsync(x => permissionIds.Contains(x.Id)) != permissionIds.Count) throw new BusinessException("权限信息异常");

                var userRoleList = permissionIds.Select(permissionId => new SystemRolePermissionEntity
                {
                    Id = guidGenerator.Create(),
                    PermissionId = permissionId,
                    RoleId = roleId
                }).ToList();

                await rolePermissionRepository.InsertRangeAsync(userRoleList);
            }
        }

        /// <summary>
        /// 获取角色有效权限列表
        /// </summary>
        /// <param name="roleId">角色Id</param>
        /// <returns></returns>
        public async Task<List<SystemRolePermissionEntity>> GetRolePermissionListAsync(Guid roleId)
        {
            var query = from rolePermission in rolePermissionRepository.Queryable
                        join permission in permissionRepository.Queryable on rolePermission.PermissionId equals permission.Id
                        where rolePermission.RoleId == roleId && permission.Status == RecordStatus.Normally
                        select rolePermission;
            var result = await query.ToListAsync();
            return result;
        }

        /// <summary>
        /// 更新角色的权限信息
        /// <para>注意：不会对角色信息进行校验</para>
        /// </summary>
        /// <param name="roleId">角色Id</param>
        /// <param name="permissionIds">权限Id集合</param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task UpdateRolePermissionsAsync(Guid roleId, List<Guid>? permissionIds)
        {
            if (permissionIds.IsNotNullOrEmpty())
            {
                permissionIds = [.. permissionIds!.Distinct()];
                var oldRolePermissions = await GetRolePermissionListAsync(roleId);

                var deleteRolePermissions = oldRolePermissions.Where(x => !permissionIds.Contains(x.PermissionId)).ToList();
                if (deleteRolePermissions.Count > 0)
                {
                    await rolePermissionRepository.DeprecateRangeAsync(deleteRolePermissions);
                }

                var insertPermissionIds = permissionIds.Except(oldRolePermissions.Select(x => x.PermissionId)).ToList();
                if (insertPermissionIds.Count > 0)
                {
                    if (await permissionRepository.CountAsync(x => insertPermissionIds.Contains(x.Id)) != insertPermissionIds.Count) throw new BusinessException("权限信息异常");

                    var rolePermissionList = insertPermissionIds.Select(permissionId => new SystemRolePermissionEntity
                    {
                        Id = guidGenerator.Create(),
                        PermissionId = permissionId,
                        RoleId = roleId
                    }).ToList();

                    await rolePermissionRepository.InsertRangeAsync(rolePermissionList);
                }
            }
            else
            {
                var oldRolePermissions = await GetRolePermissionListAsync(roleId);
                if (oldRolePermissions.Count > 0)
                {
                    await rolePermissionRepository.DeprecateRangeAsync(oldRolePermissions);
                }
            }
        }

    }
}
