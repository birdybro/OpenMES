namespace OpenMES.Application.Sync;

public sealed class SyncReport
{
    public int Fetched { get; set; }
    public int Inserted { get; set; }
    public int Updated { get; set; }
    public int Skipped { get; set; }
    public int Errors { get; set; }
    public List<string> SkipReasons { get; } = new();
    public List<string> ErrorMessages { get; } = new();
    public DateTime? FinishedUtc { get; set; }
}
