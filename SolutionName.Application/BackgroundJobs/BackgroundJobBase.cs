using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionName.Application.BackgroundJobs
{
    /// <summary>
    /// 后台任务抽象基类
    /// </summary>
    public abstract class BackgroundJobBase : SolutionNameApplicationService
    {
        /// <summary>
        /// 默认执行的任务方法
        /// </summary>
        /// <param name="ct">取消令牌：固定传入CancellationToken.None，Hanfire框架会自动构建</param>
        /// <returns></returns>
        public abstract Task ExecuteAsync(CancellationToken ct);
    }

    /// <summary>
    /// 带入参的后台任务抽象基类
    /// </summary>
    public abstract class BackgroundJobBase<T> : SolutionNameApplicationService
    {
        /// <summary>
        /// 默认执行的任务方法
        /// </summary>
        /// <param name="parameter">入参</param>
        /// <param name="ct">取消令牌：固定传入CancellationToken.None，Hanfire框架会自动构建</param>
        /// <returns></returns>
        public abstract Task ExecuteAsync(T parameter, CancellationToken ct);
    }
}
