using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DevOpsAiHub.Application.Common.Interfaces.Services;
using DevOpsAiHub.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace DevOpsAiHub.Infrastructure.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;
    private readonly CloudinaryOptions _cloudinaryOptions;

    public CloudinaryService(IOptions<CloudinaryOptions> cloudinaryOptions)
    {
        _cloudinaryOptions = cloudinaryOptions.Value;

        var account = new Account(
            _cloudinaryOptions.CloudName,
            _cloudinaryOptions.ApiKey,
            _cloudinaryOptions.ApiSecret);

        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    public async Task<(string Url, string PublicId)> UploadImageAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = _cloudinaryOptions.AvatarFolder,
            UseFilename = true,
            UniqueFilename = true,
            Overwrite = false
        };

        var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

        if (result.Error is not null)
        {
            throw new Exception($"Cloudinary upload failed: {result.Error.Message}");
        }

        if (string.IsNullOrWhiteSpace(result.SecureUrl?.ToString()) ||
            string.IsNullOrWhiteSpace(result.PublicId))
        {
            throw new Exception("Cloudinary upload failed: invalid response.");
        }

        return (result.SecureUrl.ToString(), result.PublicId);
    }

    public async Task DeleteImageAsync(
    string publicId,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(publicId))
            return;

        var deleteParams = new DeletionParams(publicId)
        {
            ResourceType = ResourceType.Image
        };

        var result = await _cloudinary.DestroyAsync(deleteParams);

        if (result.Error is not null)
        {
            throw new Exception($"Cloudinary delete failed: {result.Error.Message}");
        }
    }
}