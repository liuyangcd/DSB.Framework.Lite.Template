using DSB.Framework.Lite.Data.EFCore.Repository;
using Microsoft.EntityFrameworkCore;
using SolutionName.Domain.Entities.Systems;
using SolutionName.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace SolutionName.EntityFrameworkCore
{
    /// <summary>
    /// EFCore数据库上下文
    /// </summary>
    /// <param name="option"></param>
    public class SolutionNameContext(DbContextOptions<SolutionNameContext> option) : DbContext(option)
    {

        #region 数据表模型
        /// <summary>
        /// 用户
        /// </summary>
        public DbSet<SystemUserEntity> SystemUsers { get; set; }

        /// <summary>
        /// 角色
        /// </summary>
        public DbSet<SystemRoleEntity> SystemRoles { get; set; }

        /// <summary>
        /// 权限
        /// </summary>
        public DbSet<SystemPermissionEntity> SystemPermissions { get; set; }

        /// <summary>
        /// 用户角色关系
        /// </summary>
        public DbSet<SystemUserRoleEntity> SystemUserRoles { get; set; }

        /// <summary>
        /// 角色权限关系
        /// </summary>
        public DbSet<SystemRolePermissionEntity> SystemRolePermissions { get; set; }

        /// <summary>
        /// 上传文件
        /// </summary>
        public DbSet<SystemUploadFileEntity> SystemUploadFiles { get; set; }
        #endregion

        #region 模型约束配置
        /// <summary>
        /// 模型约束配置
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region 配置逻辑删除全局查询过滤器
            modelBuilder.ApplyDeprecateQueryFilter();
            #endregion

            #region 配置默认值
            /*
             * 取消设置默认值，改为在实体类中设置默认值，是因为有哨兵值问题。
             * 在EFCore中，Insert语句都会包含所有字段，即使是默认值字段也会包含在内，导致数据库默认值无法生效或者有哨兵值和实体属性默认值不一致问题。
             * 除非你手写Insert语句，缺省该字段。所以都通过在应用中设置实体默认值即可。
             */
            // modelBuilder.Entity<SystemUserEntity>().Property(u => u.Status).HasDefaultValue(UserStatus.Normally).HasSentinel(UserStatus.Normally); 
            #endregion

            #region 配置索引或唯一约束
            modelBuilder.Entity<SystemUserEntity>().HasIndex(u => u.CreateDateAt).IsDescending();
            modelBuilder.Entity<SystemUserEntity>().HasIndex(u => u.Account).IsUnique();

            modelBuilder.Entity<SystemRoleEntity>().HasIndex(r => r.Code).IsUnique();
            modelBuilder.Entity<SystemRoleEntity>().HasIndex(r => new { r.Sort, r.CreateDateAt }).IsDescending(false, true);

            modelBuilder.Entity<SystemPermissionEntity>().HasIndex(p => p.Code).IsUnique();
            modelBuilder.Entity<SystemPermissionEntity>().HasIndex(p => new { p.Sort, p.CreateDateAt }).IsDescending(false, true);

            modelBuilder.Entity<SystemUserRoleEntity>().HasIndex(ur => ur.RoleId);
            modelBuilder.Entity<SystemUserRoleEntity>().HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();

            modelBuilder.Entity<SystemRolePermissionEntity>().HasIndex(ur => ur.PermissionId);
            modelBuilder.Entity<SystemRolePermissionEntity>().HasIndex(ur => new { ur.RoleId, ur.PermissionId }).IsUnique();
            #endregion
        }
        #endregion

    }
}
