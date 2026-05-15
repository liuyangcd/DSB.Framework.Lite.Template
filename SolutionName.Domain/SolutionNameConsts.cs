using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionName.Domain
{
    /// <summary>
    /// 项目使用的常量定义
    /// </summary>
    public static class SolutionNameConsts
    {
        /// <summary>
        /// 权限码常量
        /// </summary>
        public static class PermissionCodes
        {
            /// <summary>
            /// 用户 - 分页查询
            /// </summary>
            public const string UserPage = "user:page";

            /// <summary>
            /// 用户 - 查看详情
            /// </summary>
            public const string UserDetail = "user:detail";

            /// <summary>
            /// 用户 - 新增
            /// </summary>
            public const string UserCreate = "user:create";

            /// <summary>
            /// 用户 - 修改
            /// </summary>
            public const string UserUpdate = "user:update";

            /// <summary>
            /// 用户 - 删除
            /// </summary>
            public const string UserDelete = "user:delete";

            /// <summary>
            /// 用户 - 修改状态
            /// </summary>
            public const string UserStatus = "user:status";

            /// <summary>
            /// 角色 - 全部角色列表
            /// </summary>
            public const string RoleList = "role:list";

            /// <summary>
            /// 角色 - 分页查询
            /// </summary>
            public const string RolePage = "role:page";

            /// <summary>
            /// 角色 - 查看详情
            /// </summary>
            public const string RoleDetail = "role:detail";

            /// <summary>
            /// 角色 - 新增
            /// </summary>
            public const string RoleCreate = "role:create";

            /// <summary>
            /// 角色 - 修改
            /// </summary>
            public const string RoleUpdate = "role:update";

            /// <summary>
            /// 角色 - 删除
            /// </summary>
            public const string RoleDelete = "role:delete";

            /// <summary>
            /// 角色 - 修改状态
            /// </summary>
            public const string RoleStatus = "role:status";

            /// <summary>
            /// 权限 - 全部权限列表
            /// </summary>
            public const string PermissionList = "permission:list";

            /// <summary>
            /// 权限 - 查看详情
            /// </summary>
            public const string PermissionDetail = "permission:detail";

            /// <summary>
            /// 权限 - 新增
            /// </summary>
            public const string PermissionCreate = "permission:create";

            /// <summary>
            /// 权限 - 修改
            /// </summary>
            public const string PermissionUpdate = "permission:update";

            /// <summary>
            /// 权限 - 删除
            /// </summary>
            public const string PermissionDelete = "permission:delete";

            /// <summary>
            /// 权限 - 修改状态
            /// </summary>
            public const string PermissionStatus = "permission:status";

            /// <summary>
            /// 文件 - 上传
            /// </summary>
            public const string FileUpload = "file:upload";

            /// <summary>
            /// 文件 - 查看配置
            /// </summary>
            public const string FileConfig = "file:config";
        }

        /// <summary>
        /// 权限分组码常量（菜单节点，与 Controller/ModuleEnum 一一对应）
        /// </summary>
        public static class PermissionGroups
        {
            /// <summary>
            /// 系统管理（根目录）
            /// </summary>
            public const string System = "system";

            /// <summary>
            /// 用户管理
            /// </summary>
            public const string SystemUser = "system:user";

            /// <summary>
            /// 角色管理
            /// </summary>
            public const string SystemRole = "system:role";

            /// <summary>
            /// 权限管理
            /// </summary>
            public const string SystemPermission = "system:permission";

            /// <summary>
            /// 文件管理
            /// </summary>
            public const string SystemFile = "system:file";

            /// <summary>
            /// 管理员角色编码
            /// </summary>
            public const string AdminRole = "admin";
        }
    }
}
