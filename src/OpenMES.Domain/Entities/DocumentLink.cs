using OpenMES.Domain.Enums;

namespace OpenMES.Domain.Entities;

/// <summary>
/// Explicit link between a document and a scope (Part / Revision / Operation / Resource).
/// A document may have any number of links; the resolver picks the most specific.
/// </summary>
public class DocumentLink
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public Document? Document { get; set; }

    public DocumentScope Scope { get; set; }

    /// <summary>
    /// Key for the link — part number for Part, "PART/REV" for Revision,
    /// "PART/REV/OPCODE" for Operation, resource code for Resource.
    /// </summary>
    public string ScopeKey { get; set; } = string.Empty;
}
