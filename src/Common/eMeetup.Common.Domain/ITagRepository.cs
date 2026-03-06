namespace eMeetup.Common.Domain
{
    public interface ITagRepository<T>
    {
        Task<bool> TagExistsAsync(string tag, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> GetByTagsAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default);
    }
}
