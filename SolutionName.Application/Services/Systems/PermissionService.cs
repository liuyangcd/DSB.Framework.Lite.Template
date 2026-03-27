using DSB.Framework.Lite.Core;
using DSB.Framework.Lite.Data.EFCore.Repository.Abstractions;
using SolutionName.Application.Contracts.Dtos.Systems.Permissions;
using SolutionName.Domain.Entities.Systems;
using SolutionName.Domain.Enums;
using SolutionName.Domain.Enums.Systems;
using SolutionName.EntityFrameworkCore.Repositories.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DSB.Framework.Lite.Data.EFCore.Extensions.SequentialGuid;

namespace SolutionName.Application.Services.Systems
{
    /// <summary>
    /// 权限管理服务
    /// </summary>
    public class PermissionService(
        PermissionRepository permissionRepository,
        IGuidGenerator guidGenerator) : SolutionNameApplicationService
    {
        /// <summary>
        /// 获取全部权限列表
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="code">编码</param>
        /// <param name="type">类型</param>
        /// <param name="status">状态</param>
        /// <returns></returns>
        public async Task<List<GetAllListOutputDto>> GetAllListAsync(string name, string code, PermissionType? type, RecordStatus? status)
        {
            Expression<Func<SystemPermissionEntity, bool>> predicate = p => true;
            predicate = predicate.AndIf(name.IsNotNullOrEmpty(), p => p.Name.Contains(name))
                .AndIf(code.IsNotNullOrEmpty(), p => p.Code.Contains(code))
                .AndIf(type.HasValue, p => p.Type == type!.Value)
                .AndIf(status.HasValue, p => p.Status == status!.Value);

            var permissions = await permissionRepository.GetListAsync(predicate,
                [new SortSpecAsc<SystemPermissionEntity>(p => p.Sort)],
                ExpressionGenericMapper<SystemPermissionEntity, GetAllListOutputDto>.Selector);

            #region 组装数据结构

            // 使用字典存储所有节点，键为Id
            var permissionDict = permissions.ToDictionary(n => n.Id);

            // 存储根节点和构建树结构
            var treePermissions = new List<GetAllListOutputDto>();

            foreach (var permission in permissions)
            {
                // 处理根节点（ParentId为null）
                if (!permission.ParentId.HasValue)
                {
                    treePermissions.Add(permission);
                    continue;
                }

                // 查找父节点
                if (permissionDict.TryGetValue(permission.ParentId.Value, out var parentNode))
                {
                    parentNode.Children.Add(permission);
                    parentNode.Children = [.. parentNode.Children.OrderBy(c => c.Sort)];
                }
                // 如果父节点不存在，也将其作为根节点，理论上不存在这种数据
                else
                {
                    treePermissions.Add(permission);
                }
            }

            treePermissions = [.. treePermissions.OrderBy(r => r.Sort)];

            #endregion

            return treePermissions;
        }

        /// <summary>
        /// 获取权限详情
        /// </summary>
        /// <param name="id">权限Id</param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task<GetDetailOutputDto> GetDetailAsync(Guid id)
        {
            return await permissionRepository.GetSingleAsync(id, ExpressionGenericMapper<SystemPermissionEntity, GetDetailOutputDto>.Selector) ?? throw new BusinessException("权限不存在");
        }

        /// <summary>
        /// 新增权限
        /// </summary>
        /// <param name="inputDto"></param>
        /// <returns></returns>
        public async Task<bool> CreateAsync(CreateInputDto inputDto)
        {
            if (await permissionRepository.IsExistsAsync(x => x.Code == inputDto.Code)) throw new BusinessException("编码已存在");

            var permission = inputDto.TransObject<CreateInputDto, SystemPermissionEntity>();

            permission.Id = guidGenerator.Create();

            await permissionRepository.InsertAsync(permission);

            return true;
        }

        /// <summary>
        /// 修改权限
        /// </summary>
        /// <param name="inputDto"></param>
        /// <returns></returns>
        public async Task<bool> UpdateAsync(UpdateInputDto inputDto)
        {
            var permission = await permissionRepository.GetSingleAsync(inputDto.Id) ?? throw new BusinessException("权限不存在");

            if (await permissionRepository.IsExistsAsync(x => x.Code == inputDto.Code && x.Id != inputDto.Id)) throw new BusinessException("编码已存在");

            inputDto.TransObject(permission);

            await permissionRepository.UpdateAsync(permission);

            return true;
        }

        /// <summary>
        /// 删除权限
        /// </summary>
        /// <param name="id">权限Id</param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task<bool> DeleteAsync(Guid id)
        {
            var role = await permissionRepository.GetSingleAsync(id) ?? throw new BusinessException("权限不存在");
            return await permissionRepository.DeprecateAsync(role);
        }

        /// <summary>
        /// 修改权限状态
        /// </summary>
        /// <param name="inputDto"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task<bool> UpdateStatusAsync(UpdateStatusInputDto inputDto)
        {
            if (!await permissionRepository.IsExistsAsync(x => x.Id == inputDto.Id)) throw new BusinessException("权限不存在");
            return await permissionRepository.ExecuteUpdateAsync(inputDto.Id, x => x.SetProperty(y => y.Status, inputDto.Status));
        }
    }
}