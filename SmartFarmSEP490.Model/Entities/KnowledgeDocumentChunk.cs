using System;
using System.Collections.Generic;

namespace SmartFarmSEP490.Model;

public partial class KnowledgeDocumentChunk
{
    public Guid Id { get; set; }

    public Guid DocumentId { get; set; }

    public int ChunkIndex { get; set; }

    public string Content { get; set; } = null!;

    public string? Embedding { get; set; }

    public string? Metadata { get; set; }

    public virtual KnowledgeDocument Document { get; set; } = null!;
}
