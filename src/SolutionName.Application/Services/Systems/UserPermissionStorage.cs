using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionName.Application.Services.Systems
{
    /// <summary>
    /// 用户权限码分布式缓存管理
    /// </summary>
    public static class UserPermissionStorage
    {
        /// <summary>
        /// 获取用户权限码集合的缓存Key
        /// </summary>
        /// <param name="id">用户Id</param>
        /// <returns></returns>
        private static string GetKey(Guid id) => $"SolutionName:Auth:UserPermissionCodes:{id}";

        /// <summary>
        /// 设置用户具有的权限码集合到分布式缓存
        /// </summary>
        /// <param name="id">用户Id</param>
        /// <param name="permissionCodes">权限码集合</param>
        /// <param name="expireTime">登录过期时间，默认不过期</param>
        /// <returns></returns>
        public static async Task<bool> SetAsync(Guid id, IEnumerable<string> permissionCodes, DateTime? expireTime = null)
        {
            if (expireTime.HasValue)
            {
                var timesSpanExpire = expireTime.Value - DateTime.Now;
                return await RedisHelper.SetAsync(GetKey(id), permissionCodes.Distinct(), timesSpanExpire);
            }
            else
            {
                return await RedisHelper.SetAsync(GetKey(id), permissionCodes.Distinct());
            }
        }

        /// <summary>
        /// 设置用户具有的权限码集合到分布式缓存
        /// </summary>
        /// <param name="id">用户Id</param>
        /// <param name="permissionCodes">权限码集合</param>
        /// <param name="expireTime">登录过期时间，默认不过期</param>
        /// <returns></returns>
        public static async Task<bool> SetAsync(Guid id, IEnumerable<string> permissionCodes, TimeSpan? expireTimeSpan = null)
        {
            if (expireTimeSpan.HasValue)
            {
                return await RedisHelper.SetAsync(GetKey(id), permissionCodes.Distinct(), expireTimeSpan.Value);
            }
            else
            {
                return await RedisHelper.SetAsync(GetKey(id), permissionCodes.Distinct());
            }
        }

        /// <summary>
        /// 从分布式缓存获取用户具有的权限码集合
        /// </summary>
        /// <param name="id">用户Id</param>
        /// <returns></returns>
        public static async Task<List<string>> GetAsync(Guid id)
        {
            return await RedisHelper.GetAsync<List<string>>(GetKey(id));
        }
    }
}
