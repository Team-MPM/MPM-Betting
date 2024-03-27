using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis");

var sql = builder.AddSqlServer("sql", "1234!§$Sql")
    .WithVolumeMount("VolumeMount.sql.data", "/var/opt/mssql")
    .AddDatabase("MPM-Betting");

var api = builder.AddProject<MPM_Betting_Api>("api")
    .WithReference(sql);

var blazor = builder.AddProject<MPM_Betting_Blazor>("blazor")
    .WithReference(api)
    .WithReference(redis)
    .WithReference(sql);

builder.Build().Run();