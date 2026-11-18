using System.Text.Json;
using DSB.Framework.Lite.Data.EFCore.Extensions;
using DSB.Framework.Lite.Data.EFCore.Extensions.Setting;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using SolutionName.EntityFrameworkCore;
using ConnectionStringSetting = DSB.Framework.Lite.Data.EFCore.Extensions.Setting.ConnectionString;

namespace SolutionName.Tests.Integration.MySql;

/// <summary>
/// MySQL 集成测试配置。
/// <para>
/// 连接信息从 <c>src/SolutionName.HttpApi.Host/appsettings.Development.json</c> 读取，
/// 但数据库名会替换为独立的测试库，避免测试数据污染开发库。
/// </para>
/// </summary>
internal static class MySqlTestSettings
{
    /// <summary>
    /// 测试专用数据库名称。每次测试运行前会删除并重建该库。
    /// </summary>
    public const string TestDatabaseName = "SolutionName_RepositoryTests";

    private static readonly Lazy<EntityFrameworkCoreDbContextOptions> Options = new(CreateOptionsCore);

    private static readonly Lazy<DbContextOptions<SolutionNameContext>> ContextOptions = new(() =>
    {
        var builder = new DbContextOptionsBuilder<SolutionNameContext>();
        builder.UseDataBase(Options.Value, false);
        return builder.Options;
    });

    /// <summary>
    /// 获取指向测试库的 EF Core 配置。
    /// </summary>
    public static EntityFrameworkCoreDbContextOptions GetOptions() => Options.Value;

    /// <summary>
    /// 创建一个指向测试库的数据库上下文。
    /// </summary>
    public static SolutionNameContext CreateContext() => new(ContextOptions.Value);

    private static EntityFrameworkCoreDbContextOptions CreateOptionsCore()
    {
        var connectionString = BuildTestConnectionString();

        return new EntityFrameworkCoreDbContextOptions
        {
            DbType = DbType.MySql,
            ConnectionString = new ConnectionStringSetting
            {
                MySql = connectionString
            }
        };
    }

    /// <summary>
    /// 读取开发环境连接串并替换为测试库。
    /// </summary>
    private static string BuildTestConnectionString()
    {
        var configurationFile = Path.Combine(FindRepositoryRoot(), "src", "SolutionName.HttpApi.Host", "appsettings.Development.json");
        if (!File.Exists(configurationFile))
        {
            throw new InvalidOperationException($"未找到开发环境配置文件：{configurationFile}");
        }

        using var document = JsonDocument.Parse(File.ReadAllText(configurationFile));
        var developmentConnectionString = document.RootElement
            .GetProperty("EntityFrameworkCoreOptions")
            .GetProperty("ConnectionString")
            .GetProperty("MySql")
            .GetString();

        if (string.IsNullOrWhiteSpace(developmentConnectionString))
        {
            throw new InvalidOperationException("appsettings.Development.json 中未配置 EntityFrameworkCoreOptions:ConnectionString:MySql");
        }

        var builder = new MySqlConnectionStringBuilder(developmentConnectionString)
        {
            Database = TestDatabaseName
        };

        return builder.ConnectionString;
    }

    /// <summary>
    /// 从测试程序集输出目录向上查找仓库根目录。
    /// </summary>
    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "SolutionName.Backend.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException($"未能从 {AppContext.BaseDirectory} 向上找到仓库根目录（SolutionName.Backend.slnx）");
    }
}
