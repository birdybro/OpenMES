using OpenMES.Domain.Enums;

namespace OpenMES.Domain.Entities;

public class Resource
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ResourceType ResourceType { get; set; }
    public string? Location { get; set; }
    public bool IsActive { get; set; } = true;
}
