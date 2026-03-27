using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionName.Domain.Enums.Systems
{
    /// <summary>
    /// 权限类型
    /// </summary>
    public enum PermissionType
    {
        /// <summary>
        /// 目录
        /// </summary>
        [Description("目录")]
        Directory = 0,

        /// <summary>
        /// 菜单
        /// </summary>
        [Description("菜单")]
        Menu = 1,

        /// <summary>
        /// 按钮
        /// </summary>
        [Description("按钮")]
        Button = 2,

        /// <summary>
        /// 接口
        /// </summary>
        [Description("接口")]
        Api = 3,

        /// <summary>
        /// 标记
        /// </summary>
        [Description("标记")]
        Tag = 4,
    }
}
