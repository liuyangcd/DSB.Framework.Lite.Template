using DSB.Framework.Lite.Data.EFCore.Extensions.SequentialGuid;
using SolutionName.Domain;
using SolutionName.Domain.Entities.Systems;
using SolutionName.Domain.Enums.Systems;
using SolutionName.EntityFrameworkCore.IRepositories.Systems;

namespace SolutionName.Application.Services.Systems
{
    /// <summary>
    /// 权限种子数据初始化
    /// </summary>
    public class PermissionDataSeedContributor(
        IPermissionRepository permissionRepository,
        IGuidGenerator guidGenerator) : IDataSeedContributor
    {
        /// <summary>
        /// 执行种子数据初始化（幂等）
        /// </summary>
        /// <returns></returns>
        public async Task SeedAsync()
        {
            var existing = await permissionRepository.GetAllListAsync();
            var codeToId = existing.ToDictionary(x => x.Code, x => x.Id);

            foreach (var (code, name, type, parentCode, sort) in GetSeedPermissions())
            {
                if (codeToId.ContainsKey(code)) continue;

                Guid? parentId = null;
                if (parentCode is not null && codeToId.TryGetValue(parentCode, out var pid))
                    parentId = pid;

                var id = guidGenerator.Create();
                codeToId[code] = id;

                await permissionRepository.InsertAsync(new SystemPermissionEntity
                {
                    Id = id,
                    Code = code,
                    Name = name,
                    Type = type,
                    ParentId = parentId,
                    Sort = sort
                });
            }
        }

        private static List<(string Code, string Name, PermissionType Type, string? ParentCode, int Sort)> GetSeedPermissions()
        {
            return
            [
                (SolutionNameConsts.PermissionGroups.System, "系统管理", PermissionType.Directory, null, 0),

                (SolutionNameConsts.PermissionGroups.SystemUser, "用户管理", PermissionType.Menu, SolutionNameConsts.PermissionGroups.System, 1),
                (SolutionNameConsts.PermissionCodes.UserPage, "分页查询", PermissionType.Api, SolutionNameConsts.PermissionGroups.SystemUser, 1),
                (SolutionNameConsts.PermissionCodes.UserDetail, "查看详情", PermissionType.Api, SolutionNameConsts.PermissionGroups.SystemUser, 2),
                (SolutionNameConsts.PermissionCodes.UserCreate, "新增用户", PermissionType.Api, SolutionNameConsts.PermissionGroups.SystemUser, 3),
                (SolutionNameConsts.PermissionCodes.UserUpdate, "修改用户", PermissionType.Api, SolutionNameConsts.PermissionGroups.SystemUser, 4),
                (SolutionNameConsts.PermissionCodes.UserDelete, "删除用户", PermissionType.Api, SolutionNameConsts.PermissionGroups.SystemUser, 5),
                (SolutionNameConsts.PermissionCodes.UserStatus, "修改状态", PermissionType.Api, SolutionNameConsts.PermissionGroups.SystemUser, 6),

                (SolutionNameConsts.PermissionGroups.SystemRole, "角色管理", PermissionType.Menu, SolutionNameConsts.PermissionGroups.System, 2),
                (SolutionNameConsts.PermissionCodes.RoleList, "全部角色", PermissionType.Api, SolutionNameConsts.PermissionGroups.SystemRole, 1),
                (SolutionNameConsts.PermissionCodes.RolePage, "分页查询", PermissionType.Api, SolutionNameConsts.PermissionGroups.SystemRole, 2),
                (SolutionNameConsts.PermissionCodes.RoleDetail, "查看详情", PermissionType.Api, SolutionNameConsts.PermissionGroups.SystemRole, 3),
                (SolutionNameConsts.PermissionCodes.RoleCreate, "新增角色", PermissionType.Api, SolutionNameConsts.PermissionGroups.SystemRole, 4),
                (SolutionNameConsts.PermissionCodes.RoleUpdate, "修改角色", PermissionType.Api, SolutionNameConsts.PermissionGroups.SystemRole, 5),
                (SolutionNameConsts.PermissionCodes.RoleDelete, "删除角色", PermissionType.Api, SolutionNameConsts.PermissionGroups.SystemRole, 6),
                (SolutionNameConsts.PermissionCodes.RoleStatus, "修改状态", PermissionType.Api, SolutionNameConsts.PermissionGroups.SystemRole, 7),

                (SolutionNameConsts.PermissionGroups.SystemPermission, "权限管理", PermissionType.Menu, SolutionNameConsts.PermissionGroups.System, 3),
                (SolutionNameConsts.PermissionCodes.PermissionList, "全部权限", PermissionType.Api, SolutionNameConsts.PermissionGroups.SystemPermission, 1),
                (SolutionNameConsts.PermissionCodes.PermissionDetail, "查看详情", PermissionType.Api, SolutionNameConsts.PermissionGroups.SystemPermission, 2),
                (SolutionNameConsts.PermissionCodes.PermissionCreate, "新增权限", PermissionType.Api, SolutionNameConsts.PermissionGroups.SystemPermission, 3),
                (SolutionNameConsts.PermissionCodes.PermissionUpdate, "修改权限", PermissionType.Api, SolutionNameConsts.PermissionGroups.SystemPermission, 4),
                (SolutionNameConsts.PermissionCodes.PermissionDelete, "删除权限", PermissionType.Api, SolutionNameConsts.PermissionGroups.SystemPermission, 5),
                (SolutionNameConsts.PermissionCodes.PermissionStatus, "修改状态", PermissionType.Api, SolutionNameConsts.PermissionGroups.SystemPermission, 6),

                (SolutionNameConsts.PermissionGroups.SystemFile, "文件管理", PermissionType.Menu, SolutionNameConsts.PermissionGroups.System, 4),
                (SolutionNameConsts.PermissionCodes.FileUpload, "上传文件", PermissionType.Api, SolutionNameConsts.PermissionGroups.SystemFile, 1),
                (SolutionNameConsts.PermissionCodes.FileConfig, "查看配置", PermissionType.Api, SolutionNameConsts.PermissionGroups.SystemFile, 2),
            ];
        }
    }
}
