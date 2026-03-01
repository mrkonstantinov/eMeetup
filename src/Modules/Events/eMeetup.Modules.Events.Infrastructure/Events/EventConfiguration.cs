using eMeetup.Modules.Events.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMeetup.Modules.Events.Infrastructure.Events;

internal sealed class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("events");

        builder.HasKey(u => u.Id);

        builder.OwnsOne(u => u.Location, locationBuilder =>
            locationBuilder.ConfigureLocation());
    }
}
