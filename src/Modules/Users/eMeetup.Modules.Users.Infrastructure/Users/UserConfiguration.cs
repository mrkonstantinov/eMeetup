using eMeetup.Modules.Users.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMeetup.Modules.Users.Infrastructure.Users;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.UserName).HasMaxLength(200);        

        builder.Property(u => u.Email).HasMaxLength(300);

        builder.Property(u => u.Bio).HasMaxLength(500);

        //builder.Property(e => e.CreatedAt)
        //            .HasColumnType("timestamp with time zone")
        //            .HasDefaultValueSql("CURRENT_TIMESTAMP")
        //            .ValueGeneratedOnAdd();

        //builder.Property(e => e.UpdatedAt)
        //    .HasColumnType("timestamp with time zone")
        //    .HasDefaultValueSql("CURRENT_TIMESTAMP")
        //    .ValueGeneratedOnAddOrUpdate();

        builder.OwnsOne(u => u.Location, locationBuilder =>
            locationBuilder.ConfigureLocation()
        );

        builder.HasIndex(u => u.Email).IsUnique();

        builder.HasIndex(u => u.IdentityId).IsUnique();
    }
}
