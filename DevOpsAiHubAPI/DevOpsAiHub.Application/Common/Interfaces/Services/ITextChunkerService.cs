namespace DevOpsAiHub.Application.Common.Interfaces.Services;

public interface ITextChunkerService
{
    IEnumerable<string> Chunk(string text, int chunkSize, int overlap);
}