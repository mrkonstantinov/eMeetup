var builder = DistributedApplication.CreateBuilder(args);

// Add Seq as a container
var seq = builder.AddSeq("seq", 5341)
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithDataVolume();

var keycloak = builder.AddKeycloak("keycloak", 6001)
    .WithDataVolume("keycloak-data");

builder.Build().Run();
