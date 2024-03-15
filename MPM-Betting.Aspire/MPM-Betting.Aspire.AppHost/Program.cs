using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<MPM_Betting_Api>("api");

builder.Build().Run();