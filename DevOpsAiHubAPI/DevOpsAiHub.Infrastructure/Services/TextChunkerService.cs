using System.Text;
using DevOpsAiHub.Application.Common.Interfaces.Services;

namespace DevOpsAiHub.Infrastructure.Services;

public class TextChunkerService : ITextChunkerService
{

    private readonly string[] _separators = { "\n\n", "\n", ". ", " ", "" };

    public IEnumerable<string> Chunk(string text, int chunkSize, int overlap)
    {
        if (string.IsNullOrWhiteSpace(text)) return Enumerable.Empty<string>();

        var cleaned = Normalize(text);
        return RecursiveChunking(cleaned, chunkSize, overlap, _separators);
    }

    private IEnumerable<string> RecursiveChunking(string text, int chunkSize, int overlap, string[] separators)
    {
        var finalChunks = new List<string>();

        string separator = separators.Last(); 
        string[] nextSeparators = Array.Empty<string>();

        for (int i = 0; i < separators.Length; i++)
        {
            if (separators[i] == "" || text.Contains(separators[i]))
            {
                separator = separators[i];
                nextSeparators = separators.Skip(i + 1).ToArray();
                break;
            }
        }

 
        var splits = separator == ""
            ? text.Select(c => c.ToString()).ToArray()
            : text.Split(new[] { separator }, StringSplitOptions.None);

        var currentDoc = new List<string>();
        int currentDocLength = 0;

        foreach (var split in splits)
        {
      
            if (split.Length > chunkSize)
            {
                if (currentDoc.Count > 0)
                {
                    var chunk = string.Join(separator, currentDoc).Trim();
                    if (!string.IsNullOrWhiteSpace(chunk)) finalChunks.Add(chunk);
                    currentDoc.Clear();
                    currentDocLength = 0;
                }

                if (nextSeparators.Length > 0)
                {
                    finalChunks.AddRange(RecursiveChunking(split, chunkSize, overlap, nextSeparators));
                }
                else
                { 
                    for (int i = 0; i < split.Length; i += chunkSize)
                    {
                        finalChunks.Add(split.Substring(i, Math.Min(chunkSize, split.Length - i)));
                    }
                }
                continue;
            }

            int separatorLength = currentDoc.Count > 0 ? separator.Length : 0;

            if (currentDocLength + split.Length + separatorLength > chunkSize && currentDoc.Count > 0)
            {
                var chunk = string.Join(separator, currentDoc).Trim();
                if (!string.IsNullOrWhiteSpace(chunk)) finalChunks.Add(chunk);

                while (currentDoc.Count > 0 && GetCurrentDocLength(currentDoc, separator) > overlap)
                {
                    currentDoc.RemoveAt(0);
                }
                currentDocLength = GetCurrentDocLength(currentDoc, separator);
            }

            currentDoc.Add(split);
            currentDocLength += split.Length + (currentDoc.Count > 1 ? separator.Length : 0);
        }

        if (currentDoc.Count > 0)
        {
            var chunk = string.Join(separator, currentDoc).Trim();
            if (!string.IsNullOrWhiteSpace(chunk)) finalChunks.Add(chunk);
        }

        return finalChunks;
    }

    private static int GetCurrentDocLength(List<string> currentDoc, string separator)
    {
        return currentDoc.Sum(c => c.Length) + Math.Max(0, currentDoc.Count - 1) * separator.Length;
    }

    private static string Normalize(string s)
    {
        var sb = new StringBuilder(s.Length);
        foreach (var ch in s)
            sb.Append(char.IsControl(ch) && ch != '\n' && ch != '\r' ? ' ' : ch);
        return sb.ToString().Replace("\r\n", "\n").Replace("\r", "\n");
    }
}