using DSB.Framework.Lite.Tools.Captcha;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionName.Application.Services.Systems
{
    /// <summary>
    /// 验证码管理服务
    /// </summary>
    public static class CaptchaService
    {
        /// <summary>
        /// Redis缓存Key前缀
        /// </summary>
        private const string cacheKeyPrefix = "SolutionName:Captcha:";

        /// <summary>
        /// 创建验证码
        /// </summary>
        /// <param name="expire">过期时间，默认1分钟</param>
        /// <returns></returns>
        public static async Task<(string imageBase64, Guid id)> Create(TimeSpan? expire = null)
        {
            expire ??= TimeSpan.FromMinutes(1);
            var (imageBase64, code) = CaptchaGenerator.CreateImageBase64(4, true);
            var id = Guid.NewGuid();
            var cacheKey = $"{cacheKeyPrefix}{id}";
            await RedisHelper.SetAsync(cacheKey, code, expire.Value);
            return (imageBase64, id);
        }

        /// <summary>
        /// 校验验证码
        /// </summary>
        /// <param name="id">验证码Id</param>
        /// <param name="value">验证码值</param>
        /// <returns></returns>
        public static async Task<bool> Verify(Guid id, string value)
        {
            var cacheKey = $"{cacheKeyPrefix}{id}";
            var cachedCode = await RedisHelper.GetAsync<string>(cacheKey);
            if (cachedCode is null)
            {
                return false;
            }
            var isValid = string.Equals(cachedCode, value, StringComparison.OrdinalIgnoreCase);
            if (isValid)
            {
                await RedisHelper.DelAsync(cacheKey);
            }
            return isValid;
        }
    }
}
