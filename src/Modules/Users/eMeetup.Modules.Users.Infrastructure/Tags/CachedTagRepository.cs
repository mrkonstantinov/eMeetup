using eMeetup.Modules.Users.Domain.Interfaces.Repositories;
using eMeetup.Modules.Users.Domain.Users;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace eMeetup.Modules.Users.Infrastructure.Tags;

//public class CachedTagRepository : ITagRepository
//{
//    private readonly ITagRepository _decoratedRepository;
//    private readonly IMemoryCache _cache;
//    private readonly ILogger<CachedTagRepository> _logger;
//    private readonly MemoryCacheEntryOptions _cacheOptions;

//    public CachedTagRepository(
//        ITagRepository decoratedRepository,
//        IMemoryCache cache,
//        ILogger<CachedTagRepository> logger)
//    {
//        _decoratedRepository = decoratedRepository;
//        _cache = cache;
//        _logger = logger;
//        _cacheOptions = new MemoryCacheEntryOptions()
//            .SetSlidingExpiration(TimeSpan.FromMinutes(30))
//            .SetAbsoluteExpiration(TimeSpan.FromHours(1));
//    }

//    public async Task<Tag?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
//    {
//        var cacheKey = $"tag_slug_{slug}";

//        if (_cache.TryGetValue<Tag>(cacheKey, out var cachedTag))
//        {
//            _logger.LogDebug("Cache hit for tag slug: {Slug}", slug);
//            return cachedTag;
//        }

//        _logger.LogDebug("Cache miss for tag slug: {Slug}", slug);
//        var tag = await _decoratedRepository.GetBySlugAsync(slug, cancellationToken);

//        if (tag != null)
//        {
//            _cache.Set(cacheKey, tag, _cacheOptions);
//        }

//        return tag;
//    }

//    public async Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
//    {
//        var cacheKey = $"tag_id_{id}";

//        if (_cache.TryGetValue<Tag>(cacheKey, out var cachedTag))
//            return cachedTag;

//        var tag = await _decoratedRepository.GetByIdAsync(id, cancellationToken);

//        if (tag != null)
//        {
//            _cache.Set(cacheKey, tag, _cacheOptions);
//        }

//        return tag;
//    }

    

//    private void InvalidateCache(Guid? tagId = null, string? slug = null)
//    {
//        // Invalidate all tags cache
//        _cache.Remove("all_tags");
//        _cache.Remove("popular_tags");
//        _cache.Remove("active_tags");

//        // Invalidate specific tag cache
//        if (tagId.HasValue)
//            _cache.Remove($"tag_id_{tagId}");

//        if (!string.IsNullOrEmpty(slug))
//            _cache.Remove($"tag_slug_{slug}");
//    }

//    // Pass-through implementations for other methods...
//    public Task<IEnumerable<Tag>> GetAsync(CancellationToken cancellationToken = default)
//        => _decoratedRepository.GetAsync(cancellationToken);

//    // ... implement other pass-through methods
//}
