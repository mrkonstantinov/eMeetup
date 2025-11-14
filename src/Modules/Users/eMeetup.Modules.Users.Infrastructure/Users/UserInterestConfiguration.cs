using eMeetup.Modules.Users.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMeetup.Modules.Users.Infrastructure.Users;

internal sealed class UserInterestConfiguration : IEntityTypeConfiguration<UserInterest>
{
    public void Configure(EntityTypeBuilder<UserInterest> builder)
    {
        builder.ToTable("user_interests");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .ValueGeneratedOnAdd();

        builder.Property(i => i.UserId)
            .IsRequired();

        builder.Property(i => i.Interest)
            .IsRequired()
            .HasMaxLength(100);

        // Navigation property
        builder.HasOne(i => i.User)
            .WithMany(u => u.Interests)
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(i => i.UserId);

        builder.HasIndex(i => i.Interest);

        builder.HasIndex(i => new { i.UserId, i.Interest })
            .IsUnique();
    }
}
