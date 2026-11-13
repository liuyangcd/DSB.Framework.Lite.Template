# 部署文档

本文档说明 `src/SolutionName.HttpApi.Host` 的两种部署方式：

- **Windows IIS 服务器部署**：适用于 Windows Server / 内网服务器，使用 IIS + ASP.NET Core Hosting Bundle。
- **Linux Docker 容器部署**：适用于 Linux 服务器，使用 Docker 运行，可直连 HTTPS 或由 Nginx 终止 TLS。

部署前请先阅读第 1 节的通用准备和第 4 节的安全清单。文中的域名、路径、端口、密码均为示例，请按实际环境替换。

---

## 1. 部署前准备

### 1.1 外部依赖

应用启动时会连接以下外部服务，必须在启动前准备好：

| 依赖 | 用途 | 说明 |
| --- | --- | --- |
| 数据库 | 业务数据 | MySQL（模板默认）/ SqlServer / PostgreSQL / Oracle |
| Redis | 缓存、分布式锁、验证码、Hangfire 存储、接口加密配置 | 未配置 Redis 时，验证码、权限缓存、Hangfire 等功能不可用，Hangfire 可能直接启动失败 |
| 反向代理 / TLS | HTTPS、域名、限流 | IIS 可直接终止 TLS；Docker 可用 Nginx 终止 TLS |

首次启动会执行数据种子（权限、管理员角色、管理员账号）。数据库表结构必须先创建，否则种子服务会失败。

### 1.2 生产配置

仓库中的 `appsettings.Production.json` 默认是空对象，必须补齐配置。可以采用两种方式：

1. **配置文件**：在服务器应用目录中维护一份 `appsettings.Production.json`（IIS 站点根目录 / Docker 的 `/app`），不要用发布包中的空文件覆盖它。
2. **环境变量**：使用 `__` 分隔层级，适合 Docker 和 CI/CD。例如：

```text
EntityFrameworkCoreOptions__DbType=MySql
EntityFrameworkCoreOptions__ConnectionString__MySql=Server=db;Port=3306;Database=SolutionName;User=app;Password=***;
ConnectionStrings__Redis=redis:6379,defaultDatabase=1
JwtBearerSetting__SecurityKey=***
```

必填配置：

| 配置节 | 是否必填 | 说明 |
| --- | --- | --- |
| `EntityFrameworkCoreOptions` | 是 | 数据库类型和连接串，缺失会启动失败 |
| `ConnectionStrings:Redis` | 是 | 缓存、锁、验证码、Hangfire、加密配置 |
| `JwtBearerSetting` | 是 | JWT 签发与校验，缺失会启动失败 |
| `HangfireOptions` | 是 | 定时任务和仪表盘，缺失会启动失败 |
| `ApiKeyOptions` | 按需 | 使用 ApiKey 鉴权的接口必须配置 |
| `CorsOptions` | 按需 | 前后端分离时必须配置 |
| `FileUploadOptions` | 按需 | 上传大小、保存目录、文件类型 |
| `HttpClientOptions` | 按需 | 命名 HttpClient 的 BaseAddress 等 |

完整的配置项可以参考 `appsettings.Development.json`，但不要把开发环境的连接串、密钥、账号密码直接用于生产。

> `EncryptionApiOptionManager.Register` 目前在 `Program.cs` 中硬编码了演示用的 AppId / SecretKey / Sm4Key。生产环境必须替换这些值；如果要动态管理，可改为从数据库加载后调用 `Register`。它不读取 `appsettings.json`。

### 1.3 数据库迁移

应用不会自动执行 EF Core 迁移。首次部署和每次升级前，先创建/更新数据库结构。

**方式一：在开发机或构建机执行**

修改 `src/SolutionName.EntityFrameworkCore.DbMigrations/appsettings.json` 中的连接串，然后从仓库根目录执行：

```bash
dotnet ef database update \
  --project ./src/SolutionName.EntityFrameworkCore.DbMigrations/ \
  --startup-project ./src/SolutionName.EntityFrameworkCore.DbMigrations/
```

**方式二：生成幂等 SQL 脚本，交给 DBA 执行（推荐生产）**

```bash
dotnet ef migrations script -i -o ./SQL/production.sql \
  --project ./src/SolutionName.EntityFrameworkCore.DbMigrations/ \
  --startup-project ./src/SolutionName.EntityFrameworkCore.DbMigrations/
```

**方式三：生成迁移可执行文件**

```bash
dotnet ef migrations bundle \
  --project ./src/SolutionName.EntityFrameworkCore.DbMigrations/ \
  --startup-project ./src/SolutionName.EntityFrameworkCore.DbMigrations/ \
  -o ./efbundle
```

把 `efbundle` 上传到服务器执行即可，不需要服务器安装 .NET SDK。如果目标服务器没有 .NET 运行时，生成时加 `--self-contained`。执行前务必备份数据库。

> 以上迁移命令使用 bash 的行尾 `\` 换行；Windows PowerShell 请换成反引号，或直接写成一行。

> `DesignTimeSolutionNameContextFactory` 只读取 DbMigrations 项目下的 `appsettings.json`，不读取环境变量。生产连接串建议用 SQL 脚本或 `efbundle` 配合命令行参数处理，避免把生产密码写进仓库。

### 1.4 证书

- **IIS**：把 PFX 导入“本地计算机 -> 个人 -> 证书”，在 IIS 站点绑定中选择该证书即可。IIS 终止 TLS，不需要给 Kestrel 配证书。
- **Docker 直连 HTTPS**：把 PFX 挂载到容器 `/https`，通过环境变量传入密码。
- **Docker + Nginx**：证书放在 Nginx 侧，容器只跑 HTTP，推荐这种方式。

模板中的 `Certs/localhost.pfx` 和 `Certs/password.txt` 仅用于本地调试，不要用于生产环境，也不要提交生产证书和密码。

### 1.5 启动顺序

1. 数据库准备完成，迁移已执行。
2. Redis 可用，连接串正确。
3. 生产配置（`appsettings.Production.json` 或环境变量）已就位。
4. 证书和 HTTPS 绑定已配置。
5. 启动应用。
6. 检查健康检查、日志、登录、权限、上传。
7. 首次登录后立即修改 `admin` 默认密码。

---

## 2. Windows IIS 部署

### 2.1 安装 IIS 与 Hosting Bundle

1. 在“服务器管理器 -> 添加角色和功能”中安装 **Web Server (IIS)**。
2. 建议勾选：
   - 常见 HTTP 功能：默认文档、静态内容、HTTP 错误。
   - 管理工具：IIS 管理控制台。
   - 如需 WebSocket，勾选 WebSocket 协议。
3. 安装 **.NET 8 Hosting Bundle**（包含 ASP.NET Core Runtime 和 ASP.NET Core Module V2）。
4. 安装完成后重启 IIS：

```powershell
net stop was /y
net start w3svc
```

或直接执行 `iisreset`。

> 如果先装了 Hosting Bundle、后装 IIS，需要修复（Repair）Hosting Bundle，确保 IIS 中注册了 ASP.NET Core Module V2。可以在 IIS 的“模块”列表中确认 `AspNetCoreModuleV2` 是否存在。

### 2.2 发布应用

在开发机或构建机的仓库根目录执行：

```powershell
dotnet publish .\src\SolutionName.HttpApi.Host\SolutionName.HttpApi.Host.csproj -c Release -o .\publish\Production
```

也可以使用仓库中的发布配置：

```powershell
dotnet publish .\src\SolutionName.HttpApi.Host\SolutionName.HttpApi.Host.csproj -c Release -p:PublishProfile=Production
```

发布配置的输出目录是 `src/SolutionName.HttpApi.Host/bin/Release/net8.0/publish/Production`。

发布后检查：

- 根目录存在 `SolutionName.HttpApi.Host.dll` 和 `web.config`。
- `web.config` 中的 `ASPNETCORE_ENVIRONMENT` 是 `Production`（发布配置的 `EnvironmentName` 会写入；如果没有，手动补充）。
- `appsettings.Production.json` 是空模板，不要直接覆盖服务器上的生产配置。
- 发布包可能包含 `appsettings.Development.json` 和 `Certs/password.txt`，生产服务器上应删除它们；即使 Production 环境不会加载 Development 文件，文件里的开发密钥也不应留在服务器上。IIS 使用证书存储，不需要发布目录中的 `Certs`。

把发布文件复制到服务器，例如 `D:\Sites\SolutionName`。不要直接使用 `robocopy /MIR` 覆盖生产目录，否则可能删除 `logs`、`UploadFiles` 和服务器上的生产配置。建议先备份，再增量复制。

### 2.3 创建应用程序池

在 IIS 管理器中：

1. 新建应用程序池，例如 `SolutionNamePool`。
2. **.NET CLR 版本**：选择 **无托管代码**（No Managed Code）。
3. **托管管道模式**：集成（Integrated）。
4. **启用 32 位应用程序**：False。
5. **启动模式**：AlwaysRunning（可选，减少冷启动）。
6. **空闲超时**：0（可选，避免空闲回收）。
7. **标识**：ApplicationPoolIdentity（默认）。

如果数据库使用 Windows 集成认证，需要给 `IIS AppPool\SolutionNamePool` 授予数据库登录权限；模板默认使用用户名/密码连接，不涉及这一步。

### 2.4 创建站点

1. 新建网站，物理路径指向发布目录，例如 `D:\Sites\SolutionName`。
2. 绑定类型选择 `https`，端口 443，填写主机名，选择证书。
3. 应用程序池选择 `SolutionNamePool`。
4. 可选：安装 “Application Initialization” 功能后，把站点的 **预加载已启用** 设为 True，配合应用程序池的 AlwaysRunning 减少首次请求等待。

### 2.5 目录权限

给应用程序池标识授予读取和必要的写入权限：

```powershell
# 先创建日志和上传目录
New-Item -ItemType Directory -Force -Path "D:\Sites\SolutionName\logs" | Out-Null
New-Item -ItemType Directory -Force -Path "D:\Sites\SolutionName\UploadFiles" | Out-Null

icacls "D:\Sites\SolutionName" /grant "IIS AppPool\SolutionNamePool:(OI)(CI)(RX)" /T
icacls "D:\Sites\SolutionName\logs" /grant "IIS AppPool\SolutionNamePool:(OI)(CI)(M)" /T
icacls "D:\Sites\SolutionName\UploadFiles" /grant "IIS AppPool\SolutionNamePool:(OI)(CI)(M)" /T
```

- `logs` 用于 Serilog 文件日志。
- `UploadFiles` 是 `FileUploadOptions:SavePath` 的默认值；如果改成绝对路径，给对应目录授权。
- 如果启用了 `stdoutLogEnabled`，`logs` 目录也必须可写。

### 2.6 生产配置

在服务器站点根目录维护 `appsettings.Production.json`，或使用系统环境变量。模板没有从外部路径加载配置的代码；如果一定要把配置放在站点目录之外，需要在 `Program.cs` 中显式 `AddJsonFile(绝对路径, optional: true)`，否则应用读不到。发布前备份、发布后恢复，避免被空模板覆盖。

如果使用 `web.config` 设置环境变量，结构如下（只放非敏感配置）：

```xml
<aspNetCore processPath="dotnet"
            arguments=".\SolutionName.HttpApi.Host.dll"
            stdoutLogEnabled="false"
            stdoutLogFile=".\logs\stdout"
            hostingModel="inprocess">
  <environmentVariables>
    <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
  </environmentVariables>
</aspNetCore>
```

> `web.config` 是明文文件，不要在这里写数据库密码、JWT 密钥等敏感信息。敏感配置应放在服务器环境变量、密钥管理服务或独立的 `appsettings.Production.json` 中并限制文件权限。

### 2.7 HTTPS 与 HTTP 跳转

- IIS 站点绑定 443 并选择证书后，IIS 负责 TLS，应用内部不需要再配置 Kestrel 证书。
- 当前 `Program.cs` 中 `app.UseHttpsRedirection()` 被注释掉了。如果同时绑定了 80 和 443，需要：
  - 在 IIS 中安装 URL Rewrite，添加 HTTP -> HTTPS 的重定向规则；或
  - 取消 `UseHttpsRedirection` 注释，并配置 `HttpsRedirection:HttpsPort=443`（IIS 进程内托管下，这个配置比 `ASPNETCORE_HTTPS_PORTS` 更可靠）。
- 生产环境建议只保留 443 绑定，或在网关层统一跳转。

### 2.8 上传大小限制

`Program.cs` 会把 `FileUploadOptions:FileMaxSize` 同步到 Kestrel、IIS 和 Form 表单限制，但 **IIS 的请求筛选是独立配置**，默认 `maxAllowedContentLength` 约为 30 MB。上传大文件时必须同时调整 IIS 限制。

在站点的 `web.config` 中加入（数值单位是字节）：

```xml
<system.webServer>
  <security>
    <requestFiltering>
      <requestLimits maxAllowedContentLength="1100000000" />
    </requestFiltering>
  </security>
</system.webServer>
```

- 上面的 1100000000 字节约 1.1 GB，略大于 `FileMaxSize = 1024` MB 的应用限制，给 multipart 边界留余量。
- 发布新包可能覆盖 `web.config`，部署后要重新确认该配置，或通过 IIS 管理器的“请求筛选”功能修改。
- 如果 `requestFiltering` 节被服务器锁定，需要在 IIS 管理器中解锁，或在 `applicationHost.config` 中配置。

### 2.9 验证部署

```powershell
# 健康检查，正常返回 200 和 Healthy
Invoke-WebRequest https://api.example.com/ -UseBasicParsing

# 查看应用日志目录
Get-ChildItem D:\Sites\SolutionName\logs
```

然后验证：

1. 生产环境 Swagger 默认关闭，访问 Swagger 返回 404 是正常的。
2. 先调用 `GET /api/Auth/GetCaptcha` 获取验证码，再调用 `POST /api/Auth/Login` 登录（生产环境会校验验证码）。
3. 用返回的 Token 调用需要权限的接口。
4. 检查 Hangfire 仪表盘（由 `UseHangfireDashboard` 注册，默认路径通常为 `/hangfire`，以框架实际配置为准）。
5. 检查数据库种子是否成功、管理员账号是否可登录。
6. 首次登录后立即修改 `admin` 默认密码。

### 2.10 升级与回滚

1. 备份当前发布目录、`appsettings.Production.json`、`logs`、`UploadFiles`。
2. 执行数据库迁移（先备份数据库）。
3. 在站点根目录放一个 `app_offline.htm`，让应用优雅下线。
4. 覆盖发布文件（不要覆盖生产配置、日志、上传目录）。
5. 删除 `app_offline.htm`。
6. 回收应用程序池或重启站点。
7. 验证健康检查和登录。
8. 保留上一版本目录，回滚时切回即可。

### 2.11 IIS 常见问题

| 现象 | 常见原因 | 处理 |
| --- | --- | --- |
| HTTP 500.30 | 应用启动失败 | 查看 `logs`、事件查看器；检查生产配置、数据库、Redis 是否可用 |
| HTTP 500.31 | 找不到 .NET 运行时 | 安装/修复 .NET 8 Hosting Bundle |
| HTTP 502.5 | 进程启动失败 | 查看事件查看器和 `stdout` 日志；检查运行时和配置 |
| HTTP 500.19 | `web.config` 配置错误或被锁定 | 检查 XML、模块、IIS 配置锁定 |
| 生产访问 Swagger 404 | 代码只在非 Production 启用 Swagger | 正常现象；需要文档时用 Staging 环境 |
| 上传返回 413 | IIS `maxAllowedContentLength` 太小 | 按 2.8 调整请求筛选 |
| 接口一直 401/403 | JWT 配置错误或权限码未分配 | 检查 `JwtBearerSetting`、角色权限、Redis 权限缓存 |
| 日志目录为空 | 目录不存在或应用池无写权限 | 创建 `logs` 并授予 Modify 权限 |
| 部署后配置丢失 | 发布包覆盖了生产 `appsettings.Production.json` | 从备份恢复，改用发布目录外的配置文件或环境变量 |
| 首次请求很慢 | 应用池空闲回收/冷启动 | 设置 AlwaysRunning、Idle Time-out=0、预加载 |

---

## 3. Linux Docker 部署

### 3.1 服务器准备

- Linux 服务器（Ubuntu 22.04/24.04、Debian 12、Rocky Linux 9 等）。
- Docker Engine 24+，可选 Docker Compose v2。
- 开放计划使用的端口，例如 8090（HTTP）、8091（HTTPS），或只开放 443 给 Nginx。
- 与数据库、Redis 的网络连通性。
- 建议设置时区 `Asia/Shanghai`，或依赖镜像中的 `TZ` 环境变量。

> 本节命令以 Linux bash 为例；Windows 构建机请使用对应的 PowerShell 命令（如 `Copy-Item` 代替 `cp`）。

```bash
docker --version
docker compose version
```

### 3.2 方式 A：使用仓库自带的 Dockerfile.Https

这种方式先在宿主机发布，再把发布目录作为构建上下文。适合没有 CI、但构建机安装了 .NET 8 SDK 的场景。

```bash
# 1. 在仓库根目录发布
dotnet publish ./src/SolutionName.HttpApi.Host/SolutionName.HttpApi.Host.csproj \
  -c Release -o ./publish/Production

# 2. 把 Dockerfile 放进发布目录（如果发布输出没有自动带上）
cp ./src/SolutionName.HttpApi.Host/Dockerfile.Https ./publish/Production/

# 3. 构建镜像
cd ./publish/Production
docker build -t solutionname-api:1.0.0 -f ./Dockerfile.Https .
```

说明：

- `Dockerfile.Https` 使用 `mcr.microsoft.com/dotnet/aspnet:8.0`，暴露 8080/8081，证书路径固定为 `/https/localhost.pfx`。
- 镜像内默认 `ASPNETCORE_ENVIRONMENT=Development`，运行时用 `-e ASPNETCORE_ENVIRONMENT=Production` 覆盖。
- 不要把生产证书打进镜像；运行时挂载到 `/https`。
- 发布目录中如果包含 `appsettings.Development.json`、`Certs/password.txt`、`Certs/localhost.pfx` 等敏感/演示文件，构建前删除，避免进入镜像。
- 不要把生产 `appsettings.Production.json` 打进镜像；生产配置用运行时挂载或环境变量提供。

### 3.3 方式 B：多阶段构建（推荐 CI/CD）

在仓库根目录新建 `Dockerfile.Production`（文件名可自定），内容示例：

```dockerfile
# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /repo

# 先复制 csproj，利用 NuGet 缓存
COPY src/SolutionName.HttpApi.Host/SolutionName.HttpApi.Host.csproj src/SolutionName.HttpApi.Host/
COPY src/SolutionName.Application/SolutionName.Application.csproj src/SolutionName.Application/
COPY src/SolutionName.Application.Contracts/SolutionName.Application.Contracts.csproj src/SolutionName.Application.Contracts/
COPY src/SolutionName.EntityFrameworkCore/SolutionName.EntityFrameworkCore.csproj src/SolutionName.EntityFrameworkCore/
COPY src/SolutionName.Domain/SolutionName.Domain.csproj src/SolutionName.Domain/
RUN dotnet restore src/SolutionName.HttpApi.Host/SolutionName.HttpApi.Host.csproj

COPY . .
RUN dotnet publish src/SolutionName.HttpApi.Host/SolutionName.HttpApi.Host.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080 \
    TZ=Asia/Shanghai
COPY --from=build /app/publish .
RUN mkdir -p /app/logs /app/UploadFiles
EXPOSE 8080
ENTRYPOINT ["dotnet", "SolutionName.HttpApi.Host.dll"]
```

在仓库根目录构建：

```bash
docker build -f ./Dockerfile.Production -t solutionname-api:1.0.0 .
```

建议同时添加 `.dockerignore`，避免把本地构建产物和敏感配置带进镜像：

```text
**/bin
**/obj
publish/
tests/
**/logs
**/UploadFiles
.git
.vs
.idea
**/appsettings.Development.json
**/appsettings.Staging.json
**/appsettings.Production.json
**/Certs
```

> 如果使用方式 B 且需要容器直连 HTTPS，把 `ASPNETCORE_HTTPS_PORTS=8081` 和 Kestrel 证书环境变量加到运行命令中即可。

### 3.4 准备目录和配置

```bash
sudo mkdir -p /opt/solutionname/{logs,uploads,certs,config}
sudo chmod 700 /opt/solutionname/certs
# 把生产配置放到 /opt/solutionname/config/appsettings.Production.json
# 把 PFX 放到 /opt/solutionname/certs/localhost.pfx（直连 HTTPS 时需要）
```

- 容器内日志目录是 `/app/logs`，上传目录默认是 `/app/UploadFiles`。
- 如果修改了 `FileUploadOptions:SavePath` 为绝对路径，挂载对应目录。
- 生产配置建议挂载为只读文件，不要把密钥写进镜像。
- 如果使用非 root 用户运行容器，需要给挂载目录相应的 UID 权限（.NET 8 镜像内置 `app` 用户，UID 1654）。

### 3.5 启动容器（直连 HTTPS）

使用方式 A 的 `Dockerfile.Https` 或方式 B 增加 HTTPS 配置后：

```bash
docker run -d --name solutionname-api \
  --restart unless-stopped \
  -p 8090:8080 -p 8091:8081 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ASPNETCORE_Kestrel__Certificates__Default__Password='***' \
  -v /opt/solutionname/certs:/https:ro \
  -v /opt/solutionname/logs:/app/logs \
  -v /opt/solutionname/uploads:/app/UploadFiles \
  -v /opt/solutionname/config/appsettings.Production.json:/app/appsettings.Production.json:ro \
  --log-opt max-size=10m \
  --log-opt max-file=3 \
  solutionname-api:1.0.0
```

如果不需要对外暴露 HTTP，去掉 `-p 8090:8080`。如果不使用配置文件、改用环境变量，删除 `appsettings.Production.json` 的挂载。如果证书文件名不是 `localhost.pfx`，追加：

```bash
-e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/your-cert.pfx
```

启动后检查：

```bash
docker ps
curl -k -i https://127.0.0.1:8091/
curl -i http://127.0.0.1:8090/
docker logs --tail 100 solutionname-api
```

> Production 下 Serilog 没有控制台输出（代码中控制台 sink 仅在 DEBUG 编译），`docker logs` 可能很少或为空。日志看挂载出来的 `/opt/solutionname/logs/`。

### 3.6 启动容器（HTTP + Nginx，推荐）

生产环境更常见的是 Nginx 终止 TLS，容器只跑 HTTP。下面的命令假设镜像是 HTTP 版本（`Dockerfile` 或 `Dockerfile.Production`）。如果使用 `Dockerfile.Https` 构建的镜像，镜像内已经设置了 HTTPS 端口和证书路径，要么同时挂载证书并暴露 8081，要么重新用 HTTP Dockerfile 构建。

```bash
docker run -d --name solutionname-api \
  --restart unless-stopped \
  -p 127.0.0.1:8090:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -v /opt/solutionname/logs:/app/logs \
  -v /opt/solutionname/uploads:/app/UploadFiles \
  -v /opt/solutionname/config/appsettings.Production.json:/app/appsettings.Production.json:ro \
  --log-opt max-size=10m \
  --log-opt max-file=3 \
  solutionname-api:1.0.0
```

只绑定 `127.0.0.1`，避免 8090 直接暴露到公网。

Nginx 配置示例：

```nginx
server {
    listen 80;
    server_name api.example.com;
    return 301 https://$host$request_uri;
}

server {
    listen 443 ssl http2;
    server_name api.example.com;

    ssl_certificate     /etc/letsencrypt/live/api.example.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/api.example.com/privkey.pem;

    # 略大于 FileUploadOptions:FileMaxSize（1024 MB），给 multipart 边界留余量
    client_max_body_size 1100m;

    location / {
        proxy_pass http://127.0.0.1:8090;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_read_timeout 300s;
    }
}
```

注意事项：

- `client_max_body_size` 必须大于等于应用的上传限制，否则 Nginx 会先返回 413。上面的示例按 1 GB 上限配置。
- 如果 Nginx 也在 Docker 中，`proxy_pass` 要改成应用的服务名（例如 `http://api:8080`），并让两个容器处于同一 Docker network。
- 应用已经启用 `UseForwardedHeaders`，但默认只信任回环代理。Nginx 和应用在同一台主机时，`127.0.0.1` 属于默认信任范围；如果 Nginx 在另一个容器或另一台机器，需要在 `Program.cs` 中配置 `KnownNetworks` / `KnownProxies`，否则日志和 `ClientIP` 会是代理地址。
- 如果 Hangfire 仪表盘需要 WebSocket/长连接，按实际需要补充 `Upgrade` 和 `Connection` 头。
- 建议把 Hangfire 仪表盘限制在内网或加 IP 白名单，并使用强密码。

### 3.7 Docker Compose 示例

如果使用 Compose，可参考：

```yaml
services:
  api:
    # 使用 HTTP 版本的镜像（Dockerfile 或 Dockerfile.Production）
    image: solutionname-api:1.0.0
    container_name: solutionname-api
    restart: unless-stopped
    env_file:
      - .env
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ASPNETCORE_HTTP_PORTS: "8080"
    ports:
      - "127.0.0.1:8090:8080"
    volumes:
      - ./logs:/app/logs
      - ./uploads:/app/UploadFiles
      - ./config/appsettings.Production.json:/app/appsettings.Production.json:ro
    depends_on:
      - redis

  redis:
    image: redis:7-alpine
    container_name: solutionname-redis
    restart: unless-stopped
    command: ["redis-server", "--appendonly", "yes"]
    volumes:
      - ./redis-data:/data
```

`.env` 示例（不要提交到仓库）：

```dotenv
EntityFrameworkCoreOptions__DbType=MySql
# 如果 mysql 是 Compose 服务名，用 mysql；否则改成实际数据库地址
EntityFrameworkCoreOptions__ConnectionString__MySql=Server=mysql;Port=3306;Database=SolutionName;User=app;Password=***;
ConnectionStrings__Redis=redis:6379,defaultDatabase=1
JwtBearerSetting__SecurityKey=***
JwtBearerSetting__RefreshSecurityKey=***
HangfireOptions__ConnectionString__Redis=redis:6379,defaultDatabase=5
```

启动：

```bash
docker compose up -d
docker compose logs -f api
```

> 如果数据库也在 Compose 中，把服务名（如 `mysql`）作为连接串主机名。容器内的 `127.0.0.1` 指向容器自身，不是宿主机。宿主机上的数据库/Redis 需要用宿主机 IP 或 `host.docker.internal`（Linux 需 `--add-host=host.docker.internal:host-gateway`）。

### 3.8 验证部署

```bash
# 直连 HTTP
curl -i http://127.0.0.1:8090/

# 直连 HTTPS
curl -k -i https://127.0.0.1:8091/

# 经 Nginx
curl -i https://api.example.com/
```

正常返回 200 和健康检查文本。注意：当前 `AddHealthChecks()` 没有注册数据库/Redis 检查，`/` 返回 Healthy 只代表进程存活，数据库和 Redis 仍需单独验证。然后验证：

1. 登录接口（生产环境需要验证码）。
2. 需要权限码的接口。
3. 文件上传/下载。
4. Hangfire 仪表盘。
5. 日志文件是否写入 `/opt/solutionname/logs/`。
6. 数据库种子是否成功。

### 3.9 升级与回滚

1. 备份数据库、`uploads`、`config`、`certs`。
2. 执行数据库迁移。
3. 构建新镜像并使用版本号标签，例如 `solutionname-api:1.0.1`；不要在生产使用 `latest`。
4. 停止旧容器，启动新容器：

```bash
docker stop solutionname-api
docker rm solutionname-api
# 用新镜像重新执行 docker run
```

5. 验证健康检查和核心接口。
6. 保留旧镜像和旧容器配置，回滚时用旧标签重新启动。

Compose 方式：如果镜像来自镜像仓库，执行：

```bash
docker compose pull
docker compose up -d
```

如果镜像是本地构建的，先重新 `docker build`，再执行 `docker compose up -d`。

### 3.10 Docker 常见问题

| 现象 | 常见原因 | 处理 |
| --- | --- | --- |
| 容器启动后立即退出 | 配置缺失、数据库/Redis 连不上、证书错误 | `docker logs solutionname-api`，检查 `appsettings.Production.json` |
| 502 Bad Gateway | 容器没监听、端口映射错误、Nginx 地址错误 | `curl http://127.0.0.1:8090/`；检查 `docker ps` 和 `proxy_pass` |
| 413 Request Entity Too Large | Nginx `client_max_body_size` 太小 | 调到与应用上传限制一致 |
| 上传失败/文件不存在 | 上传目录没挂载或路径不一致 | 检查 `FileUploadOptions:SavePath` 和 volume |
| 日志目录为空 | 挂载路径错误或权限不足 | 检查 `/app/logs` 挂载和 UID 权限 |
| `docker logs` 没内容 | Production 下 Serilog 不输出控制台 | 查看挂载的日志文件 |
| 证书加载失败 | PFX 路径/密码错误、证书没有私钥 | 检查 `ASPNETCORE_Kestrel__Certificates__Default__*` |
| 时间不正确 | 容器时区不是东八区 | 设置 `TZ=Asia/Shanghai`，必要时安装 tzdata |
| 连不上数据库/Redis | 容器内 `localhost` 指向容器自身 | 使用服务名、宿主机 IP 或 `host.docker.internal` |
| 权限不足/无法写文件 | 挂载目录 UID 不匹配、SELinux 限制 | `chown` 对应 UID；SELinux 加 `:Z` 挂载选项 |

---

## 4. 生产安全清单

- [ ] 替换 JWT `SecurityKey` / `RefreshSecurityKey`，不要使用模板值。
- [ ] 替换接口加密的 AppId / SecretKey / Sm4Key，不要使用 `Program.cs` 中的演示值。
- [ ] 修改 `admin` 默认密码（默认 `123456`）。
- [ ] 设置强密码的 Hangfire 账号，并限制仪表盘访问来源。
- [ ] 替换 `ApiKeyOptions` 的默认 Key，按对接方分配独立 Key。
- [ ] 生产 `appsettings.Production.json` / `.env` 不提交仓库，文件权限最小化。
- [ ] 数据库、Redis 使用独立账号和强密码，按最小权限授权。
- [ ] IIS 对外开放 443；如需 HTTP 跳转再开放 80。Docker 只暴露必要端口，HTTP 端口绑定 127.0.0.1 交给 Nginx。
- [ ] 配置 HTTPS 证书并启用 HTTP -> HTTPS 跳转。
- [ ] CORS `Hosts` 按实际前端域名配置，不要使用 `*`。
- [ ] 生产环境不启用 Swagger、不启用请求日志（当前代码已按 Environment 自动处理）。
- [ ] 上传目录禁止执行脚本，限制文件类型和大小。
- [ ] 配置日志轮转和磁盘告警；Docker 日志加 `max-size` / `max-file`。
- [ ] 定期备份数据库、上传目录和生产配置。
- [ ] 监控健康检查 `/`、接口错误率、数据库/Redis 连接和磁盘空间。
- [ ] 保留上一版本发布目录/镜像，准备回滚方案。

---

## 5. 附录

### 5.1 环境变量对照表

| 配置 | 环境变量 | 示例 |
| --- | --- | --- |
| 运行环境 | `ASPNETCORE_ENVIRONMENT` | `Production` |
| HTTP 端口 | `ASPNETCORE_HTTP_PORTS` | `8080` |
| HTTPS 端口 | `ASPNETCORE_HTTPS_PORTS` | `8081` |
| 数据库类型 | `EntityFrameworkCoreOptions__DbType` | `MySql` |
| MySQL 连接串 | `EntityFrameworkCoreOptions__ConnectionString__MySql` | `Server=...;Database=...;User=...;Password=...;` |
| Redis | `ConnectionStrings__Redis` | `redis:6379,defaultDatabase=1` |
| JWT Issuer | `JwtBearerSetting__Issuer` | `SolutionName` |
| JWT Audience | `JwtBearerSetting__Audience` | `SolutionName` |
| JWT 密钥 | `JwtBearerSetting__SecurityKey` | `***` |
| JWT 刷新密钥 | `JwtBearerSetting__RefreshSecurityKey` | `***` |
| Hangfire 账号 | `HangfireOptions__Account` | `admin` |
| Hangfire 密码 | `HangfireOptions__Password` | `***` |
| Hangfire Redis | `HangfireOptions__ConnectionString__Redis` | `redis:6379` |
| ApiKey | `ApiKeyOptions__Default` | `***` |
| CORS 开关 | `CorsOptions__IsCors` | `true` |
| CORS 第一个来源 | `CorsOptions__Hosts__0` | `https://app.example.com` |
| 上传根目录 | `FileUploadOptions__SavePath` | `UploadFiles` |
| 上传大小（MB） | `FileUploadOptions__FileMaxSize` | `1024` |
| Kestrel 证书路径 | `ASPNETCORE_Kestrel__Certificates__Default__Path` | `/https/localhost.pfx` |
| Kestrel 证书密码 | `ASPNETCORE_Kestrel__Certificates__Default__Password` | `***` |

> 环境变量只适合覆盖简单值；数组和字典（如 `CorsOptions:Hosts`、`FileTypeOptions`）建议用配置文件维护。

### 5.2 生产配置骨架

```json
{
  "EntityFrameworkCoreOptions": {
    "DbType": "MySql",
    "ConnectionString": {
      "MySql": "Server=db;Port=3306;Database=SolutionName;User=app;Password=***;"
    }
  },
  "ConnectionStrings": {
    "Redis": "redis:6379,defaultDatabase=1"
  },
  "JwtBearerSetting": {
    "Issuer": "SolutionName",
    "Audience": "SolutionName",
    "SecurityKey": "***",
    "Expire": 8,
    "ExpireType": "Hours",
    "RefreshSecurityKey": "***",
    "RefreshExpire": 30,
    "RefreshExpireType": "Days",
    "UserInfoDesKey": "***"
  },
  "HangfireOptions": {
    "Account": "admin",
    "Password": "***",
    "StorageType": "Redis",
    "ConnectionString": {
      "Redis": "redis:6379"
    },
    "RedisStorageOptions": {
      "Db": 5,
      "Prefix": "SolutionName:hangfire:"
    }
  },
  "ApiKeyOptions": {
    "Default": "***"
  },
  "CorsOptions": {
    "IsCors": true,
    "Hosts": ["https://app.example.com"]
  },
  "FileUploadOptions": {
    "FileMaxSize": 1024,
    "SavePath": "UploadFiles",
    "FileTypeOptions": {
      "Pictrue": {
        "Extensions": ".jpg,.jpeg,.png,.gif,.bmp",
        "MaxSize": 20
      },
      "Document": {
        "Extensions": ".doc,.docx,.xls,.xlsx,.json,.txt,.pdf",
        "MaxSize": 100
      }
    }
  }
}
```

> `FileTypeOptions` 只是示例，按实际需要补充 Video、Audio、Attachment 等类型。

### 5.3 端口与路径速查

| 项目 | IIS | Docker |
| --- | --- | --- |
| HTTP | 80 或站点绑定端口 | 容器 8080，映射如 8090 |
| HTTPS | 443（IIS 终止 TLS） | 容器 8081，映射如 8091，或由 Nginx 终止 |
| 应用目录 | `D:\Sites\SolutionName` | `/app` |
| 日志 | `D:\Sites\SolutionName\logs` | `/app/logs` |
| 上传 | `D:\Sites\SolutionName\UploadFiles` | `/app/UploadFiles` |
| 证书 | IIS 证书存储 | `/https` |
| 健康检查 | `https://域名/` | `http://127.0.0.1:8090/` |

---

部署完成后，建议把实际的域名、端口、配置路径、备份策略和回滚步骤补充到本项目的运维记录中。
