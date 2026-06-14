using System;
using System.Collections.Generic;

namespace SmartFarmSEP490.Model;

public partial class KnowledgeDocument
{
    public Guid Id { get; set; }

    public Guid? CropVarietyId { get; set; }

    public string Title { get; set; } = null!;

    public string? SourceUrl { get; set; }

    public string? FileUrl { get; set; }

    public Guid? UploadedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual CropVariety? CropVariety { get; set; }

    public virtual ICollection<KnowledgeDocumentChunk> KnowledgeDocumentChunks { get; set; } = new List<KnowledgeDocumentChunk>();

    public virtual User? UploadedByNavigation { get; set; }
}
