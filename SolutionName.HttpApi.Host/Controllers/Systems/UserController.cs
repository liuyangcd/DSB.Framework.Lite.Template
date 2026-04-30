using DSB.Framework.Lite.Core;
using DSB.Framework.Lite.Data.EFCore.Repository.Abstractions;
using DSB.Framework.Lite.WebApi.Extensions.Http.JwtBearer;
using DSB.Framework.Lite.WebApi.Extensions.SwaggerConfig.Attributes;
using Microsoft.AspNetCore.Mvc;
using SolutionName.Application.Contracts.Dtos.Systems.Users;
using SolutionName.Application.Services.Systems;

namespace SolutionName.HttpApi.Host.Controllers.Systems
{
    /// <summary>
    /// 用户管理
    /// </summary>
    [ApiExplorer(ModuleEnum.User)]
    public class UserController(
        IJwtService jwtService,
        UserService userService) : ManagerControllerBase(jwtService)
    {
        #region 用户管理

        /// <summary>
        /// 分页获取用户列表
        /// </summary>
        /// <param name="inputDto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<PagedResult<GetPageListOutputDto>>> GetPageList(GetPageListInputDto inputDto)
        {
            return ApiResult<PagedResult<GetPageListOutputDto>>.GetSuccess(await userService.GetPageListAsync(inputDto));
        }

        /// <summary>
        /// 获取用户详情
        /// </summary>
        /// <param name="id">用户Id</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<GetDetailOutputDto>> GetDetail(Guid id)
        {
            return ApiResult<GetDetailOutputDto>.GetSuccess(await userService.GetDetailAsync(id));
        }

        /// <summary>
        /// 新增用户
        /// </summary>
        /// <param name="inputDto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<bool>> Create(CreateInputDto inputDto)
        {
            return ApiResult<bool>.GetSuccess(await userService.CreateAsync(inputDto, UserContext?.Id));
        }

        /// <summary>
        /// 修改用户
        /// </summary>
        /// <param name="inputDto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<bool>> Update(UpdateInputDto inputDto)
        {
            return ApiResult<bool>.GetSuccess(await userService.UpdateAsync(inputDto));
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="id">用户Id</param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<ApiResult<bool>> Delete(Guid id)
        {
            return ApiResult<bool>.GetSuccess(await userService.DeleteAsync(id));
        }

        /// <summary>
        /// 修改用户状态
        /// </summary>
        /// <param name="inputDto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<bool>> UpdateStatus(UpdateStatusInputDto inputDto)
        {
            return ApiResult<bool>.GetSuccess(await userService.UpdateStatusAsync(inputDto));
        }

        #endregion
    }
}