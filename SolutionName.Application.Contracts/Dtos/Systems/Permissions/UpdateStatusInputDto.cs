using DSB.Framework.Lite.Core.Attributes;
using SolutionName.Domain.Enums;

namespace SolutionName.Application.Contracts.Dtos.Systems.Permissions;

public class UpdateStatusInputDto
{
    /// <summary>
    /// 权限Id
    /// </summary>
    [GuidRequired(ErrorMessage = "用户Id不能为空")]
    public Guid Id { get; set; }

    /// <summary>
    /// 权限状态
    /// </summary>
    public RecordStatus Status { get; set; }
}