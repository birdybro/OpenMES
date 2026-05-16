using OpenMES.Domain.Entities;

namespace OpenMES.PluginAbstractions;

public sealed record MaterialValidationResult(bool IsAllowed, string? Reason = null)
{
    public static MaterialValidationResult Allow() => new(true);
    public static MaterialValidationResult Deny(string reason) => new(false, reason);
}

public sealed record MaterialIssueContext(Job Job, MaterialLot Lot, decimal QuantityRequested);

/// <summary>
/// A pluggable check that runs before a lot may be issued to a job. Multiple
/// rules may be registered; the first denial wins.
/// </summary>
public interface IMaterialValidationRule
{
    Task<MaterialValidationResult> ValidateAsync(
        MaterialIssueContext context,
        CancellationToken cancellationToken = default);
}
