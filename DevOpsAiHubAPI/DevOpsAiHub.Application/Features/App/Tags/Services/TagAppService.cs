using DevOpsAiHub.Application.Common.Exceptions;
using DevOpsAiHub.Application.Common.Interfaces.Persistence;
using DevOpsAiHub.Application.Common.Interfaces.Repositories;
using DevOpsAiHub.Application.Common.Interfaces.Services;
using DevOpsAiHub.Application.Features.App.Tags.DTOs;
using DevOpsAiHub.Domain.Entities.Posts;

namespace DevOpsAiHub.Application.Features.App.Tags.Services;

public class TagAppService : ITagAppService
{
    private readonly ITagRepository _tagRepository;
    private readonly IDateTimeService _dateTimeService;
    private readonly IApplicationDbContext _context;

    public TagAppService(
        ITagRepository tagRepository,
        IDateTimeService dateTimeService,
        IApplicationDbContext context)
    {
        _tagRepository = tagRepository;
        _dateTimeService = dateTimeService;
        _context = context;
    }

    public async Task<List<TagDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tags = await _tagRepository.GetAllAsync(cancellationToken);

        return [.. tags.Select(MapToDto)];
    }

    public async Task<TagDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tag = await _tagRepository.GetByIdAsync(id, cancellationToken);
        return tag is null ? throw new NotFoundException("Tag not found.") : MapToDto(tag);
    }

    public async Task<TagDto> CreateAsync(CreateTagRequestDto request, CancellationToken cancellationToken = default)
    {
        var name = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new BadRequestException("Tag name is required.");

        var exists = await _tagRepository.ExistsByNameAsync(name, cancellationToken);
        if (exists)
            throw new BadRequestException("Tag name already exists.");

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = name,
            PostCount = 0,
            CreatedAt = _dateTimeService.UtcNow
        };

        await _tagRepository.AddAsync(tag, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(tag);
    }

    public async Task<TagDto> UpdateAsync(Guid id, UpdateTagRequestDto request, CancellationToken cancellationToken = default)
    {
        var name = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new BadRequestException("Tag name is required.");

        var tag = await _tagRepository.GetByIdAsync(id, cancellationToken);
        if (tag is null)
            throw new NotFoundException("Tag not found.");

        var existingTag = await _tagRepository.GetByNameAsync(name, cancellationToken);
        if (existingTag is not null && existingTag.Id != id)
            throw new BadRequestException("Tag name already exists.");

        tag.Name = name;

        _tagRepository.Update(tag);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(tag);
    }

    private static TagDto MapToDto(Tag tag)
    {
        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            PostCount = tag.PostCount,
            CreatedAt = tag.CreatedAt
        };
    }
}