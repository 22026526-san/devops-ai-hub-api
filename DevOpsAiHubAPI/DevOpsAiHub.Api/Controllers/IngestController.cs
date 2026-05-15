using DevOpsAiHub.Application.Features.AI.UseCase;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using UglyToad.PdfPig;

namespace DevOpsAiHub.Api.Controllers;

[ApiController]
[Route("api/ingest")]
public class IngestController : ControllerBase
{
    private readonly IngestDocumentUseCase _useCase;
    private readonly ILogger<IngestController> _logger;

    public IngestController(
        IngestDocumentUseCase useCase,
        ILogger<IngestController> logger)
    {
        _useCase = useCase;
        _logger = logger;
    }

    /// <summary>
    /// Upload file để ingest vào Qdrant collection devops_articles
    /// Hỗ trợ: .pdf, .txt, .md
    /// </summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [DisableRequestSizeLimit]
    [RequestFormLimits(MultipartBodyLengthLimit = 1024L * 1024L * 200L)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload(
        [FromForm] List<IFormFile> files,
        CancellationToken ct)
    {
        if (files is null || files.Count == 0)
            return BadRequest(new { error = "No files uploaded." });

        var extracted = new List<(string RawText, string FileName, string FileType)>();

        foreach (var file in files)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext is not (".pdf" or ".txt" or ".md"))
            {
                _logger.LogWarning("Skipped unsupported file: {FileName}", file.FileName);
                continue;
            }

            var tmp = Path.GetTempFileName();
            try
            {
                await using (var fs = System.IO.File.Create(tmp))
                    await file.CopyToAsync(fs, ct);

                var raw = ext switch
                {
                    ".pdf" => ExtractPdf(tmp),
                    ".txt" or ".md" => await System.IO.File.ReadAllTextAsync(tmp, ct),
                    _ => string.Empty
                };

                if (!string.IsNullOrWhiteSpace(raw))
                {
                    extracted.Add((raw, file.FileName, ext.TrimStart('.')));
                    _logger.LogInformation(
                        "Extracted {Chars} chars from {FileName}", raw.Length, file.FileName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract {FileName}", file.FileName);
            }
            finally
            {
                System.IO.File.Delete(tmp);
            }
        }

        if (extracted.Count == 0)
            return BadRequest(new { error = "No extractable text. Supported: .pdf, .txt, .md" });

        var result = await _useCase.ExecuteAsync(extracted, ct);
        return Ok(result);
    }

    private static string ExtractPdf(string path)
    {
        var sb = new StringBuilder();
        using var doc = PdfDocument.Open(path);
        foreach (var page in doc.GetPages())
            sb.AppendLine(page.Text);
        return sb.ToString();
    }
}