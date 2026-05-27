using DSB.Framework.Lite.Core;
using DSB.Framework.Lite.WebApi.Extensions.Http.JwtBearer;
using DSB.Framework.Lite.WebApi.Extensions.SwaggerConfig.Attributes;
using Microsoft.AspNetCore.Mvc;
using SolutionName.Application.Contracts.Dtos.Systems.Permissions;
using SolutionName.Application.Services.Systems;
using SolutionName.Domain;
using SolutionName.Domain.Enums;
using SolutionName.Domain.Enums.Systems;

namespace SolutionName.HttpApi.Host.Controllers.Systems
{
    /// <summary>
    /// 权限管理
    /// </summary>
    [ApiExplorer(ModuleEnum.Permission)]
    public class PermissionController(
        IJwtService jwtService,
        PermissionService permissionService) : ManagerControllerBase(jwtService)
    {
        /// <summary>
        /// 获取全部权限列表
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="code">编码</param>
        /// <param name="type">类型</param>
        /// <param name="status">状态:1表示可用的权限</param>
        /// <returns></returns>
        [HttpGet]
        [IdentifierAuthorize(SolutionNameConsts.PermissionCodes.PermissionList)]
        public async Task<ApiResult<List<GetAllListOutputDto>>> GetAllList(string? name, string? code, PermissionType? type, RecordStatus? status)
        {
            return ApiResult<List<GetAllListOutputDto>>.GetSuccess(await permissionService.GetAllListAsync(name, code, type, status));
        }

        /// <summary>
        /// 获取权限详情
        /// </summary>
        /// <param name="id">权限Id</param>
        /// <returns></returns>
        [HttpGet]
        [IdentifierAuthorize(SolutionNameConsts.PermissionCodes.PermissionDetail)]
        public async Task<ApiResult<GetDetailOutputDto>> GetDetail(Guid id)
        {
            return ApiResult<GetDetailOutputDto>.GetSuccess(await permissionService.GetDetailAsync(id));
        }

        /// <summary>
        /// 新增权限
        /// </summary>
        /// <param name="inputDto"></param>
        /// <returns></returns>
        [HttpPost]
        [IdentifierAuthorize(SolutionNameConsts.PermissionCodes.PermissionCreate)]
        public async Task<ApiResult<bool>> Create(CreateInputDto inputDto)
        {
            return ApiResult<bool>.GetSuccess(await permissionService.CreateAsync(inputDto));
        }

        /// <summary>
        /// 修改权限
        /// </summary>
        /// <param name="inputDto"></param>
        /// <returns></returns>
        [HttpPost]
        [IdentifierAuthorize(SolutionNameConsts.PermissionCodes.PermissionUpdate)]
        public async Task<ApiResult<bool>> Update(UpdateInputDto inputDto)
        {
            return ApiResult<bool>.GetSuccess(await permissionService.UpdateAsync(inputDto));
        }

        /// <summary>
        /// 删除权限
        /// </summary>
        /// <param name="id">权限Id</param>
        /// <returns></returns>
        [HttpDelete]
        [IdentifierAuthorize(SolutionNameConsts.PermissionCodes.PermissionDelete)]
        public async Task<ApiResult<bool>> Delete(Guid id)
        {
            return ApiResult<bool>.GetSuccess(await permissionService.DeleteAsync(id));
        }

        /// <summary>
        /// 修改权限状态
        /// </summary>
        /// <param name="inputDto"></param>
        /// <returns></returns>
        [HttpPost]
        [IdentifierAuthorize(SolutionNameConsts.PermissionCodes.PermissionStatus)]
        public async Task<ApiResult<bool>> UpdateStatus(UpdateStatusInputDto inputDto)
        {
            return ApiResult<bool>.GetSuccess(await permissionService.UpdateStatusAsync(inputDto));
        }
    }
}
