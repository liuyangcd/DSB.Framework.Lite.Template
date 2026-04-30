using DSB.Framework.Lite.Application.Password.Abstractions;
using DSB.Framework.Lite.Core;
using DSB.Framework.Lite.Data.EFCore.Repository.Abstractions;
using Microsoft.Extensions.Logging;
using SolutionName.Application.Contracts.Bos.Systems.Users;
using SolutionName.Application.Contracts.Dtos.Systems.Users;
using SolutionName.Application.Contracts.UserContext;
using SolutionName.Domain.Entities.Systems;
using SolutionName.Domain.Enums;
using SolutionName.EntityFrameworkCore.Repositories.Systems;
using System.Linq.Expressions;

namespace SolutionName.Application.Services.Systems
{
    /// <summary>
    /// 鉴权服务
    /// </summary>
    public class AuthService(
        IPasswordService passwordService,
        UserRepository userRepository) : SolutionNameApplicationService
    {
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="inputDto"></param>
        /// <param name="tokenExpireTimeSpan">登录token过期时间间隔</param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task<JwtUserContext> LoginAsync(LoginInputDto inputDto, TimeSpan tokenExpireTimeSpan)
        {
            var user = await userRepository.GetSingleAsync(x => x.Account == inputDto.Account,
                                                           ExpressionGenericMapper<SystemUserEntity, LoginOutputBo>.Selector) ?? throw new BusinessException("用户不存在");

            if (user.Status == RecordStatus.Disabled) throw new BusinessException("用户已被禁用");
            if (user.Salt.IsNullOrEmpty() || user.Password.IsNullOrEmpty()) throw new BusinessException("用户未设置密码");
            if (!passwordService.VerifyPassword(inputDto.Password, user.Password, user.Salt)) throw new BusinessException("密码错误");

            // 获取用户权限信息并存储到缓存
            var permissionCodes = await userRepository.GetUserPermissionCodesAsync(user.Id);
            await UserPermissionStorage.SetAsync(user.Id, permissionCodes, tokenExpireTimeSpan);

            return user.TransObject<LoginOutputBo, JwtUserContext>();
        }

        /// <summary>
        /// 获取用户登录信息详情，注意：包含权限信息
        /// </summary>
        /// <param name="id">用户Id</param>
        /// <param name="tokenExpireTimeSpan">登录token过期时间间隔</param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task<InfoOutputDto> GetInfoAsync(Guid id, TimeSpan tokenExpireTimeSpan)
        {
            var info = await userRepository.GetSingleAsync(id, ExpressionGenericMapper<SystemUserEntity, InfoOutputDto>.Selector) ?? throw new BusinessException("用户不存在");
            var permissionCodes = await UserPermissionStorage.GetAsync(id);
            if (permissionCodes is null)
            {
                permissionCodes = await userRepository.GetUserPermissionCodesAsync(id);
                await UserPermissionStorage.SetAsync(id, permissionCodes, tokenExpireTimeSpan);
            }
            info.PermissionCodes = permissionCodes;
            return info;
        }
    }
}
