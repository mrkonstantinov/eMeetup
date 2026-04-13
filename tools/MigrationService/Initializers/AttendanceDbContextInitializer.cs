using eMeetup.Modules.Attendance.Infrastructure.Database;

namespace MigrationService.Initializers;

internal class AttendanceDbContextInitializer : DbContextInitializerBase<AttendanceDbContext>
{
    public AttendanceDbContextInitializer(AttendanceDbContext dbContext) : base(dbContext)
    {
    }
}
