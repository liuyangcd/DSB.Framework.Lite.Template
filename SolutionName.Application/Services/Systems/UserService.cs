using DSB.Framework.Lite.Application.Password.Abstractions;
using DSB.Framework.Lite.Core;
using DSB.Framework.Lite.Data.EFCore.Extensions.SequentialGuid;
using DSB.Framework.Lite.Data.EFCore.Repository.Abstractions;
using Microsoft.Extensions.Logging;
using SolutionName.Application.Contracts.Bos.Systems.Users;
using SolutionName.Application.Contracts.Dtos.Systems.Users;
using SolutionName.Domain.Entities.Systems;
using SolutionName.Domain.Enums;
using SolutionName.EntityFrameworkCore.IRepositories.Systems;
using System.Linq.Expressions;

namespace SolutionName.Application.Services.Systems
{
    /// <summary>
    /// 用户服务
    /// </summary>
    public class UserService(
        ILogger<UserService> logger,
        IGuidGenerator guidGenerator,
        IPasswordService passwordService,
        IUserRepository userRepository) : SolutionNameApplicationService
    {

        #region 用户管理
        /// <summary>
        /// 分页获取用户列表
        /// </summary>
        /// <param name="inputDto"></param>
        /// <returns></returns>
        public async Task<PagedResult<GetPageListOutputDto>> GetPageListAsync(GetPageListInputDto inputDto)
        {
            Expression<Func<SystemUserEntity, bool>> predicate = x => true;

            predicate = predicate.AndIf(inputDto.Name.IsNotNullOrEmpty(), x => x.Name.Contains(inputDto.Name!))
                .AndIf(inputDto.Account.IsNotNullOrEmpty(), x => x.Account.Contains(inputDto.Account!))
                .AndIf(inputDto.Phone.IsNotNullOrEmpty(), x => x.Phone!.Contains(inputDto.Phone!))
                .AndIf(inputDto.Status.HasValue, x => x.Status == inputDto.Status!.Value);

            var userList = await userRepository.GetPagedListAsync(inputDto,
                                              predicate,
                                              [new SortSpecDesc<SystemUserEntity>(x => x.CreateDateAt)],
                                              ExpressionGenericMapper<SystemUserEntity, GetPageListOutputDto>.Selector);
            return userList;
        }

        /// <summary>
        /// 获取用户详情
        /// </summary>
        /// <param name="id">用户Id</param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task<GetDetailOutputDto> GetDetailAsync(Guid id)
        {
            var user = await userRepository.GetSingleAsync(id, ExpressionGenericMapper<SystemUserEntity, GetDetailOutputDto>.Selector) ?? throw new BusinessException("用户不存在");
            var roleIds = await userRepository.GetUserRoleListAsync(id, x => x.RoleId);
            user.RoleIds = roleIds;
            return user;
        }

        /// <summary>
        /// 新增用户
        /// </summary>
        /// <param name="inputDto"></param>
        /// <param name="createdUserId"></param>
        /// <returns></returns>
        public async Task<bool> CreateAsync(CreateInputDto inputDto, Guid? createdUserId)
        {
            if (inputDto.PasswordText.IsNullOrEmpty()) throw new BusinessException("密码不能为空");

            // 此处是因为数据库有唯一索引约束，但为了更友好提示，故在此处做判断；如果没有数据库没有唯一约束，直接判断会有并发问题，需要加锁处理
            if (await userRepository.IsExistsAsync(x => x.Account == inputDto.Account)) throw new BusinessException("账号已存在");

            // 自动映射属性，需要属性名称和类型一致
            var user = inputDto.TransObject<CreateInputDto, SystemUserEntity>();

            user.Id = guidGenerator.Create();
            user.CreatedUserId = createdUserId ?? user.Id;
            var (hashedPassword, salt) = passwordService.CreatePasswordHash(inputDto.PasswordText);
            user.Password = hashedPassword;
            user.Salt = salt;

            await userRepository.InsertAsync(user);

            await userRepository.InitUserRolesAsync(user.Id, inputDto.RoleIds);

            return true;
        }

        /// <summary>
        /// 修改用户
        /// </summary>
        /// <param name="inputDto"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task<bool> UpdateAsync(UpdateInputDto inputDto)
        {
            var user = await userRepository.GetSingleAsync(inputDto.Id) ?? throw new BusinessException("用户不存在");

            // 自动映射更新属性，需要属性名称和类型一致
            inputDto.TransObject(user);

            // 密码不为空则更新密码
            if (inputDto.PasswordText.IsNotNullOrEmpty())
            {
                var (hashedPassword, salt) = passwordService.CreatePasswordHash(inputDto.PasswordText);
                user.Password = hashedPassword;
                user.Salt = salt;
            }

            await userRepository.UpdateAsync(user);

            await userRepository.UpdateUserRolesAsync(user.Id, inputDto.RoleIds);

            return true;
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="id">用户Id</param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task<bool> DeleteAsync(Guid id)
        {
            var user = await userRepository.GetSingleAsync(id) ?? throw new BusinessException("用户不存在");
            return await userRepository.DeprecateAsync(user);
        }

        /// <summary>
        /// 修改用户状态
        /// </summary>
        /// <param name="inputDto"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task<bool> UpdateStatusAsync(UpdateStatusInputDto inputDto)
        {
            if (!await userRepository.IsExistsAsync(x => x.Id == inputDto.Id)) throw new BusinessException("用户不存在");
            return await userRepository.ExecuteUpdateAsync(inputDto.Id, x => x.SetProperty(y => y.Status, inputDto.Status));
        }
        #endregion

    }
}
