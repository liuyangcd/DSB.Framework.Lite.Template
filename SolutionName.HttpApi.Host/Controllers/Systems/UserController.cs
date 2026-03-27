using DSB.Framework.Lite.Core;
using DSB.Framework.Lite.Data.EFCore.Repository.Abstractions;
using DSB.Framework.Lite.WebApi.Extensions.Http.JwtBearer;
using DSB.Framework.Lite.WebApi.Extensions.SwaggerConfig.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolutionName.Application.Contracts.Dtos.Systems.Users;
using SolutionName.Application.Contracts.UserContext;
using SolutionName.Application.Services.Systems;

namespace SolutionName.HttpApi.Host.Controllers.Systems
{
    /// <summary>
    /// 用户管理
    /// </summary>
    [ApiExplorer(ModuleEnum.User)]
    public class UserController(
        IJwtService jwtService,
        UserService userService,
        IWebHostEnvironment webHostEnvironment) : ManagerControllerBase(jwtService)
    {
        #region 鉴权管理

        /// <summary>
        /// 获取验证码
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<GetCaptchaOutputDto> GetCaptcha()
        {
            var (imageBase64, id) = await CaptchaService.Create();
            return new GetCaptchaOutputDto()
            {
                Id = id,
                ImageBase64 = $"data:image/png;base64,{imageBase64}"
            };
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ApiResult<JwtTokenModel>> Login(LoginInputDto inputDto)
        {
            // 1，密码是否需要加密传参
            // 2，需要新增登录失败次数限制和请求速率限制功能

            // 只有非开发环境才校验验证码
            if (!webHostEnvironment.IsDevelopment() && !await CaptchaService.Verify(inputDto.CaptchaId, inputDto.CaptchaCode))
            {
                throw new BusinessException("验证码错误");
            }

            var userContext = await userService.LoginAsync(inputDto, jwtService.ExpireTimeSpan);

            var token = jwtService.CreateToken<JwtUserContext, Guid>(userContext);

            return ApiResult<JwtTokenModel>.GetSuccess(token);
        }

        /// <summary>
        /// 获取当前登录用户信息，含权限信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<InfoOutputDto>> Info()
        {
            var info = await userService.GetInfoAsync(UserContext.Id, jwtService.ExpireTimeSpan);
            return ApiResult<InfoOutputDto>.GetSuccess(info);
        }

        #endregion

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