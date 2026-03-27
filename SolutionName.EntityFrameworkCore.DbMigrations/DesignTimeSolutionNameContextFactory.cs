using DSB.Framework.Lite.Data.EFCore.Extensions;
using DSB.Framework.Lite.Data.EFCore.Extensions.Setting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionName.EntityFrameworkCore.DbMigrations
{
    public class DesignTimeSolutionNameContextFactory : IDesignTimeDbContextFactory<SolutionNameContext>
    {
        public SolutionNameContext CreateDbContext(string[] args)
        {
            var migrationsAssemblyName = "SolutionName.EntityFrameworkCore.DbMigrations";
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var efDbContextOptions = configuration.GetSection("EntityFrameworkCoreOptions").Get<EntityFrameworkCoreDbContextOptions>() ?? throw new ArgumentNullException("未找到数据库配置信息");
            var optionsBuilder = new DbContextOptionsBuilder<SolutionNameContext>();
            switch (efDbContextOptions.DbType)
            {
                case DbType.MySql:
                    efDbContextOptions.MySQLOptionsAction = options => options.MigrationsAssembly(migrationsAssemblyName);
                    break;
                case DbType.SqlServer:
                    efDbContextOptions.SqlServerOptionsAction = options => options.MigrationsAssembly(migrationsAssemblyName);
                    break;
                case DbType.Oracle:
                    efDbContextOptions.OracleOptionsAction = options => options.MigrationsAssembly(migrationsAssemblyName);
                    break;
                case DbType.PostgreSQL:
                    efDbContextOptions.NpgsqlOptionsAction = options => options.MigrationsAssembly(migrationsAssemblyName);
                    break;
                default: throw new NotImplementedException("不支持该数据库类型");
            }
            optionsBuilder.UseDataBase(efDbContextOptions);
            return new SolutionNameContext(optionsBuilder.Options);
        }
    }
}
