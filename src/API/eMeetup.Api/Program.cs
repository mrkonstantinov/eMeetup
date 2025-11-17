using System.Diagnostics;
using eMeetup.Api.Extensions;
using eMeetup.Api.Middleware;
using eMeetup.Common.Application;
using eMeetup.Common.Infrastructure;
using eMeetup.Common.Presentation.Endpoints;
using eMeetup.Modules.Users.Infrastructure;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
builder.AddApplicationServices();

builder.AddSeqEndpoint("seq", settings =>
{
    settings.DisableHealthChecks = true;
    settings.ServerUrl = "http://localhost:5341";
    settings.Logs.TimeoutMilliseconds = 10000;
});

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(t => t.FullName?.Replace("+", "."));
});

builder.Services.AddApplication([
    eMeetup.Modules.Users.Application.AssemblyReference.Assembly,
    //eMeetup.Modules.Ticketing.Application.AssemblyReference.Assembly,
    //eMeetup.Modules.Attendance.Application.AssemblyReference.Assembly
    ]);

builder.Services.AddInfrastructure(
    [
        //TicketingModule.ConfigureConsumers,
        //AttendanceModule.ConfigureConsumers
    ],
    builder.Configuration.GetConnectionString("meetupDb")!,
    builder.Configuration.GetConnectionString("Cache")!);

builder.Configuration.AddModuleConfiguration(["users"]);

builder.Services.AddUsersModule(builder.Configuration);
//builder.Services.AddTicketingModule(builder.Configuration);
//builder.Services.AddAttendanceModule(builder.Configuration);

WebApplication app = builder.Build();

ActivitySource source = new("eMeetup.Source");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    //app.ApplyMigrations();
}

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.UseAuthentication();

app.UseAuthorization();

app.MapEndpoints();

app.UseStaticFiles();

app.Run();

public partial class Program;
