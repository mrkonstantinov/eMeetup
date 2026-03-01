using eMeetup.Modules.Events.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMeetup.Modules.Events.Infrastructure.Events;

public static class LocationConfiguration
{
    public static OwnedNavigationBuilder<T, Location> ConfigureLocation<T>(
        this OwnedNavigationBuilder<T, Location> builder) where T : class
    {
        builder.Property(l => l.Latitude)
            .IsRequired()
            .HasColumnType("decimal(9,6)");

        builder.Property(l => l.Longitude)
            .IsRequired()
            .HasColumnType("decimal(9,6)");

        builder.Property(l => l.City)
            .HasMaxLength(100);

        builder.Property(l => l.Street)
            .HasMaxLength(100);

        return builder;
    }
}
