using DSB.Framework.Lite.Data.EFCore.Extensions.SequentialGuid;
using DSB.Framework.Lite.Data.EFCore.Repository;
using SolutionName.Domain;
using SolutionName.Domain.Entities.Systems;
using SolutionName.Domain.Enums.Systems;
using SolutionName.EntityFrameworkCore;
using SolutionName.EntityFrameworkCore.IRepositories.Systems;

namespace SolutionName.Application.Services.Systems
{
    /// <summary>
    /// 权限种子数据初始化
    /// </summary>
    public class PermissionDataSeedContributor(
        IPermissionRepository permissionRepository,
        IGuidGenerator guidGenerator,
        IUnitOfWork<SolutionNameContext> unitOfWork) : IDataSeedContributor
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

            await unitOfWork.SaveChangesAsync();
        }

        private static List<(string Code, string Name, PermissionType Type, string? ParentCode, int Sort)> GetSeedPermissions()
        {
            return
            [
                (PermissionCodes.System, "系统管理", PermissionType.Directory, null, 1),

                (PermissionCodes.User.Menu, "用户管理", PermissionType.Menu, PermissionCodes.System, 1),
                (PermissionCodes.User.Page, "分页查询", PermissionType.Api, PermissionCodes.User.Menu, 1),
                (PermissionCodes.User.Detail, "查看详情", PermissionType.Api, PermissionCodes.User.Menu, 2),
                (PermissionCodes.User.Create, "新增用户", PermissionType.Api, PermissionCodes.User.Menu, 3),
                (PermissionCodes.User.Update, "修改用户", PermissionType.Api, PermissionCodes.User.Menu, 4),
                (PermissionCodes.User.Delete, "删除用户", PermissionType.Api, PermissionCodes.User.Menu, 5),
                (PermissionCodes.User.Status, "修改状态", PermissionType.Api, PermissionCodes.User.Menu, 6),

                (PermissionCodes.Role.Menu, "角色管理", PermissionType.Menu, PermissionCodes.System, 2),
                (PermissionCodes.Role.List, "全部角色", PermissionType.Api, PermissionCodes.Role.Menu, 1),
                (PermissionCodes.Role.Page, "分页查询", PermissionType.Api, PermissionCodes.Role.Menu, 2),
                (PermissionCodes.Role.Detail, "查看详情", PermissionType.Api, PermissionCodes.Role.Menu, 3),
                (PermissionCodes.Role.Create, "新增角色", PermissionType.Api, PermissionCodes.Role.Menu, 4),
                (PermissionCodes.Role.Update, "修改角色", PermissionType.Api, PermissionCodes.Role.Menu, 5),
                (PermissionCodes.Role.Delete, "删除角色", PermissionType.Api, PermissionCodes.Role.Menu, 6),
                (PermissionCodes.Role.Status, "修改状态", PermissionType.Api, PermissionCodes.Role.Menu, 7),

                (PermissionCodes.PermissionManage.Menu, "权限管理", PermissionType.Menu, PermissionCodes.System, 3),
                (PermissionCodes.PermissionManage.List, "全部权限", PermissionType.Api, PermissionCodes.PermissionManage.Menu, 1),
                (PermissionCodes.PermissionManage.Detail, "查看详情", PermissionType.Api, PermissionCodes.PermissionManage.Menu, 2),
                (PermissionCodes.PermissionManage.Create, "新增权限", PermissionType.Api, PermissionCodes.PermissionManage.Menu, 3),
                (PermissionCodes.PermissionManage.Update, "修改权限", PermissionType.Api, PermissionCodes.PermissionManage.Menu, 4),
                (PermissionCodes.PermissionManage.Delete, "删除权限", PermissionType.Api, PermissionCodes.PermissionManage.Menu, 5),
                (PermissionCodes.PermissionManage.Status, "修改状态", PermissionType.Api, PermissionCodes.PermissionManage.Menu, 6),

                (PermissionCodes.File.Menu, "文件管理", PermissionType.Menu, PermissionCodes.System, 4),
                (PermissionCodes.File.Upload, "上传文件", PermissionType.Api, PermissionCodes.File.Menu, 1),
                (PermissionCodes.File.Config, "查看配置", PermissionType.Api, PermissionCodes.File.Menu, 2),
            ];
        }
    }
}
