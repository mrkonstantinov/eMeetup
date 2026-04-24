using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var keycloak = builder.AddKeycloak("keycloak", 6001)
    .WithDataVolume("keycloak-data")
    .WithExternalHttpEndpoints()
    .WithArgs("--features", "account-api");

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
    .WithDataVolume(isReadOnly: false)
    .WithPgWeb(); 
var meetupDb = postgres.AddDatabase("meetupDb");

// Добавляем контейнер Jaeger
var jaeger = builder.AddContainer("jaeger", "jaegertracing/all-in-one")
    .WithHttpEndpoint(16686, targetPort: 16686, name: "jaegerPortal")   // Веб-интерфейс Jaeger
    .WithHttpEndpoint(4317, targetPort: 4317, name: "jaegerEndpoint");  // OTLP gRPC порт

var migrationService = builder.AddProject<Projects.MigrationService>("migrations")
    .WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", "http://localhost:4317")  // уже правильно
    //.WithEnvironment("Seq__ServerUrl", "http://localhost:5341")  // Добавьте это
    .WithReference(meetupDb)
    .WaitFor(postgres);

builder
    .AddProject<Projects.eMeetup_Api>("api")
    .WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", "http://localhost:4317")
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(seq)
    .WaitFor(seq)
    .WithExternalHttpEndpoints()
    .WithReference(meetupDb)
    .WaitForCompletion(migrationService)
    .WithReference(keycloak);

builder.Build().Run();
