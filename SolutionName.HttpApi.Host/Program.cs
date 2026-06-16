using DSB.Framework.Lite.Core;
using DSB.Framework.Lite.Data.EFCore.Extensions.Setting;
using DSB.Framework.Lite.Data.EFCore.Repository.WebApi;
using DSB.Framework.Lite.Hangfire.Extensions;
using DSB.Framework.Lite.Hangfire.Extensions.Setting;
using DSB.Framework.Lite.WebApi.ApiKeyAuthorization;
using DSB.Framework.Lite.WebApi.EncryptionApi;
using DSB.Framework.Lite.WebApi.Extensions;
using DSB.Framework.Lite.WebApi.Extensions.Http.Cors;
using DSB.Framework.Lite.WebApi.Extensions.Http.JwtBearer;
using DSB.Framework.Lite.WebApi.Extensions.Http.Middleware;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using Serilog.Events;
using SolutionName.Application;
using SolutionName.Application.Services.Systems;
using SolutionName.Domain.Options;
using SolutionName.EntityFrameworkCore;

namespace SolutionName.HttpApi.Host
{
    /// <summary>
    /// 主程序
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main函数
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            #region 日志配置

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.File(Path.Combine(AppContext.BaseDirectory, "logs", "logs.txt"), rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
#if DEBUG
                .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}")
#endif
                .CreateLogger();

            #endregion

            try
            {
                Log.Information("Starting SolutionName.HttpApi.Host");

                var builder = WebApplication.CreateBuilder(args);

                var configuration = builder.Configuration;

                builder.Host.UseSerilog();

                // Add services to the container.

                #region 注册数据库EFCore模块

                var efDbContextOptions = configuration.GetSection("EntityFrameworkCoreOptions").Get<EntityFrameworkCoreDbContextOptions>() ?? throw new ArgumentNullException("未找到数据库配置信息");
                builder.Services.AddEntityFrameworkCoreModule(efDbContextOptions);

                #endregion

                #region 注册应用模块

                builder.Services.AddApplicationModule(configuration);

                #endregion

                #region 配置WebApi

                builder.Services.AddHealthChecks();
                builder.Services.AddControllers().AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new DateTimeToJsonConverter());
                    options.JsonSerializerOptions.Converters.Add(new DateTimeOffsetToJsonConverter());
                });

                // 添加全局请求日志过滤器，注意：如果在生产环境开启可能会导致敏感数据泄露
                if (!builder.Environment.IsProduction())
                {
                    builder.Services.AddRequestLogFilter();
                }

                // 添加全局ApiResult包装过滤器，可以统一参数校验失败的返回结果格式
                builder.Services.AddApiResultFilter();

                #endregion

                #region 配置JWT授权和鉴权

                var jwtSetting = configuration.GetSection("JwtBearerSetting").Get<JwtBearerSetting>() ?? throw new ArgumentNullException("未找到JWT配置信息");
                builder.Services.AddJwtBearerAuthentication(setting =>
                {
                    setting.Audience = jwtSetting.Audience;
                    setting.UserInfoDesKey = jwtSetting.UserInfoDesKey;
                    setting.Expire = jwtSetting.Expire;
                    setting.ExpireType = jwtSetting.ExpireType;
                    setting.Issuer = jwtSetting.Issuer;
                    setting.SecurityKey = jwtSetting.SecurityKey;
                });

                // 鉴权规则
                builder.Services.AddIdentifierAuthorization(async (context, requirement) =>
                {
                    var isSuccess = false;
                    var id = context.User.FindFirst("Id")?.Value;
                    if (!string.IsNullOrEmpty(id) && Guid.TryParse(id, out var userId))
                    {
                        // 根据用户Id获取权限码集合，从分布式缓存中获取
                        var userCodes = await UserPermissionStorage.GetAsync(userId);
                        isSuccess = requirement.Check(userCodes);
                    }
                    return isSuccess;
                });

                // ApiKey鉴权
                builder.Services.AddApiKeyAuthorization(configuration);

                #endregion

                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                // builder.Services.AddEndpointsApiExplorer();

                #region 配置Swagger文档

                if (!builder.Environment.IsProduction())
                {
                    builder.Services.AddMySwagger(typeof(ModuleEnum), options =>
                    {
                        options.AddTokenHeaderAuthorize(JwtBearerDefaults.AuthenticationScheme); //JWT认证
                    });
                }

                #endregion

                #region 配置跨域

                var corsOptions = configuration.GetSection("CorsOptions").Get<CorsOptions>();
                builder.Services.AddMyCors(options =>
                {
                    options.IsCors = corsOptions?.IsCors ?? false;
                    options.Hosts = corsOptions?.Hosts;
                });

                #endregion

                #region 注册Hangfire服务

                var hangfireOptions = configuration.GetSection("HangfireOptions").Get<HangfireOptions>() ?? throw new ArgumentNullException("未找到hangfire配置信息");
                builder.Services.AddHangfireAndServer(hangfireOptions, jobServerOptions =>
                {
                    // 使用Redis可以设置成1s，提高检测到取消任务的响应速度，默认是5s
                    jobServerOptions.CancellationCheckInterval = TimeSpan.FromSeconds(1);
                });

                #endregion

                #region 注册上传文件配置
                var fileUploadOptions = configuration.GetSection("FileUploadOptions").Get<FileUploadOptions>() ?? new FileUploadOptions();
                builder.Services.Configure<FileUploadOptions>(options =>
                {
                    options.FileMaxSize = fileUploadOptions.FileMaxSize;
                    options.SavePath = fileUploadOptions.SavePath;
                    options.FileTypeOptions = fileUploadOptions.FileTypeOptions;
                });
                int fileMaxSize = fileUploadOptions.FileMaxSize * 1024 * 1024;
                // Kestrel设置请求体限制
                builder.Services.Configure<KestrelServerOptions>(options =>
                {
                    options.Limits.MaxRequestBodySize = fileMaxSize;
                });
                // IIS设置请求体限制
                builder.Services.Configure<IISServerOptions>(options =>
                {
                    options.MaxRequestBodySize = fileMaxSize;
                });
                // Form表单设置请求体限制
                builder.Services.Configure<FormOptions>(options =>
                {
                    options.MultipartBodyLengthLimit = fileMaxSize;
                });
                #endregion

                #region 配置转发头中间件
                // 配置转发头中间件，处理反向代理服务器传递过来的头信息，如X-Forwarded-For和X-Forwarded-Proto，确保在控制器通过ClientIP属性获取到正确的客户端IP地址
                builder.Services.Configure<ForwardedHeadersOptions>(options =>
                {
                    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                });
                #endregion

                #region 配置加密接口
                // 此处示例固定一个配置，如果需要从数据库加载，请在对应的启动服务中调用配置
                EncryptionApiOptionManager.Register(() =>
                {
                    return
                    [
                        new EncryptionApiOptions()
                    {
                        AppId="07e2b50a-950a-a7f7-c8f7-3a16a79c788c",
                        SecretKey="oL+pccgAs6CF7bjW58/XW9GuJNxsjcKHzFwadhhMGoc=",
                        Sm4Key="1234567890ascbdf"
                    }
                    ];
                }, "SolutionName");
                #endregion

                var app = builder.Build();

                // 使用转发头中间件，处理反向代理服务器传递过来的头信息
                app.UseForwardedHeaders();

                // 应用跨域默认策略
                app.UseCors();

                // 注意添加全局异常捕获，可以屏蔽UseDeveloperExceptionPage
                app.UseGlobalExceptionMiddleware();

                // 添加数据库工作单元中间件
                app.UseUnitOfWorkMiddleware<SolutionNameContext>();

                // 添加健康检查终结点
                app.UseHealthChecks(PathString.FromUriComponent("/"));

                // Configure the HTTP request pipeline.

                #region 配置SwaggerUI

                if (!app.Environment.IsProduction())
                {
                    app.UseMySwaggerUI(typeof(ModuleEnum));
                }

                #endregion

                //app.UseHttpsRedirection();

                app.UseAuthorization();

                app.MapControllers();

                #region 配置Hangfire任务和仪表盘

                app.Services.UseHangfireBackgroundJobs();
                app.UseHangfireDashboard(hangfireOptions.Account, hangfireOptions.Password);

                #endregion

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}