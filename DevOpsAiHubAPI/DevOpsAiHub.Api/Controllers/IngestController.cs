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

    public IngestController(IngestDocumentUseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [DisableRequestSizeLimit]
    [RequestFormLimits(MultipartBodyLengthLimit = 1024L * 1024L * 200L)]
    public async Task<IActionResult> Upload(
        [FromForm] List<IFormFile> files,
        CancellationToken ct)
    {
        if (files is null || files.Count == 0)
            return BadRequest(new { error = "No files uploaded." });

        var extracted = new List<(string RawText, string FileName, string FileType)>();
        var failedFiles = new List<object>();

        foreach (var file in files)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext is not (".pdf" or ".txt" or ".md"))
            {
                failedFiles.Add(new { fileName = file.FileName, reason = "Unsupported file type. Only .pdf, .txt, .md are allowed." });
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
                }
                else
                {
                    failedFiles.Add(new { fileName = file.FileName, reason = "File is empty or contains no extractable text." });
                }
            }
            catch (Exception ex)
            {
                failedFiles.Add(new { fileName = file.FileName, reason = $"Extraction failed: {ex.Message}" });
            }
            finally
            {
                System.IO.File.Delete(tmp);
            }
        }
        if (extracted.Count == 0)
        {
            return BadRequest(new
            {
                error = "No files could be extracted successfully.",
                failedDetails = failedFiles
            });
        }

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