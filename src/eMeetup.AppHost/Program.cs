var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.eMeetup_ApiService>("apiservice");

builder.AddProject<Projects.eMeetup_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Build().Run();
