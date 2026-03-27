using DSB.Framework.Lite.Core;
using DSB.Framework.Lite.Data.EFCore.Repository.Abstractions;
using DSB.Framework.Lite.WebApi.Extensions.Http.JwtBearer;
using DSB.Framework.Lite.WebApi.Extensions.SwaggerConfig.Attributes;
using Microsoft.AspNetCore.Mvc;
using SolutionName.Application.Contracts.Dtos.Systems.Roles;
using SolutionName.Application.Services.Systems;

namespace SolutionName.HttpApi.Host.Controllers.Systems
{
    /// <summary>
    /// 角色管理
    /// </summary>
    [ApiExplorer(ModuleEnum.Role)]
    public class RoleController(
        IJwtService jwtService,
        RoleService roleService) : ManagerControllerBase(jwtService)
    {
        /// <summary>
        /// 获取当前所有可用角色信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<List<GetAllRolesOutputDto>>> GetAllRoles()
        {
            return ApiResult<List<GetAllRolesOutputDto>>.GetSuccess(await roleService.GetAllRolesAsync());
        }

        /// <summary>
        /// 获取角色分页列表
        /// </summary>
        /// <param name="inputDto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<PagedResult<GetPageListOutputDto>>> GetPageList(GetPageListInputDto inputDto)
        {
            return ApiResult<PagedResult<GetPageListOutputDto>>.GetSuccess(await roleService.GetPageListAsync(inputDto));
        }

        /// <summary>
        /// 获取角色详情
        /// </summary>
        /// <param name="id">角色Id</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<GetDetailOutputDto>> GetDetail(Guid id)
        {
            return ApiResult<GetDetailOutputDto>.GetSuccess(await roleService.GetDetailAsync(id));
        }

        /// <summary>
        /// 新增角色
        /// </summary>
        /// <param name="inputDto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<bool>> Create(CreateInputDto inputDto)
        {
            return ApiResult<bool>.GetSuccess(await roleService.CreateAsync(inputDto, UserContext.Id));
        }

        /// <summary>
        /// 修改角色
        /// </summary>
        /// <param name="inputDto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<bool>> Update(UpdateInputDto inputDto)
        {
            return ApiResult<bool>.GetSuccess(await roleService.UpdateAsync(inputDto));
        }

        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="id">角色Id</param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<ApiResult<bool>> Delete(Guid id)
        {
            return ApiResult<bool>.GetSuccess(await roleService.DeleteAsync(id));
        }

        /// <summary>
        /// 修改角色状态
        /// </summary>
        /// <param name="inputDto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<bool>> UpdateStatusAsync(UpdateStatusInputDto inputDto)
        {
            return ApiResult<bool>.GetSuccess(await roleService.UpdateStatusAsync(inputDto));
        }
    }
}