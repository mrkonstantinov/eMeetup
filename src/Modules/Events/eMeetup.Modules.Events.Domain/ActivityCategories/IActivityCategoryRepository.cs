namespace eMeetup.Modules.Events.Domain.ActivityCategories;

public interface IActivityCategoryRepository
{
    Task<IEnumerable<ActivityCategory>> GetActivityCategories(CancellationToken cancellationToken = default);
}
