using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionName.Domain.Enums
{
    /// <summary>
    /// 记录状态
    /// </summary>
    public enum RecordStatus : byte
    {
        /// <summary>
        /// 禁用
        /// </summary>
        [Description("禁用")]
        Disabled = 0,

        /// <summary>
        /// 正常
        /// </summary>
        [Description("正常")]
        Normally = 1,
    }
}
