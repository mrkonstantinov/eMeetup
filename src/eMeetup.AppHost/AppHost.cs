using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var keycloak = builder.AddKeycloak("keycloak", 6001)
    .WithDataVolume("keycloak-data")
    .WithExternalHttpEndpoints();

var redis = builder.AddRedis("redis", 6379);

var seq = builder.AddSeq("seq", 5341)
                 .ExcludeFromManifest()
                 .WithLifetime(ContainerLifetime.Persistent)
                 .WithEnvironment("ACCEPT_EULA", "Y")
                 .WithDataVolume();

var username = builder.AddParameter("username", "postgres", publishValueAsDefault: false, secret: true);
var password = builder.AddParameter("password", "postgres", publishValueAsDefault: false, secret: true);

var postgres = builder.AddPostgres(name: "postgres", userName: username, password: password, port: 5435)
    .WithImage("postgis/postgis")
    .WithImageTag("latest")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume(isReadOnly: false);

var meetupDb = postgres.AddDatabase("meetupDb");

var migrationService = builder.AddProject<Projects.MigrationService>("migrations")
    .WithReference(meetupDb)
    .WaitFor(postgres);

builder
    .AddProject<Projects.eMeetup_Api>("api")
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(seq)
    .WaitFor(seq)
    .WithExternalHttpEndpoints()
    .WithReference(meetupDb)
    .WaitForCompletion(migrationService)
    .WithReference(keycloak);

builder.Build().Run();
