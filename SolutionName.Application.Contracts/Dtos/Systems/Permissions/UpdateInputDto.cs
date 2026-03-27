namespace SolutionName.Application.Contracts.Dtos.Systems.Permissions;

public class UpdateInputDto : CreateInputDto
{
    /// <summary>
    /// 权限Id
    /// </summary>
    public Guid Id { get; set; }
}