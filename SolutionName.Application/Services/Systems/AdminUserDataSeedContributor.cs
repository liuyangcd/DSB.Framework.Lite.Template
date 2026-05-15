using DSB.Framework.Lite.Application.Password.Abstractions;
using DSB.Framework.Lite.Data.EFCore.Extensions.SequentialGuid;
using DSB.Framework.Lite.Data.EFCore.Repository;
using SolutionName.Domain;
using SolutionName.Domain.Entities.Systems;
using SolutionName.Domain.Enums;
using SolutionName.Domain.Enums.Systems;
using SolutionName.EntityFrameworkCore;
using SolutionName.EntityFrameworkCore.IRepositories.Systems;

namespace SolutionName.Application.Services.Systems
{
    /// <summary>
    /// 管理员种子数据初始化，每次启动自动同步全部权限
    /// </summary>
    public class AdminUserDataSeedContributor(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IPasswordService passwordService,
        IGuidGenerator guidGenerator,
        IUnitOfWork<SolutionNameContext> unitOfWork) : IDataSeedContributor
    {
        /// <summary>
        /// 执行种子数据初始化
        /// </summary>
        /// <returns></returns>
        public async Task SeedAsync()
        {
            var allPermissions = await permissionRepository.GetListAsync(x => x.Status == RecordStatus.Normally);
            var allPermissionIds = allPermissions.Select(p => p.Id).ToList();

            var adminRole = await EnsureAdminRoleAsync(allPermissionIds);
            await unitOfWork.SaveChangesAsync();
            var adminUser = await EnsureAdminUserAsync();
            await unitOfWork.SaveChangesAsync();
            await EnsureAdminUserRoleAsync(adminUser.Id, adminRole.Id);
        }

        /// <summary>
        /// 初始化管理员角色，并分配全部权限
        /// </summary>
        /// <param name="allPermissionIds"></param>
        /// <returns></returns>
        private async Task<SystemRoleEntity> EnsureAdminRoleAsync(List<Guid> allPermissionIds)
        {
            var adminRole = await roleRepository.GetSingleAsync(x => x.Code == SolutionNameConsts.PermissionGroups.AdminRole);
            if (adminRole is null)
            {
                adminRole = new SystemRoleEntity
                {
                    Id = guidGenerator.Create(),
                    Code = SolutionNameConsts.PermissionGroups.AdminRole,
                    Name = "管理员",
                    Status = RecordStatus.Normally
                };
                await roleRepository.InsertAsync(adminRole);
                await roleRepository.InitRolePermissionsAsync(adminRole.Id, allPermissionIds);
            }
            else
            {
                await roleRepository.UpdateRolePermissionsAsync(adminRole.Id, allPermissionIds);
            }
            return adminRole;
        }

        /// <summary>
        /// 初始化管理员用户
        /// </summary>
        /// <returns></returns>
        private async Task<SystemUserEntity> EnsureAdminUserAsync()
        {
            var adminUser = await userRepository.GetSingleAsync(x => x.Account == "admin");
            if (adminUser is null)
            {
                var (hashedPassword, salt) = passwordService.CreatePasswordHash("123456");
                adminUser = new SystemUserEntity
                {
                    Id = guidGenerator.Create(),
                    Account = "admin",
                    Name = "管理员",
                    Password = hashedPassword,
                    Salt = salt,
                    Status = RecordStatus.Normally
                };
                await userRepository.InsertAsync(adminUser);
            }
            return adminUser;
        }

        /// <summary>
        /// 初始化管理员为管理员角色
        /// </summary>
        /// <param name="adminUserId"></param>
        /// <param name="adminRoleId"></param>
        /// <returns></returns>
        private async Task EnsureAdminUserRoleAsync(Guid adminUserId, Guid adminRoleId)
        {
            var existing = await userRepository.GetUserRoleListAsync(adminUserId);
            if (!existing.Any(x => x.RoleId == adminRoleId))
            {
                await userRepository.InitUserRolesAsync(adminUserId, [adminRoleId]);
            }
        }
    }
}
