using DSB.Framework.Lite.Core;
using DSB.Framework.Lite.Data.EFCore.Repository;
using DSB.Framework.Lite.Data.EFCore.Repository.WebApi.Attributes;
using Microsoft.AspNetCore.Mvc;
using SolutionName.Domain.Entities.Systems;
using SolutionName.EntityFrameworkCore;

namespace SolutionName.Tests.Integration.MySql;

/// <summary>
/// 用于验证工作单元过滤器的探针控制器。
/// </summary>
[ApiController]
[Route("api/unit-of-work-probe")]
public sealed class UnitOfWorkProbeController(
    IEntityFrameworkCoreRepository<SolutionNameContext, SystemUserEntity, Guid> userRepository,
    IUnitOfWork<SolutionNameContext> unitOfWork) : ControllerBase
{
    /// <summary>
    /// 只调用仓储新增，不手动保存；过滤器应在 Action 返回后自动提交。
    /// </summary>
    /// <param name="account">账户</param>
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromQuery] string account)
    {
        await userRepository.InsertAsync(NewUser(account));
        return Ok();
    }

    /// <summary>
    /// 新增后抛出异常；过滤器不应提交待保存的变更。
    /// </summary>
    /// <param name="account">账户</param>
    [HttpPost("create-then-throw")]
    public async Task<IActionResult> CreateThenThrow([FromQuery] string account)
    {
        await userRepository.InsertAsync(NewUser(account));
        throw new InvalidOperationException("模拟业务异常");
    }

    /// <summary>
    /// 新增后抛出 BusinessException；业务异常同样应中断自动提交。
    /// </summary>
    /// <param name="account">账户</param>
    [HttpPost("create-business-throw")]
    public async Task<IActionResult> CreateBusinessThrow([FromQuery] string account)
    {
        await userRepository.InsertAsync(NewUser(account));
        throw new BusinessException("模拟业务校验失败");
    }

    /// <summary>
    /// 标记 UnitOfWorkIgnore 且不手动保存；请求结束后变更应被丢弃。
    /// </summary>
    /// <param name="account">账户</param>
    [HttpPost("create-ignore")]
    [UnitOfWorkIgnore]
    public async Task<IActionResult> CreateIgnore([FromQuery] string account)
    {
        await userRepository.InsertAsync(NewUser(account));
        return Ok();
    }

    /// <summary>
    /// 标记 UnitOfWorkIgnore 并手动保存；数据应在请求内落库。
    /// </summary>
    /// <param name="account">账户</param>
    [HttpPost("create-ignore-save")]
    [UnitOfWorkIgnore]
    public async Task<IActionResult> CreateIgnoreAndSave([FromQuery] string account)
    {
        await userRepository.InsertAsync(NewUser(account));
        await unitOfWork.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// 使用 autoSave: true 提前保存后再抛异常；过滤器无法撤销已提交的数据。
    /// </summary>
    /// <param name="account">账户</param>
    [HttpPost("create-autosave-then-throw")]
    public async Task<IActionResult> CreateAutoSaveThenThrow([FromQuery] string account)
    {
        await userRepository.InsertAsync(NewUser(account), autoSave: true);
        throw new InvalidOperationException("保存后抛出异常");
    }

    private static SystemUserEntity NewUser(string account)
    {
        return new SystemUserEntity
        {
            Id = Guid.NewGuid(),
            Account = account,
            Name = account
        };
    }
}