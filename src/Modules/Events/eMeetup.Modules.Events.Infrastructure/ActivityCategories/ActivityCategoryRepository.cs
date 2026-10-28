using System;
using System.Collections.Generic;
using System.Text;
using eMeetup.Common.Application.Messaging;
using eMeetup.Modules.Events.Domain.ActivityCategories;
using eMeetup.Modules.Events.Infrastructure.Database;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace eMeetup.Modules.Events.Infrastructure.ActivityCategories;

internal sealed class ActivityCategoryRepository(EventsDbContext context) : IActivityCategoryRepository
{
    public async Task<IEnumerable<ActivityCategory>> GetActivityCategories(
        CancellationToken cancellationToken = default)
    {
        return await context.ActivityCategories.ToListAsync(cancellationToken);
    }
}
