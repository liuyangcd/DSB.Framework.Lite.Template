using DSB.Framework.Lite.Core;
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
    /// 鉴权管理
    /// </summary>
    [ApiExplorer(ModuleEnum.Auth)]
    public class AuthController(
        IJwtService jwtService,
        AuthService authService,
        IWebHostEnvironment webHostEnvironment) : ManagerControllerBase(jwtService)
    {
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
            // 只有非开发环境才校验验证码
            if (!webHostEnvironment.IsDevelopment() && !await CaptchaService.Verify(inputDto.CaptchaId, inputDto.CaptchaCode))
            {
                throw new BusinessException("验证码错误");
            }

            var userContext = await authService.LoginAsync(inputDto, jwtService.ExpireTimeSpan);

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
            var info = await authService.GetInfoAsync(UserContext.Id, jwtService.ExpireTimeSpan);
            return ApiResult<InfoOutputDto>.GetSuccess(info);
        }
    }
}
