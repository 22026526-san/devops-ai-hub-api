namespace DevOpsAiHub.Application.Common.Interfaces.Services;

public interface ICloudinaryService
{
    Task<(string Url, string PublicId)> UploadImageAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task DeleteImageAsync(string publicId, CancellationToken cancellationToken = default);
}