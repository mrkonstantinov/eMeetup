using eMeetup.Common.Infrastructure.Inbox;
using eMeetup.Common.Infrastructure.Outbox;
using eMeetup.Modules.Attendance.Application.Abstractions.Data;
using eMeetup.Modules.Attendance.Domain.Attendees;
using eMeetup.Modules.Attendance.Infrastructure.Attendees;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace eMeetup.Modules.Attendance.Infrastructure.Database;

//Add-Migration InitialMigration -Context AttendanceDbContext -Project eMeetup.Modules.Attendance.Infrastructure -OutputDir Database\Migrations
public sealed class AttendanceDbContext(DbContextOptions<AttendanceDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<Attendee> Attendees { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Attendance);

        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConsumerConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConsumerConfiguration());

        modelBuilder.ApplyConfiguration(new AttendeeConfiguration());
        //modelBuilder.ApplyConfiguration(new EventStatisticsConfiguration());
    }
}
