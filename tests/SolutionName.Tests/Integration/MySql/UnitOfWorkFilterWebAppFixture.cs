using DSB.Framework.Lite.Core;
using DSB.Framework.Lite.Data.EFCore.Extensions;
using DSB.Framework.Lite.Data.EFCore.Repository;
using DSB.Framework.Lite.Data.EFCore.Repository.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SolutionName.EntityFrameworkCore;
using Xunit;
using DbType = DSB.Framework.Lite.Data.EFCore.Extensions.Setting.DbType;

namespace SolutionName.Tests.Integration.MySql;

/// <summary>
/// 使用 TestServer 搭建最小 WebApi 管道，注册与 Host 一致的 DbContext、仓储和工作单元过滤器。
/// </summary>
public sealed class UnitOfWorkFilterWebAppFixture : IAsyncLifetime
{
    private WebApplication? application;

    /// <summary>
    /// 指向测试服务器的 HTTP 客户端。
    /// </summary>
    public HttpClient Client { get; private set; } = null!;

    /// <summary>
    /// 启动测试服务器。
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();

        builder.Services.AddEntityFrameworkCoreDbContext<SolutionNameContext>(MySqlTestSettings.GetOptions(), false);
        builder.Services.AddGuidGenerator(DbType.MySql);
        builder.Services.AddEntityFrameworkCoreRepository(typeof(SolutionNameContext));
        builder.Services.AddUnitOfWorkFilter<SolutionNameContext>();
        builder.Services.AddControllers().AddApplicationPart(typeof(UnitOfWorkProbeController).Assembly);

        application = builder.Build();

        // 模拟 GlobalExceptionMiddleware：捕获异常并返回 500，不重新抛出
        application.Use(async (context, next) =>
        {
            try
            {
                await next(context);
            }
            catch (BusinessException exception)
            {
                // 与 GlobalExceptionMiddleware 一致：业务异常返回 HTTP 200，响应体携带业务码
                context.Response.StatusCode = StatusCodes.Status200OK;
                await context.Response.WriteAsJsonAsync(new { code = exception.Code, msg = exception.Message });
            }
            catch (Exception exception)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync(exception.Message);
            }
        });

        application.MapControllers();
        await application.StartAsync();

        Client = application.GetTestClient();
    }

    /// <summary>
    /// 停止测试服务器并释放客户端。
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        Client.Dispose();

        if (application is not null)
        {
            await application.DisposeAsync();
        }
    }
}