namespace SolutionName.Domain;

/// <summary>
/// 权限码常量（按 目录 → 菜单 → 接口 的层级组织）
/// </summary>
public static class PermissionCodes
{
    #region 系统管理

    /// <summary>
    /// 系统管理（目录节点码）
    /// </summary>
    public const string System = "system";

    #region 用户管理

    /// <summary>
    /// 用户管理
    /// </summary>
    public static class User
    {
        #region 菜单节点码

        /// <summary>
        /// 菜单节点码
        /// </summary>
        public const string Menu = "system:user";

        #endregion

        #region 接口权限码

        /// <summary>
        /// 分页查询
        /// </summary>
        public const string Page = "user:page";

        /// <summary>
        /// 查看详情
        /// </summary>
        public const string Detail = "user:detail";

        /// <summary>
        /// 新增
        /// </summary>
        public const string Create = "user:create";

        /// <summary>
        /// 修改
        /// </summary>
        public const string Update = "user:update";

        /// <summary>
        /// 删除
        /// </summary>
        public const string Delete = "user:delete";

        /// <summary>
        /// 修改状态
        /// </summary>
        public const string Status = "user:status";

        #endregion
    }

    #endregion

    #region 角色管理

    /// <summary>
    /// 角色管理
    /// </summary>
    public static class Role
    {
        #region 菜单节点码

        /// <summary>
        /// 菜单节点码
        /// </summary>
        public const string Menu = "system:role";

        #endregion

        #region 接口权限码

        /// <summary>
        /// 全部角色列表
        /// </summary>
        public const string List = "role:list";

        /// <summary>
        /// 分页查询
        /// </summary>
        public const string Page = "role:page";

        /// <summary>
        /// 查看详情
        /// </summary>
        public const string Detail = "role:detail";

        /// <summary>
        /// 新增
        /// </summary>
        public const string Create = "role:create";

        /// <summary>
        /// 修改
        /// </summary>
        public const string Update = "role:update";

        /// <summary>
        /// 删除
        /// </summary>
        public const string Delete = "role:delete";

        /// <summary>
        /// 修改状态
        /// </summary>
        public const string Status = "role:status";

        #endregion
    }

    #endregion

    #region 权限管理

    /// <summary>
    /// 权限管理
    /// </summary>
    public static class PermissionManage
    {
        #region 菜单节点码

        /// <summary>
        /// 菜单节点码
        /// </summary>
        public const string Menu = "system:permission";

        #endregion

        #region 接口权限码

        /// <summary>
        /// 全部权限列表
        /// </summary>
        public const string List = "permission:list";

        /// <summary>
        /// 查看详情
        /// </summary>
        public const string Detail = "permission:detail";

        /// <summary>
        /// 新增
        /// </summary>
        public const string Create = "permission:create";

        /// <summary>
        /// 修改
        /// </summary>
        public const string Update = "permission:update";

        /// <summary>
        /// 删除
        /// </summary>
        public const string Delete = "permission:delete";

        /// <summary>
        /// 修改状态
        /// </summary>
        public const string Status = "permission:status";

        #endregion
    }

    #endregion

    #region 文件管理

    /// <summary>
    /// 文件管理
    /// </summary>
    public static class File
    {
        #region 菜单节点码

        /// <summary>
        /// 菜单节点码
        /// </summary>
        public const string Menu = "system:file";

        #endregion

        #region 接口权限码

        /// <summary>
        /// 上传文件
        /// </summary>
        public const string Upload = "file:upload";

        /// <summary>
        /// 查看配置
        /// </summary>
        public const string Config = "file:config";

        #endregion
    }

    #endregion

    #endregion
}
