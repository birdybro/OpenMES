namespace OpenMES.Domain.Enums;

public enum ProductionEventType
{
    JobCreated = 0,
    JobReleased = 1,
    JobStarted = 2,
    JobPaused = 3,
    JobResumed = 4,
    MaterialIssued = 5,
    GoodQuantityReported = 6,
    ScrapQuantityReported = 7,
    DowntimeStarted = 8,
    DowntimeEnded = 9,
    QualityCheckCompleted = 10,
    OperationCompleted = 11,
    JobCompleted = 12,
    DocumentOpened = 13,
    BarcodeScanned = 14
}
