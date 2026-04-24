namespace eMeetup.Modules.Events.Domain.TagGroups;

public interface ITagGroupRepository
{
    Task<TagGroup?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<string?> GetPictureFileNameByIdAsync(int id, CancellationToken cancellationToken = default);
}
