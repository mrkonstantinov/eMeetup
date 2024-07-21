using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var db1 = builder.AddSqlServer("sql1").AddDatabase("db1");

builder.AddProject<Projects.eMeetupApi>("emeetupapi")
       .WithReference(db1);

builder.AddProject<Projects.eMeetupApi_MigrationService>("migration")
       .WithReference(db1);

builder.Build().Run();
