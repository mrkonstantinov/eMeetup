using eMeetup.Modules.Users.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MigrationService;
using MigrationService.Initializers;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHostedService<Worker>();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(Worker.ActivitySourceName));


// Access IConfiguration directly from the builder
IConfiguration configuration = builder.Configuration;
string databaseConnectionString = configuration.GetConnectionString("meetupDb")!;

builder.Services.AddScoped<UsersDbContextInitializer>();

builder.Services.AddDbContext<UsersDbContext>(options =>
{
    options.UseNpgsql(databaseConnectionString,
        npgsqlOptions => { })
        .UseSnakeCaseNamingConvention();
});

//builder.Services.AddScoped<TicketingDbContextInitializer>();
//builder.AddNpgsqlDbContext<TicketingDbContext>("meetupDb");

var host = builder.Build();

host.Run();
