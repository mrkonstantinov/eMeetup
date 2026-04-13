using eMeetup.Modules.Attendance.Domain.Attendees;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMeetup.Modules.Attendance.Infrastructure.Attendees;

internal sealed class AttendeeConfiguration : IEntityTypeConfiguration<Attendee>
{
    public void Configure(EntityTypeBuilder<Attendee> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.UserName).HasMaxLength(200);

        builder.Property(c => c.Email).HasMaxLength(300);
    }
}
