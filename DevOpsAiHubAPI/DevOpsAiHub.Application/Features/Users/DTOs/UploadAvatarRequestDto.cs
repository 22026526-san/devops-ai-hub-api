using Microsoft.AspNetCore.Http;

namespace DevOpsAiHub.Application.Features.Users.DTOs
{
    public class UploadAvatarRequestDto
    {
        public IFormFile File { get; set; }
    }
}
