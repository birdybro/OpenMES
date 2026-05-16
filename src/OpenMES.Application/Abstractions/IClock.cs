namespace OpenMES.Application.Abstractions;

/// <summary>
/// Indirection over <see cref="DateTime.UtcNow"/> so services can be tested
/// deterministically.
/// </summary>
public interface IClock
{
    DateTime UtcNow { get; }
}

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
