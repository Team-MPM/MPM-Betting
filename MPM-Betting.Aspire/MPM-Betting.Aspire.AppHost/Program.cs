using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<MPM_Betting_Api>("api");

var blazor = builder.AddProject<MPM_Betting_Blazor>("blazor")
    .WithReference(api);

builder.Build().Run();