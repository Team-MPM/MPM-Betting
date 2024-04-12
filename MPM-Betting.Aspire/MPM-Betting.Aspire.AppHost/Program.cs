using MPM_Betting.Aspire.AppHost;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var azurite = builder.AddContainer("azurite", "mcr.microsoft.com/azure-storage/azurite")
    .WithEndpoint(containerPort: 10000, name: "blob", hostPort: 11000)
    .WithEndpoint(containerPort: 10001, name: "queue", hostPort: 11001)
    .WithEndpoint(containerPort: 10001, name: "table", hostPort: 11002);

var queue = azurite.GetEndpoint("queue");
var queueConnStrCallback = () =>  $"DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;QueueEndpoint={queue.Value.Replace("tcp", "http")}/devstoreaccount1;";

var functions = builder.AddAzureFunction<MPM_Betting_Functions>("functions")
    .WithReference(queue)
    .WithEnvironment("QueueConnectionString", queueConnStrCallback);

var redis = builder.AddRedis("redis");

var sql = builder.AddSqlServer("sql", "1234!§$Sql")
    .WithVolumeMount("VolumeMount.sql.data", "/var/opt/mssql")
    .AddDatabase("MPM-Betting");

var api = builder.AddProject<MPM_Betting_Api>("api")
    .WithReference(sql)
    .WithReference(redis)
    .WithReference(queue)
    .WithEnvironment("QueueConnectionString", queueConnStrCallback);

var blazor = builder.AddProjectWithDotnetWatch<MPM_Betting_Blazor>("blazor")
    .WithHttpsEndpoint(5023)
    .WithReference(api)
    .WithReference(redis)
    .WithReference(sql);

builder.Build().Run();