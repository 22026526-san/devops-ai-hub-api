using System.Text;
using System.Text.RegularExpressions;
using DevOpsAiHub.Application.Common.Interfaces.Services;

namespace DevOpsAiHub.Infrastructure.Services;

public class SlugService : ISlugService
{
    public string GenerateSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Guid.NewGuid().ToString();

        var normalized = input.ToLowerInvariant().Trim();
        normalized = Regex.Replace(normalized, @"\s+", "-");
        normalized = Regex.Replace(normalized, @"[^a-z0-9\-]", "");
        normalized = Regex.Replace(normalized, @"-+", "-").Trim('-');

        return string.IsNullOrWhiteSpace(normalized)
            ? Guid.NewGuid().ToString()
            : normalized;
    }
}