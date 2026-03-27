using DSB.Framework.Lite.Core;
using DSB.Framework.Lite.Data.EFCore.Extensions.SequentialGuid;
using DSB.Framework.Lite.Data.EFCore.Repository;
using DSB.Framework.Lite.Data.EFCore.Repository.Abstractions;
using SolutionName.Application.Contracts.Dtos.Systems.Roles;
using SolutionName.Domain.Entities.Systems;
using SolutionName.Domain.Enums;
using SolutionName.EntityFrameworkCore;
using SolutionName.EntityFrameworkCore.Repositories.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SolutionName.Application.Services.Systems
{
    /// <summary>
    /// 角色管理服务
    /// </summary>
    public class RoleService(
        RoleRepository roleRepository,
        IGuidGenerator guidGenerator) : SolutionNameApplicationService
    {

        /// <summary>
        /// 获取当前所有可用角色信息
        /// </summary>
        /// <returns></returns>
        public async Task<List<GetAllRolesOutputDto>> GetAllRolesAsync()
        {
            return await roleRepository.GetListAsync(x => x.Status == RecordStatus.Normally,
                                                     [new SortSpecAsc<SystemRoleEntity>(x => x.Sort)],
                                                     ExpressionGenericMapper<SystemRoleEntity, GetAllRolesOutputDto>.Selector);
        }

        /// <summary>
        /// 获取角色分页列表
        /// </summary>
        /// <param name="inputDto"></param>
        /// <returns></returns>
        public async Task<PagedResult<GetPageListOutputDto>> GetPageListAsync(GetPageListInputDto inputDto)
        {
            Expression<Func<SystemRoleEntity, bool>> predicate = x => true;

            predicate = predicate.AndIf(inputDto.Code.IsNotNullOrEmpty(), x => x.Code.Contains(inputDto.Code!))
                .AndIf(inputDto.Name.IsNotNullOrEmpty(), x => x.Name.Contains(inputDto.Name!))
                .AndIf(inputDto.Status.HasValue, x => x.Status == inputDto.Status!.Value);

            var roleList = await roleRepository.GetPagedListAsync(inputDto,
                                            predicate,
                                            [
                                                new SortSpecAsc<SystemRoleEntity>(x => x.Sort),
                                                new SortSpecDesc<SystemRoleEntity>(x => x.CreateDateAt)
                                            ],
                                            ExpressionGenericMapper<SystemRoleEntity, GetPageListOutputDto>.Selector);
            return roleList;
        }

        /// <summary>
        /// 获取角色详情
        /// </summary>
        /// <param name="id">角色Id</param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task<GetDetailOutputDto> GetDetailAsync(Guid id)
        {
            return await roleRepository.GetSingleAsync(x => x.Id == id, ExpressionGenericMapper<SystemRoleEntity, GetDetailOutputDto>.Selector) ?? throw new BusinessException("角色不存在");
        }

        /// <summary>
        /// 新增角色
        /// </summary>
        /// <param name="inputDto"></param>
        /// <param name="createdUserId"></param>
        /// <returns></returns>
        public async Task<bool> CreateAsync(CreateInputDto inputDto, Guid createdUserId)
        {
            // 此处是因为数据库有唯一索引约束，但为了更友好提示，故在此处做判断；如果没有数据库没有唯一约束，直接判断会有并发问题，需要加锁处理
            if (await roleRepository.IsExistsAsync(x => x.Code == inputDto.Code)) throw new BusinessException("编码已存在");

            // 自动映射属性，需要属性名称和类型一致
            var role = inputDto.TransObject<CreateInputDto, SystemRoleEntity>();

            role.Id = guidGenerator.Create();
            role.CreatedUserId = createdUserId;

            await roleRepository.InsertAsync(role);

            await roleRepository.InitRolePermissionsAsync(role.Id, inputDto.PermissionIds);

            return true;
        }

        /// <summary>
        /// 修改角色
        /// </summary>
        /// <param name="inputDto"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task<bool> UpdateAsync(UpdateInputDto inputDto)
        {
            var role = await roleRepository.GetSingleAsync(inputDto.Id) ?? throw new BusinessException("角色不存在");

            if (await roleRepository.IsExistsAsync(x => x.Code == inputDto.Code && x.Id != inputDto.Id)) throw new BusinessException("编码已存在");

            // 自动映射更新属性，需要属性名称和类型一致
            inputDto.TransObject(role);

            await roleRepository.UpdateAsync(role);

            await roleRepository.UpdateRolePermissionsAsync(role.Id, inputDto.PermissionIds);

            return true;
        }

        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="id">角色Id</param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task<bool> DeleteAsync(Guid id)
        {
            var role = await roleRepository.GetSingleAsync(id) ?? throw new BusinessException("角色不存在");
            return await roleRepository.DeprecateAsync(role);
        }

        /// <summary>
        /// 修改角色状态
        /// </summary>
        /// <param name="inputDto"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task<bool> UpdateStatusAsync(UpdateStatusInputDto inputDto)
        {
            if (!await roleRepository.IsExistsAsync(x => x.Id == inputDto.Id)) throw new BusinessException("角色不存在");
            return await roleRepository.ExecuteUpdateAsync(inputDto.Id, x => x.SetProperty(y => y.Status, inputDto.Status));
        }
    }
}
