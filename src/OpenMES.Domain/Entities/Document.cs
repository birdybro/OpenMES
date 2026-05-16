using OpenMES.Domain.Enums;

namespace OpenMES.Domain.Entities;

/// <summary>
/// A reference to a document (work instruction, drawing, …). OpenMES stores the
/// metadata and a path or URL; it does not version or store the file itself.
/// </summary>
public class Document
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }

    public string? PartNumber { get; set; }
    public string? Revision { get; set; }
    public string? OperationCode { get; set; }
    public string? ResourceCode { get; set; }

    public DateTime? EffectiveDate { get; set; }
    public bool IsReleased { get; set; }
    public bool IsObsolete { get; set; }

    /// <summary>File path on a shared drive, or an HTTP(S) URL.</summary>
    public string UrlOrPath { get; set; } = string.Empty;

    public List<DocumentLink> Links { get; set; } = new();
}
