namespace OpenMES.Domain.Entities;

/// <summary>
/// Placeholder identity. Real auth will replace this — keep dependencies on it
/// shallow until then.
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Role { get; set; } = Roles.Operator;
    public bool IsActive { get; set; } = true;
}
