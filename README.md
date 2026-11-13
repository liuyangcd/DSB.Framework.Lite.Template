# DSB.Framework.Lite.Template

基于 `DSB.Framework.Lite` 实现的 ASP.NET Core 8 解决方案模板。`src/SolutionName.HttpApi.Host` 是 Web API 入口项目，负责承载控制器、认证授权、全局过滤器、Swagger、Hangfire 仪表盘和静态/文件下载等 HTTP 相关能力；业务逻辑位于 `SolutionName.Application`，数据访问位于 `SolutionName.EntityFrameworkCore`。

> 本 README 面向日常开发，重点说明 **DSB.Framework.Lite 在本项目中的用法** 和 **编码规范**。更完整的框架 API 说明见各 NuGet 包自带的 readme，同目录的 `AGENTS.md` 也记录了构建、迁移和工作单元约定。

## 1. 解决方案结构

| 项目 | 职责 |
| --- | --- |
| `SolutionName.HttpApi.Host` | Web API 入口：控制器、认证授权、全局过滤器、Swagger、Hangfire |
| `SolutionName.Application` | 业务服务、后台任务、数据种子 |
| `SolutionName.Application.Contracts` | DTO、BO、Jwt 用户上下文 |
| `SolutionName.EntityFrameworkCore` | DbContext、仓储接口与实现、查询 |
| `SolutionName.Domain` | 实体、枚举、权限码/角色码、Options |
| `SolutionName.EntityFrameworkCore.DbMigrations` | EF Core 迁移（设计时项目） |
| `tests/SolutionName.Tests` | 单元测试与业务集成测试（xUnit v3） |

源码项目统一放在 `src/`，测试项目放在 `tests/`。

`src/SolutionName.HttpApi.Host` 内部结构：

| 关注点 | 所在位置 |
| --- | --- |
| 程序入口、DI 注册、中间件管道 | `src/SolutionName.HttpApi.Host/Program.cs` |
| 控制器与控制器基类 | `src/SolutionName.HttpApi.Host/Controllers/` |
| Swagger 模块分组 | `src/SolutionName.HttpApi.Host/ModuleEnum.cs` |
| 运行配置 | `src/SolutionName.HttpApi.Host/appsettings*.json` |
| 证书与 Docker | `src/SolutionName.HttpApi.Host/Certs/`、`src/SolutionName.HttpApi.Host/Dockerfile`、`src/SolutionName.HttpApi.Host/Dockerfile.Https`、`src/SolutionName.HttpApi.Host/docker.txt` |

项目依赖链：

```text
SolutionName.HttpApi.Host
  -> SolutionName.Application
    -> SolutionName.Application.Contracts
      -> SolutionName.EntityFrameworkCore
        -> SolutionName.Domain
```

各层职责：

- **HttpApi.Host**：路由、参数绑定、认证授权、返回包装、中间件、Swagger、Hangfire 仪表盘。
- **Application**：业务编排、业务校验、事务、后台任务。
- **Application.Contracts**：Dto、Bo、Jwt 用户上下文。
- **EntityFrameworkCore**：DbContext、仓储接口与实现、查询。
- **Domain**：实体、枚举、权限码/角色码、配置项类型。

## 2. 快速启动

### 2.1 环境要求

- .NET 8 SDK
- MySQL（模板默认）或其他受支持数据库：SqlServer / MySql / PostgreSQL / Oracle
- Redis（缓存、分布式锁、验证码、Hangfire、接口加密配置均依赖）
- `dotnet-ef` 工具（仅执行迁移时需要）

### 2.2 配置

开发环境配置在 `src/SolutionName.HttpApi.Host/appsettings.Development.json`。启动前至少确认：

| 配置节 | 作用 | 关键点 |
| --- | --- | --- |
| `EntityFrameworkCoreOptions` | 数据库 | `DbType` 决定使用 `ConnectionString` 中的哪一个连接串 |
| `ConnectionStrings:Redis` | Redis | 缓存、锁、验证码、Hangfire、加密接口配置 |
| `JwtBearerSetting` | JWT | `SecurityKey`、`RefreshSecurityKey`、过期时间、`UserInfoDesKey` |
| `HangfireOptions` | 任务调度 | 存储类型、Redis 连接、仪表盘 Basic 认证账号密码 |
| `ApiKeyOptions` | ApiKey 鉴权 | 字典结构，键名即 Key 名称，值为实际 Key |
| `CorsOptions` | 跨域 | `IsCors`、`Hosts` |
| `FileUploadOptions` | 上传 | 全局大小上限、保存目录、各类型扩展名与大小限制 |
| `HttpClientOptions` | 出站 HttpClient | 按名称配置 BaseAddress、是否忽略 SSL 等 |

> `src/SolutionName.HttpApi.Host/appsettings.Staging.json` 和 `src/SolutionName.HttpApi.Host/appsettings.Production.json` 默认是空对象。部署时必须通过环境变量、配置中心或密钥管理补充配置，不要把生产密钥写进仓库。

### 2.3 运行

```bash
# 还原并构建整个解决方案
dotnet build ./SolutionName.Backend.sln

# 启动 API
dotnet run --project ./src/SolutionName.HttpApi.Host
```

启动后会执行数据种子：

- 自动补齐缺失的权限数据（`src/SolutionName.Application/Services/Systems/PermissionDataSeedContributor.cs`）。
- 确保存在管理员角色，并把全部权限授予该角色（`AdminUserDataSeedContributor`）。
- 确保存在管理员账号，模板默认账号为 `admin`，密码为 `123456`。**部署后请立即修改。**
- 种子服务通过 `IUnitOfWorkProvider` 创建独立作用域并在最后统一保存。

健康检查终结点为 `/`。Swagger UI 仅在非 Production 环境启用。

### 2.4 测试

测试工程位于 `tests/SolutionName.Tests`，使用 xUnit v3，包含单元测试和业务集成测试：

```bash
# 运行测试工程
dotnet test ./tests/SolutionName.Tests/SolutionName.Tests.csproj

# 或通过解决方案运行全部测试
dotnet test ./SolutionName.Backend.sln
```

集成测试位于 `tests/SolutionName.Tests/Integration`，使用 SQLite 内存数据库搭建真实的 DbContext、仓储和服务，不依赖 MySQL/Redis。新增测试时，在 `tests/SolutionName.Tests` 下按被测模块建文件夹；需要访问 Application/Domain 类型时，在测试工程中添加对应的 `ProjectReference`。

### 2.5 Docker

先发布到 `./publish/Production`，复制 Dockerfile 后构建（完整部署说明见 [DEPLOYMENT.md](src/SolutionName.HttpApi.Host/DEPLOYMENT.md)）：

```bash
dotnet publish ./src/SolutionName.HttpApi.Host/SolutionName.HttpApi.Host.csproj -c Release -o ./publish/Production
cp ./src/SolutionName.HttpApi.Host/Dockerfile.Https ./publish/Production/
cd ./publish/Production
docker build -t solutionname.httpapi.host:v1 -f ./Dockerfile.Https .
```

启动示例（证书密码通过环境变量传入）：

```bash
docker run -d -p 8090:8080 -p 8091:8081 \
  -e ASPNETCORE_Kestrel__Certificates__Default__Password="12345678" \
  --restart=always \
  -v /root/app/test/api/logs:/app/logs \
  -v /root/app/test/api/Certs:/https \
  --name solutionname.httpapi.host \
  solutionname.httpapi.host:v1
```

## 3. 启动流程

### 3.1 服务注册顺序

`src/SolutionName.HttpApi.Host/Program.cs` 中的注册顺序如下：

1. **Serilog**：控制台（仅 DEBUG）+ 按天滚动文件日志。
2. **EF Core 模块**：`AddEntityFrameworkCoreModule` 注册 `SolutionNameContext`、Guid 生成器和仓储。
   - 默认全局 `NoTracking`，需要修改实体时显式传 `isTracking: true`。
   - DEBUG 下输出 SQL 日志、敏感数据和详细错误。
3. **应用模块**：`AddApplicationModule` 注册 Redis 缓存/分布式锁、PBKDF2 密码服务、命名 HttpClient、自动扫描应用服务、数据种子托管服务。
4. **MVC 与全局过滤器**：
   - `AddControllers` + `DateTimeToJsonConverter` / `DateTimeOffsetToJsonConverter`。
   - `AddRequestLogFilter()`：非 Production 才启用。
   - `AddUnitOfWorkFilter<SolutionNameContext>()`：Action 成功后统一提交。
   - `AddApiResultFilter()`：统一包装 `EmptyResult` 和参数校验失败结果。
5. **JWT 认证 + 权限码鉴权**：`AddJwtBearerAuthentication` + `AddIdentifierAuthorization`。
6. **ApiKey 鉴权**：`AddApiKeyAuthorization`。
7. **Swagger**：非 Production 注册，按 `ModuleEnum` 分组。
8. **CORS**：读取 `CorsOptions`。
9. **Hangfire**：注册客户端、服务端和 Redis 存储。
10. **上传限制**：把 `FileUploadOptions.FileMaxSize` 同步到 Kestrel、IIS 和 Form 表单限制。
11. **转发头**：配置 `X-Forwarded-For` / `X-Forwarded-Proto`，确保反向代理后能拿到正确客户端 IP。
12. **加密接口配置**：`EncryptionApiOptionManager.Register` 写入示例 AppId/SecretKey/Sm4Key。

### 3.2 中间件顺序

```text
UseForwardedHeaders      反向代理头
UseCors                  跨域（要在异常中间件之前，异常响应才带 CORS 头）
UseGlobalExceptionMiddleware
UseHealthChecks          "/"
UseMySwaggerUI           仅非 Production
UseAuthentication
UseAuthorization
MapControllers
Hangfire Dashboard
```

顺序不能随意调整：

- `UseCors` 必须在 `UseGlobalExceptionMiddleware` 之前。
- `UseGlobalExceptionMiddleware` 必须位于业务中间件和 `MapControllers` 之前；注册在它之前的中间件抛出的异常捕获不到。
- `UseAuthentication` 必须在 `UseAuthorization` 之前。
- `UseHttpsRedirection` 当前被注释掉，是否启用由部署方式决定（容器/网关通常自行处理 HTTPS）。

### 3.3 过滤器执行顺序

```text
GlobalExceptionMiddleware
  RequestLogFilter              -1000
    UnitOfWorkActionFilter      -500
      自定义 Action 过滤器      建议 0、100
        Action
      自定义 Action 过滤器返回
    SaveChangesAsync
  RequestEnd 日志
  ApiResultFilter               Result 过滤器
  Result 执行
```

框架保留 `-1000` 和 `-500`，自定义过滤器不要占用。需要数据库操作随本次请求提交时，Order 必须大于 `-500`。

## 4. DSB 框架使用指南

### 4.1 控制器基类与路由

所有控制器继承以下三个基类之一：

| 基类 | 认证方式 | 适用场景 |
| --- | --- | --- |
| `ManagerControllerBase` | JWT，需登录 | 管理后台接口 |
| `AnonymousControllerBase` | 匿名，类级 `[AllowAnonymous]` | 完全公开接口 |
| `ApiKeyControllerBase` | ApiKey | 第三方系统对接 |

基类已经包含 `[ApiController]` 和 `[Route("api/[controller]/[action]")]`，所以派生控制器不需要重复标注。路由示例：`UserController.GetPageList` -> `/api/User/GetPageList`。

推荐写法（`src/SolutionName.HttpApi.Host/Controllers/Systems/UserController.cs`）：

```csharp
[ApiExplorer(ModuleEnum.User)]
public class UserController(
    IJwtService jwtService,
    UserService userService) : ManagerControllerBase(jwtService)
{
    /// <summary>
    /// 分页获取用户列表
    /// </summary>
    [HttpPost]
    [IdentifierAuthorize(PermissionCodes.User.Page)]
    public async Task<ApiResult<PagedResult<GetPageListOutputDto>>> GetPageList(GetPageListInputDto inputDto)
    {
        return ApiResult<PagedResult<GetPageListOutputDto>>.GetSuccess(await userService.GetPageListAsync(inputDto));
    }
}
```

- `[ApiExplorer(ModuleEnum.X)]` 是 DSB 的 Swagger 分组特性，不是 ASP.NET Core 自带的 `ApiExplorerSettings`。
- `ManagerControllerBase` 暴露 `UserContext`，直接使用 `UserContext.Id` / `UserContext.Name`，不要手动解析 Claims。
- 单个 Action 需要匿名访问时，在 Action 上标 `[AllowAnonymous]`（`AuthController` 的登录、验证码就是这种写法）。
- 文件上传/下载等敏感或大 body 接口可以加 `[RequestLogIgnore]`，避免请求日志记录文件内容。

### 4.2 统一返回 ApiResult

普通 JSON 接口统一返回 `ApiResult<T>`：

```csharp
return ApiResult<OrderDto>.GetSuccess(order);                  // 成功
return ApiResult<OrderDto>.GetFail("订单不存在");              // 业务失败
return ApiResult<OrderDto>.Get(ResultCode.Fail, "库存不足", order); // 自定义状态码
```

字段为 `Code`、`Msg`、`Data`，JSON 序列化后是 camelCase：

```json
{
  "code": 200,
  "msg": "...",
  "data": {}
}
```

常用 `ResultCode`：

| Code | 含义 |
| --- | --- |
| 200 | 成功 |
| 201 | 业务失败 |
| 301 | 参数校验失败 |
| 302 | Header 错误 |
| 303 | 请求类型错误 |
| 401 | 未授权 |
| 403 | 拒绝访问 |
| 404 | 资源未找到 |
| 500 | 服务器异常 |

业务代码中不需要在每个 Action 里写 try/catch。直接抛 `BusinessException`：

```csharp
throw new BusinessException("用户不存在");
```

全局异常中间件会把它转换成 `ApiResult`（HTTP 200，`code = 201`）。未知异常统一返回 `code = 500`，不会把堆栈暴露给客户端。客户端取消请求和服务端取消请求也会被区分处理。

> 约定：普通 JSON 接口统一返回 `ApiResult<T>`，不要把实体或匿名对象直接返回给前端；文件流、验证码图片等特殊响应可以直接返回 `IActionResult` / DTO。Controller 里不写业务判断，业务异常由 Service 抛出。

### 4.3 参数校验

DTO 使用 DataAnnotations 表达边界约束：

```csharp
public class CreateInputDto
{
    [StringLength(50, ErrorMessage = "姓名长度不超过50个字符")]
    public required string Name { get; set; }

    [StringLength(100, ErrorMessage = "邮箱长度不超过100")]
    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    public string? Email { get; set; }

    public RecordStatus Status { get; set; } = RecordStatus.Normally;
}
```

- 基类的 `[ApiController]` 会自动触发模型校验。
- 校验失败由 `ApiResultFilter` 包装成 `code = 301`，错误明细放在 `data` 中。
- 简单格式校验放在 DTO；跨字段校验、查库校验放在 Service，抛 `BusinessException`。

### 4.4 JWT 认证与权限码鉴权

登录流程（`AuthController` + `AuthService`）：

1. `POST /api/Auth/Login` 校验账号密码（PBKDF2）。
2. `AuthService` 查询用户权限码，写入 Redis（`UserPermissionStorage`）。
3. `jwtService.CreateToken<JwtUserContext, Guid>(userContext)` 生成 Token。
4. 后续请求由 `JwtControllerBase` 解析 Token，填充 `UserContext`。

JWT 配置来自 `JwtBearerSetting`：

```json
{
  "JwtBearerSetting": {
    "Issuer": "SolutionName",
    "Audience": "SolutionName",
    "SecurityKey": "...",
    "Expire": 8,
    "ExpireType": "Hours",
    "RefreshSecurityKey": "...",
    "RefreshExpire": 30,
    "RefreshExpireType": "Days",
    "UserInfoDesKey": "12345678"
  }
}
```

接口权限通过权限码控制：

```csharp
[HttpPost]
[IdentifierAuthorize(PermissionCodes.User.Create)]
public async Task<ApiResult<bool>> Create(CreateInputDto inputDto)
{
    return ApiResult<bool>.GetSuccess(await userService.CreateAsync(inputDto, UserContext?.Id));
}
```

权限码统一放在 `src/SolutionName.Domain/PermissionCodes.cs`，按「目录 -> 菜单 -> 接口」组织：

```csharp
public static class PermissionCodes
{
    public const string System = "system";

    public static class User
    {
        public const string Menu = "system:user";
        public const string Page = "user:page";
        public const string Create = "user:create";
        public const string Update = "user:update";
        public const string Delete = "user:delete";
    }
}
```

鉴权回调在 `src/SolutionName.HttpApi.Host/Program.cs` 注册：从 JWT 中取 `Id` 和 `UserType`，再从 `UserPermissionStorage` 读取该用户的权限码集合，交给 `requirement.Check(userCodes)` 判断。

注意：

- `UserPermissionStorage` 的缓存有效期与 Token 有效期一致。角色/权限调整后，已登录用户不会立刻生效；需要重新登录、刷新 Token，或主动更新/删除对应用户的缓存。
- 鉴权失败时，DSB 默认把 401/403 统一改为 **HTTP 200 + `ApiResult`**（`AddJwtBearerAuthentication` 的 `isStatusCodeFailedTo200` 默认是 `true`）。如果需要标准 HTTP 状态码，注册时显式传 `isStatusCodeFailedTo200: false`。
- 新增业务模块时，要同步新增权限码常量、权限种子数据和接口上的 `[IdentifierAuthorize]`，否则管理员也访问不了。

### 4.5 ApiKey 鉴权

用于第三方系统对接，配置为字典：

```json
{
  "ApiKeyOptions": {
    "Default": "123456",
    "Partner": "partner-key"
  }
}
```

控制器继承 `ApiKeyControllerBase`，基类已标注 `[ApiKeyAuthorization]`：

```csharp
public class ApiKeyTestController : ApiKeyControllerBase
{
    [HttpGet]
    public Task<ApiResult<string>> ApiKeyTest()
    {
        return Task.FromResult(ApiResult<string>.GetSuccess($"ApiKey验证成功：{ApiKeyName}"));
    }
}
```

请求头支持两种写法：

```http
Authorization: Bearer <ApiKey>
Authorization: <ApiKey>
```

- 不指定名称时匹配任意已配置的 Key；`[ApiKeyAuthorization("Partner")]` 只匹配指定名称。
- 标记 `[AllowAnonymous]` 的接口会跳过 ApiKey 校验。
- 校验通过后，`ApiKeyName` 就是配置中的键名。
- 默认失败返回 401，可通过 `AddApiKeyAuthorization` 的第二个参数自定义失败响应。

### 4.6 工作单元与保存时机（重点）

DSB 的仓储默认 `autoSave = false`，只修改跟踪状态，不立即落库。保存时机取决于代码运行位置，共有三条规则：

#### 规则 1：API 管道内，自动保存

`AddUnitOfWorkFilter<SolutionNameContext>()` 注册了全局 Action 过滤器。Action 正常返回后自动 `SaveChangesAsync`；Action 或内层过滤器抛异常则不提交。

```csharp
// Controller 中不需要写 SaveChanges
public async Task<ApiResult<bool>> Create(CreateInputDto inputDto)
{
    await userRepository.InsertAsync(entity);
    return ApiResult<bool>.GetSuccess(true);
}
// 请求成功后，UnitOfWorkActionFilter 统一提交
```

注意：

- 过滤器**只提交，不自动开启事务，也不负责回滚**。
- 即使返回 `BadRequest`、`Unauthorized` 等失败结果，只要没有抛异常，也会提交。
- 需要多步操作原子性时，自行开启事务。

#### 规则 2：API 管道外，手动保存

Hangfire 任务、HostedService、控制台程序等不经过 Action 过滤器，必须注入 `IUnitOfWork<TDbContext>` 并手动保存：

```csharp
public class CycleJob(
    IUnitOfWork<SolutionNameContext> unitOfWork,
    IUserRepository userRepository) : BackgroundJobBase
{
    public override async Task ExecuteAsync(CancellationToken ct)
    {
        var user = await userRepository.GetAsync(null, cancellationToken: ct);
        await unitOfWork.SaveChangesAsync(ct); // 必须手动提交
    }
}
```

参考：`src/SolutionName.Application/BackgroundJobs/Jobs/CycleJob.cs`、`src/SolutionName.Application/BackgroundJobs/Jobs/OneTimeJob.cs`。

#### 规则 3：API 内新开线程 / fire-and-forget，使用 IUnitOfWorkProvider

HTTP 请求结束后，当前 DI 作用域和所有仓储/UoW 都会被释放。不要在新线程里直接使用构造函数注入的仓储。

```csharp
using var uowScope = unitOfWorkProvider.CreateScope();
var repository = uowScope.ServiceScope.ServiceProvider
    .GetRequiredService<IUserRepository>();

await repository.InsertAsync(entity);
await uowScope.Current.SaveChangesAsync();
```

参考：`src/SolutionName.Application/DataSeedHostedService.cs`。

#### 跳过自动提交与事务

需要自己控制保存节点时，在 Action 或 Controller 上标 `[UnitOfWorkIgnore]`：

```csharp
[HttpPost]
[UnitOfWorkIgnore]
public async Task<IActionResult> Confirm()
{
    await using var transaction = await unitOfWork.BeginTransactionAsync();
    try
    {
        // ... 多步数据库操作 ...
        await unitOfWork.SaveChangesAsync();
        await transaction.CommitAsync();
        return Ok();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

`ExecuteUpdateAsync` / `ExecuteDeleteAsync` / `ExecuteDeprecateAsync` 是直接下发 SQL，立即生效，不经过工作单元，也不需要 `SaveChanges`。

### 4.7 服务自动注册

应用服务继承 `SolutionNameApplicationService`：

```csharp
public class UserService(
    IGuidGenerator guidGenerator,
    IPasswordService passwordService,
    IUserRepository userRepository) : SolutionNameApplicationService
{
}
```

`AddApplicationModule` 中的 `services.AddApplicationService<SolutionNameApplicationService>()` 会扫描基类所在程序集，把派生服务批量注册为 Scoped。

- 当前模板直接注入具体服务类（如 `UserService`），也可以定义业务接口并按接口注入。
- 生命周期由基类实现的 `IBaseScopedService` 决定，不要在服务上再实现多个生命周期标记接口。
- 后台任务基类 `BackgroundJobBase` / `BackgroundJobBase<T>` 同样继承 `SolutionNameApplicationService`，所以也会被自动注册。

### 4.8 仓储与查询

#### 自定义仓储 vs 泛型仓储

| 场景 | 做法 |
| --- | --- |
| Service 需要注入该实体的仓储 | 建 `IXxxRepository` + `XxxRepository` |
| 只在其他仓储内部使用（如用户角色、角色权限等中间表） | 直接注入泛型 `IEntityFrameworkCoreRepository<SolutionNameContext, TEntity, Guid>` |

自定义仓储实现：

```csharp
public class UserRepository(
    IUnitOfWork<SolutionNameContext> unitOfWork,
    IGuidGenerator guidGenerator,
    IRoleRepository roleRepository,
    IEntityFrameworkCoreRepository<SolutionNameContext, SystemUserRoleEntity, Guid> userRoleRepository)
    : EntityFrameworkCoreRepository<SolutionNameContext, SystemUserEntity, Guid>(unitOfWork), IUserRepository
{
}
```

仓储接口继承：

```csharp
public interface IUserRepository
    : IEntityFrameworkCoreRepository<SolutionNameContext, SystemUserEntity, Guid>
{
}
```

#### 查询数据的三层入口

| 入口 | 是否自动过滤逻辑删除 | 使用建议 |
| --- | --- | --- |
| 默认仓储方法（`GetAsync`、`GetListAsync`、`GetPagedListAsync` 等） | 是 | 首选 |
| `Queryable` | 是 | 自定义 join / 投影查询 |
| `DbSet` / `Database` | **否** | 仅特殊场景，必须自己加 `EntityStatus` 过滤 |

> 这里的 `EntityStatus` 是 `EntityBase` 的逻辑删除标记，和业务字段 `Status`（`RecordStatus`）不是一回事。

#### 常用查询

```csharp
// 按主键投影
var dto = await userRepository.GetSingleAsync(id,
    ExpressionGenericMapper<SystemUserEntity, GetDetailOutputDto>.Selector);

// 条件 + 排序 + 投影分页
var page = await userRepository.GetPagedListAsync(
    inputDto,
    predicate,
    [new SortSpecDesc<SystemUserEntity>(x => x.CreateDateAt)],
    ExpressionGenericMapper<SystemUserEntity, GetPageListOutputDto>.Selector);

// 自定义 join 查询
var query = from userRole in userRoleRepository.Queryable
            join role in roleRepository.Queryable on userRole.RoleId equals role.Id
            where userRole.UserId == userId && role.Status == RecordStatus.Normally
            select userRole;
var list = await query.Select(selector).ToListAsync();
```

约定：

- 分页必须传排序。没有排序时数据库返回顺序不确定，翻页可能重复或遗漏。
- `PageIndex` 从 1 开始。
- 只读查询使用默认无跟踪；需要修改实体时传 `isTracking: true`。
- 列表查询优先用投影，只查需要的列，不要先查全表再在内存里裁剪。

### 4.9 逻辑删除

所有删除使用逻辑删除，禁止物理删除：

```csharp
// 标记删除，等待工作单元提交
var user = await userRepository.GetSingleAsync(id) ?? throw new BusinessException("用户不存在");
await userRepository.DeprecateAsync(user);

// 按条件立即逻辑删除（直接下发 SQL）
await userRepository.ExecuteDeprecateAsync(x => x.Status == RecordStatus.Disabled);
```

默认查询方法已经排除 `EntityStatus` 为删除的数据。确需查询已删除数据时使用 `NoFilterQueryable`。

### 4.10 Swagger 模块分组

Swagger 按 `ModuleEnum` 分组。每个枚举值必须标 `[Display]`，并指定 `Name`、`Description`、`Order`：

```csharp
public enum ModuleEnum
{
    [Display(Name = nameof(Auth), Description = "鉴权管理", Order = (int)Auth)]
    Auth,

    [Display(Name = nameof(User), Description = "用户管理", Order = (int)User)]
    User
}
```

控制器上标 `[ApiExplorer(ModuleEnum.User)]` 即归入对应分组。新增模块时：

1. 在 `ModuleEnum` 增加枚举值和 `[Display]`。
2. 控制器加 `[ApiExplorer(ModuleEnum.新模块)]`。
3. 确认项目开启 `GenerateDocumentationFile`，否则 Swagger 没有 XML 注释。

### 4.11 Hangfire

注册与启动：

```csharp
builder.Services.AddHangfireAndServer(hangfireOptions, options =>
{
    // Redis 下可以调小，提高取消任务的响应速度
    options.CancellationCheckInterval = TimeSpan.FromSeconds(1);
});

app.Services.UseHangfireBackgroundJobs();
app.UseHangfireDashboard(hangfireOptions.Account, hangfireOptions.Password);
```

发布一次性任务：

```csharp
var jobId = backgroundJobClient.Enqueue<OneTimeJob>(
    job => job.ExecuteAsync(Guid.Empty, CancellationToken.None));
```

注册周期任务（`SolutionNameApplicationModule.UseHangfireBackgroundJobs`）：

```csharp
recurringJobManager.AddOrUpdate<CycleJob>(
    nameof(CycleJob),
    job => job.ExecuteAsync(CancellationToken.None),
    Cron.Minutely);
```

任务类约定：

```csharp
public class OneTimeJob(
    ILogger<OneTimeJob> logger,
    IUnitOfWork<SolutionNameContext> unitOfWork,
    IUserRepository userRepository) : BackgroundJobBase<Guid>
{
    [JobDisplayName("一次性任务示例")]
    public override async Task ExecuteAsync(Guid parameter, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // ... 业务 ...
        await unitOfWork.SaveChangesAsync(ct); // 管道外必须手动保存
    }
}
```

- 任务必须继承 `BackgroundJobBase` 或 `BackgroundJobBase<T>`，放在 `src/SolutionName.Application/BackgroundJobs/Jobs`。
- `CancellationToken` 固定作为最后一个参数传入，Hangfire 会自动构建；所有异步调用都要传播 `ct`。
- 捕获 `OperationCanceledException` 做优雅退出，循环中可用 `ct.ThrowIfCancellationRequested()`。
- `[JobDisplayName]` 让仪表盘显示可读名称。
- 管道外没有自动保存，记得调用 `IUnitOfWork.SaveChangesAsync`。

### 4.12 文件上传

`FileUploadOptions` 配置全局上限和按类型的扩展名/大小限制：

```json
{
  "FileUploadOptions": {
    "FileMaxSize": 1024,
    "SavePath": "UploadFiles",
    "FileTypeOptions": {
      "Pictrue": { "Extensions": ".jpg,.jpeg,.png,.gif,.bmp", "MaxSize": 20 },
      "Document": { "Extensions": ".doc,.docx,.xls,.xlsx,.json,.txt,.pdf", "MaxSize": 100 }
    }
  }
}
```

`src/SolutionName.HttpApi.Host/Program.cs` 会把 `FileMaxSize` 同步到 Kestrel、IIS 和 `FormOptions.MultipartBodyLengthLimit`。上传实现位于 `src/SolutionName.Application/Services/Systems/UploadFileService.cs`：

- 文件保存到 `AppContext.BaseDirectory/SavePath/yyyyMM/dd/{fileId}{ext}`。
- 上传接口标 `[RequestLogIgnore]`，避免日志记录文件流。
- 下载接口为匿名访问（`/api/File/Get/{fileName}`），通过 `fileName` 反查文件记录。
- `FileUploadOptions` 中的类型名 `Pictrue` 是模板现有拼写，新增配置时保持与代码一致即可。

### 4.13 接口加密（SM4 + 签名）

用于对请求参数解密、时间戳校验、HMAC-SHA256 签名校验，以及对响应数据加密。控制器写法：

```csharp
[HttpPost]
public async Task<EncryptionApiResult<EncryptionApiTestDto>> EncryptionApi(
    [FromBody] EncryptionApiInput<EncryptionApiTestDto> inputDto)
{
    // inputDto.Data 已解密并通过校验
    return EncryptionApiResult<EncryptionApiTestDto>.GetSuccess(inputDto.Data, inputDto.ApiOption);
}
```

启动时注册配置：

```csharp
EncryptionApiOptionManager.Register(() =>
[
    new EncryptionApiOptions
    {
        AppId = "07e2b50a-950a-a7f7-c8f7-3a16a79c788c",
        SecretKey = "oL+pccgAs6CF7bjW58/XW9GuJNxsjcKHzFwadhhMGoc=",
        Sm4Key = "1234567890ascbdf"
    }
], "SolutionName");
```

- 加密接口的配置读写依赖 CSRedis，使用前必须配置 `ConnectionStrings:Redis`；否则 `EncryptionApiOptionManager.Register` 无法写入配置。
- 模板里的 AppId/SecretKey/Sm4Key 是演示值，正式环境必须替换，并避免提交到仓库。
- 配置也可以从数据库加载后在启动服务中调用 `Register` 写入。
- 请求格式、签名算法和 `ClockSkew` 等参数见 `DSB.Framework.Lite.WebApi.EncryptionApi` 包说明。

### 4.14 缓存、分布式锁、密码、验证码、HttpClient

这些能力在 `Application` 模块注册，Host 侧只需正确配置：

- **缓存 / 分布式锁**：配置 `ConnectionStrings:Redis` 后，`AddCache`、`AddCSRedis`、`AddRangeExclusiveLock("SolutionName")` 自动启用。项目里通过 `RedisHelper` 直接读写缓存（`UserPermissionStorage`、`CaptchaService`）。
- **密码**：`AddPBKDF2Password` 注册 `IPasswordService`，登录和新增用户时用它生成/校验密码哈希。
- **验证码**：`CaptchaService` 基于 Redis 存储，默认 1 分钟过期；登录接口在非开发环境才校验验证码。
- **HttpClient**：`HttpClientOptions` 按名称配置出站客户端（BaseAddress、是否忽略 SSL 等），通过框架的命名客户端注入使用。

### 4.15 健康检查、转发头、CORS

- 健康检查：`app.UseHealthChecks("/")`，用于容器/网关探活。
- 转发头：`UseForwardedHeaders` 处理反向代理传来的 `X-Forwarded-For` / `X-Forwarded-Proto`，否则控制器里拿到的客户端 IP 可能是代理 IP。
- CORS：通过 `CorsOptions` 配置；启用后允许配置的来源、任意请求头和任意方法，并允许携带凭据。生产环境不要把 `Hosts` 配成 `*`。

## 5. 编码规范

### 5.1 分层职责

| 层 | 只做 | 不要做 |
| --- | --- | --- |
| Controller | 路由、绑定、认证授权特性、调用 Service、包装 `ApiResult` | 写业务逻辑、直接访问 DbContext、拼 SQL |
| Application Service | 业务校验、编排、事务、调用仓储 | 依赖 `HttpContext`、返回实体给控制器 |
| Contracts | DTO/BO 定义、DataAnnotations、Jwt 用户上下文 | 放业务逻辑、在 DTO 中引用 DbContext/仓储 |
| Domain | 实体、枚举、权限码/角色码、Options | 引用 Application / EntityFrameworkCore 项目 |
| EntityFrameworkCore | DbContext、仓储、查询、索引配置 | 写业务规则 |

依赖方向只能从上往下：Host -> Application -> Contracts -> EntityFrameworkCore -> Domain。

### 5.2 命名约定

| 类型 | 规则 | 示例 |
| --- | --- | --- |
| 控制器 | `XxxController` | `UserController` |
| 服务 | `XxxService` | `UserService`、`AuthService` |
| 仓储接口 | `IXxxRepository` | `IUserRepository` |
| 仓储实现 | `XxxRepository` | `UserRepository` |
| 实体 | `SystemXxxEntity`（系统模块） | `SystemUserEntity` |
| 输入 DTO | `XxxInputDto` | `CreateInputDto`、`GetPageListInputDto` |
| 输出 DTO | `XxxOutputDto` | `GetDetailOutputDto` |
| 业务对象 | `XxxBo` | `UploadInputBo`、`LoginOutputBo` |
| 异步方法 | 以 `Async` 结尾 | `GetPageListAsync` |
| 权限码 | 小写冒号分隔 | `user:create`、`system:user` |
| 角色码 | 小写 | `admin` |

### 5.3 控制器规范

- 继承 `ManagerControllerBase` / `AnonymousControllerBase` / `ApiKeyControllerBase`，不要直接继承 `ControllerBase`。
- 类上标 `[ApiExplorer(ModuleEnum.X)]`，否则 Swagger 不会归入模块分组。
- 每个需要授权的 Action 标 `[IdentifierAuthorize(PermissionCodes.X.Y)]`；公开接口才用 `[AllowAnonymous]`。
- 一个 Action 只做参数接收、调用 Service、返回 `ApiResult<T>.GetSuccess(...)`；文件流、验证码等特殊响应除外。
- 当前登录用户从 `UserContext` 取，不要解析 `HttpContext.User` 里的 Claim。
- 上传/下载或包含敏感信息的接口加 `[RequestLogIgnore]`。
- 所有 public 成员写 XML 注释，Swagger 会读取。
- 普通 JSON 接口统一用 `ApiResult<T>` 包装；文件流、验证码等特殊响应可以直接返回 `IActionResult` 或 DTO。

### 5.4 服务规范

- 继承 `SolutionNameApplicationService`，使用主构造函数注入依赖。
- 业务失败抛 `BusinessException`，消息给用户看；不要返回 `null` 让控制器猜。
- 实体与 DTO 的转换优先用 `TransObject` / `ExpressionGenericMapper`，只有特殊字段才手写映射。
- API 管道内不要手动 `SaveChanges`，交给工作单元过滤器；后台任务和管道外代码必须手动保存。
- 需要多步写入的原子性时，显式 `BeginTransactionAsync`，catch 后回滚并重新抛出。
- 服务返回 DTO/BO，不把实体暴露给控制器。
- 需要日志时注入 `ILogger<T>`，不要用静态日志。

### 5.5 DTO / BO 规范

- DTO 只放数据和 DataAnnotations，不放业务方法。
- 输入 DTO 的必填引用类型用 `required`，可空字段用 `?`。
- 字符串统一加 `[StringLength]`，邮箱加 `[EmailAddress]`，必填参数加 `[Required]`。
- 分页输入继承/使用 `PagedQueryBase` 的 `PageIndex` / `PageSize`，不要自己发明分页字段。
- 文件流等无法直接 JSON 绑定的输入用 BO（如 `UploadInputBo`）。
- 输出 DTO 只包含前端需要的字段，不要把实体全部字段直接暴露。

### 5.6 实体规范

- 继承 `EntityBase<Guid>`，自动获得 `Id`、`EntityStatus`、`CreateDateAt`、`UpdateDateAt`。
- 非空引用类型用 `required`，可空字段用 `?`。
- 长度、格式约束用 DataAnnotations，和 DTO 保持一致。
- **默认值写在实体属性上，不要用 Fluent API 的 `.HasDefaultValue()`**：

```csharp
public RecordStatus Status { get; set; } = RecordStatus.Normally;
```

  原因：EF Core 的哨兵值机制会导致 Insert 语句包含所有字段，数据库默认值可能不生效，且容易出现实体默认值与数据库默认值不一致。

- 业务状态用 `RecordStatus`（`Disabled = 0`、`Normally = 1`）；逻辑删除用继承的 `EntityStatus`，两者不要混用。
- 索引、唯一约束、逻辑删除全局过滤器在 `src/SolutionName.EntityFrameworkCore/SolutionNameContext.cs` 的 `OnModelCreating` 中配置。

### 5.7 仓储规范

- Service 需要注入的仓储才建自定义接口/实现；中间表直接用泛型仓储。
- 自定义仓储继承 `EntityFrameworkCoreRepository<SolutionNameContext, TEntity, Guid>`。
- 自定义查询用 `Queryable`（已带逻辑删除过滤），不要用 `DbSet` 再手动过滤。
- 只读查询默认无跟踪；准备修改实体时才传 `isTracking: true`。
- 分页查询必须排序，优先投影。
- 删除只用 `DeprecateAsync` / `DeprecateRangeAsync` / `ExecuteDeprecateAsync`，不要物理删除。

### 5.8 数据修改优先级

| 优先级 | 场景 | 写法 |
| --- | --- | --- |
| 1 | 只改少量字段，不需要先读实体 | `ExecuteUpdateAsync(id, x => x.SetProperty(...))` |
| 2 | 需要先校验/读取实体，再按字段修改 | `GetSingleAsync(id, isTracking: true)` + 修改 + `UpdateAsync` |
| 3 | 批量更新 | `ExecuteUpdateAsync(where, setters)` 或 `UpdateRangeAsync` |

示例：

```csharp
// 只改状态：直接 SQL，无需查询
return await userRepository.ExecuteUpdateAsync(
    inputDto.Id,
    x => x.SetProperty(y => y.Status, inputDto.Status));

// 需要校验用户存在，并可能修改多个字段：跟踪查询
var user = await userRepository.GetSingleAsync(inputDto.Id, true)
    ?? throw new BusinessException("用户不存在");

inputDto.TransObject(user);
await userRepository.UpdateAsync(user);
```

跟踪查询生成的是**只包含变化字段**的 UPDATE；游离实体 `UpdateAsync(entity)` 会整行更新，非必要不要用。

### 5.9 异步与取消

- 所有 I/O 使用异步方法，禁止 `.Result`、`.Wait()`、`GetAwaiter().GetResult()`。
- 后台任务的 `CancellationToken` 必须继续传给仓储、`SaveChangesAsync` 和 `Task.Delay`。
- 长耗时接口可以接收 Action 的 `CancellationToken`，在循环中调用 `ThrowIfCancellationRequested()`。
- 捕获 `OperationCanceledException` 时不要当成业务异常返回；让框架的全局异常中间件处理客户端取消/服务端取消。

### 5.10 异常与日志

- 可预期的业务错误抛 `BusinessException`，消息用中文、面向用户。
- 未知异常不要吞掉，让它冒泡到 `GlobalExceptionMiddleware`；需要记录内部异常时先 `logger.LogError(ex, ...)` 再抛 `BusinessException`。`BusinessException` 只有 `(string)` 和 `(ResultCode, string)` 两个构造函数，不能直接传内部异常。
- 不要在 Controller 里写大段 try/catch。
- 请求日志只在非 Production 开启，且会记录请求参数和请求头；敏感接口用 `[RequestLogIgnore]`。
- 日志使用 Serilog，默认写入 `{程序目录}/logs/`，按天滚动（文件名形如 `logs20261113.txt`）。不要记录密码、Token、密钥、完整身份证号等敏感信息。

### 5.11 注释与代码格式

- 所有 public 类型和成员写 XML 注释（`<summary>`、参数、返回值、异常）。
- 新文件使用 file-scoped namespace；枚举、DTO、实体、服务保持与现有文件一致的风格。
- 开启 nullable，尽量避免无理由的 `!`。
- 使用主构造函数注入依赖，字段只在需要时声明。
- 常量集中管理：权限码放 `PermissionCodes`，角色码放 `RoleCodes`，不要散落魔法字符串。
- 一个文件一个主要类型；DTO 按业务目录分文件夹。

### 5.12 配置与安全

- 开发配置放 `src/SolutionName.HttpApi.Host/appsettings.Development.json`，生产配置通过环境变量/配置中心注入。
- JWT 密钥、加密接口密钥、数据库密码、Redis 密码不要提交到仓库。
- 生产环境关闭 Swagger 和请求日志（当前代码已按环境判断）。
- CORS 的 `Hosts` 不要使用 `*`，按实际前端域名配置。
- 上传目录要有写权限，并限制可执行文件类型；不要把上传目录配置到 Web 根目录下可执行的位置。

## 6. 新增业务模块 Checklist

以新增「订单管理」为例：

1. **Domain**
   - 新建 `SystemOrderEntity : EntityBase<Guid>`，属性加 DataAnnotations，状态默认值写在实体上。
   - 如有枚举，放 `src/SolutionName.Domain/Enums`。
   - 在 `PermissionCodes` 增加 `system:order`、`order:page`、`order:create` 等常量。
2. **EntityFrameworkCore**
   - `SolutionNameContext` 增加 `DbSet<SystemOrderEntity>`。
   - 在 `OnModelCreating` 配置索引/唯一约束。
   - 新建 `IOrderRepository` / `OrderRepository`（只有 Service 注入时才需要）。
   - 在 DbMigrations 项目生成迁移并更新数据库。
3. **Application.Contracts**
   - 新建 `CreateInputDto`、`UpdateInputDto`、`GetPageListInputDto`、`GetPageListOutputDto`、`GetDetailOutputDto`。
4. **Application**
   - 新建 `OrderService : SolutionNameApplicationService`，实现增删改查。
   - 分页查询使用 `GetPagedListAsync` + 排序 + 投影。
   - 删除使用 `DeprecateAsync`。
5. **HttpApi.Host**
   - `ModuleEnum` 增加 `Order` 及 `[Display]`。
   - 新建 `OrderController : ManagerControllerBase`，标 `[ApiExplorer(ModuleEnum.Order)]`。
   - 每个 Action 标 `[IdentifierAuthorize(PermissionCodes.Order.X)]`，返回 `ApiResult<T>`。
6. **权限种子**
   - 在 `src/SolutionName.Application/Services/Systems/PermissionDataSeedContributor.cs` 的 `GetSeedPermissions` 中增加目录/菜单/接口权限记录，否则管理员角色不会自动获得新权限。
7. **验证**
   - `dotnet build ./SolutionName.Backend.sln`
   - 启动后检查 Swagger 分组、登录、权限校验、分页和逻辑删除。
   - 如果新增后台任务，确认任务类继承 `BackgroundJobBase` 并在管道外手动保存。

### EF Core 迁移命令

所有 EF 命令都在 **DbMigrations 项目**执行，不要在 Host 项目执行：

```bash
# 新增迁移
dotnet ef migrations add <Name> \
  --project ./src/SolutionName.EntityFrameworkCore.DbMigrations/ \
  --startup-project ./src/SolutionName.EntityFrameworkCore.DbMigrations/

# 更新数据库
dotnet ef database update \
  --project ./src/SolutionName.EntityFrameworkCore.DbMigrations/ \
  --startup-project ./src/SolutionName.EntityFrameworkCore.DbMigrations/

# 生成幂等 SQL 脚本
dotnet ef migrations script -i -o ./script.sql \
  --project ./src/SolutionName.EntityFrameworkCore.DbMigrations/ \
  --startup-project ./src/SolutionName.EntityFrameworkCore.DbMigrations/
```

`src/SolutionName.HttpApi.Host/.config/dotnet-tools.json` 中的 `dotnet-ef` 是 9.0.8，而项目使用 EF Core 8.x。如果命令报版本不兼容，安装与 EF Core 匹配的工具版本。

## 7. 常见问题

**Q：Action 执行成功，但数据没有落库？**

检查是否标了 `[UnitOfWorkIgnore]` 但没有手动保存、是否在 API 管道外（Hangfire / HostedService）忘了调用 `IUnitOfWork.SaveChangesAsync`，或者 Action 是否抛了异常导致过滤器跳过提交。正常情况下，Action 成功返回后 `UnitOfWorkActionFilter` 会统一提交。

**Q：Action 抛异常了，数据还在？**

工作单元过滤器只跳过自己的提交，不会撤销已经提交的变更。检查异常前是否调用过 `SaveChanges` / `autoSave: true` / `ExecuteUpdateAsync`，或者是否开了事务但没有回滚。

**Q：登录成功但接口提示无权限？**

`[IdentifierAuthorize]` 校验的是 Redis 中的权限码集合。确认：用户角色已分配对应权限、权限码常量与数据库中的 `Code` 一致、登录后缓存已写入。角色/权限调整后需要重新登录或刷新 Token。

**Q：Swagger 看不到新接口或分组？**

确认控制器继承了正确的基类、类上有 `[ApiExplorer(ModuleEnum.X)]`、`ModuleEnum` 的新值有 `[Display]`，并且项目开启了 `GenerateDocumentationFile`。

**Q：Hangfire 任务里修改数据不生效？**

Hangfire 在 API 管道外执行，不会经过 `UnitOfWorkActionFilter`。注入 `IUnitOfWork<SolutionNameContext>` 并在最后 `await unitOfWork.SaveChangesAsync(ct)`。

**Q：启动时连接 Redis 失败？**

缓存、分布式锁、验证码、Hangfire、接口加密都依赖 Redis。检查 `ConnectionStrings:Redis` 和 `HangfireOptions:ConnectionString:Redis` 是否指向可用实例；不需要这些能力时至少保证 Hangfire 的存储可用。

**Q：迁移命令提示 EF Core 版本不匹配？**

见上文「EF Core 迁移命令」。`dotnet-ef` 工具版本需要和项目引用的 EF Core 版本匹配。

## 8. 参考资料

- 同目录 `AGENTS.md`：构建、迁移、工作单元、仓储和编码约定。
- `DSB.Framework.Lite.Core`：`ApiResult`、`ResultCode`、`BusinessException`、对象映射与序列化扩展。
- `DSB.Framework.Lite.WebApi.Extensions`：全局异常、请求日志、`ApiResultFilter`、JWT、权限码鉴权、Swagger、CORS。
- `DSB.Framework.Lite.Data.EFCore.Repository`：实体基类、仓储 API、分页、逻辑删除、工作单元。
- `DSB.Framework.Lite.Data.EFCore.Repository.WebApi`：`UnitOfWorkActionFilter`、`[UnitOfWorkIgnore]`。
- `DSB.Framework.Lite.Hangfire.Extensions`：Hangfire 注册与 Basic 认证仪表盘。
- `DSB.Framework.Lite.WebApi.ApiKeyAuthorization`：ApiKey 鉴权。
- `DSB.Framework.Lite.WebApi.EncryptionApi`：SM4 + 签名接口加密。
- `DSB.Framework.Lite.Caching.Extensions`：Redis 缓存、CSRedis、分布式锁。
- `DSB.Framework.Lite.Service.Extensions`：应用服务自动注册。
