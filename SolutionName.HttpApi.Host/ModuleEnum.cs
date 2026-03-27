using System.ComponentModel.DataAnnotations;

namespace SolutionName.HttpApi.Host
{
    /// <summary>
    /// 系统模块
    /// </summary>
    public enum ModuleEnum
    {
        /// <summary>
        /// 用户管理
        /// </summary>
        [Display(Name = nameof(User), Description = "用户管理", Order = (int)User)]
        User,

        /// <summary>
        /// 角色管理
        /// </summary>
        [Display(Name = nameof(Role), Description = "角色管理", Order = (int)Role)]
        Role,

        /// <summary>
        /// 权限管理
        /// </summary>
        [Display(Name = nameof(Permission), Description = "权限管理", Order = (int)Permission)]
        Permission,

        /// <summary>
        /// 文件管理
        /// </summary>
        [Display(Name = nameof(File), Description = "文件管理", Order = (int)File)]
        File,

        /// <summary>
        /// 系统管理
        /// </summary>
        [Display(Name = nameof(System), Description = "系统管理", Order = (int)System)]
        System
    }
}