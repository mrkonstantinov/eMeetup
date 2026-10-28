namespace eMeetup.Modules.Events.Application.ActivityCategories.GetActivityCategory;

public sealed record ActivityCategoryResponse(
    int Id,
    string Name,
    string Slug,
    string? Icon,
    string? Color,
    int? ParentId,
    string? ParentName,
    int DisplayOrder
    );

public sealed record ActivityCategoryTreeResponse(
    int Id,
    string Name,
    string Slug,
    string? Icon,
    string? Color,
    int? ParentId,
    int DisplayOrder,
    IReadOnlyCollection<ActivityCategoryTreeResponse> Children);
