namespace OpenMES.Domain;

/// <summary>
/// Placeholder role names used until real authentication wires in.
/// Kept as constants so consumers (UI, services) don't drift on spelling.
/// </summary>
public static class Roles
{
    public const string Operator = "Operator";
    public const string Technician = "Technician";
    public const string Quality = "Quality";
    public const string Supervisor = "Supervisor";
    public const string Admin = "Admin";

    public static readonly IReadOnlyList<string> All = new[]
    {
        Operator,
        Technician,
        Quality,
        Supervisor,
        Admin
    };
}
