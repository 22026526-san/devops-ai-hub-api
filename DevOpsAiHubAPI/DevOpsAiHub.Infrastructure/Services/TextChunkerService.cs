namespace DevOpsAiHub.Infrastructure.Services;
using DevOpsAiHub.Application.Common.Interfaces.Services;
using System.Text;

public class TextChunkerService : ITextChunkerService
{
    public IEnumerable<string> Chunk(string text, int chunkSize, int overlap)
    {
        if (string.IsNullOrWhiteSpace(text)) yield break;

        var cleaned = Normalize(text);
        int start = 0;

        while (start < cleaned.Length)
        {
            int end = Math.Min(start + chunkSize, cleaned.Length);
            int boundary = cleaned.LastIndexOfAny(
                new[] { '.', '!', '?', '\n' }, end - 1, Math.Min(150, end - start));

            if (boundary > start + 100) end = boundary + 1;

            var chunk = cleaned[start..end].Trim();
            if (!string.IsNullOrWhiteSpace(chunk))
                yield return chunk;

            if (end >= cleaned.Length) break;
            start = Math.Max(0, end - overlap);
        }
    }

    private static string Normalize(string s)
    {
        var sb = new StringBuilder(s.Length);
        foreach (var ch in s)
            sb.Append(char.IsControl(ch) && ch != '\n' && ch != '\r' ? ' ' : ch);
        return sb.ToString().Replace("\r\n", "\n").Replace("\r", "\n");
    }
}
