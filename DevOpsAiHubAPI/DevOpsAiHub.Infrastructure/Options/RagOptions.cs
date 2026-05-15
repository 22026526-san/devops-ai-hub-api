namespace DevOpsAiHub.Infrastructure.Options;

internal class RagOptions
{
    public int TopK_QA { get; set; }
    public int TopK_Text { get; set; }
    public float  MinScore { get; set; } 
    public int ChunkSize { get; set; }
    public int ChunkOverlap { get; set; } 
    public int TopK_Rerank { get; set; } 
    
}
