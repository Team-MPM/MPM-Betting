using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// var azurite = builder.AddContainer("azurite", "mcr.microsoft.com/azure-storage/azurite")
//     .WithEndpoint(containerPort: 10000, name: "blob", hostPort: 11000)
//     .WithEndpoint(containerPort: 10001, name: "queue", hostPort: 11001)
//     .WithEndpoint(containerPort: 10001, name: "table", hostPort: 11002);

//var queue = azurite.GetEndpoint("queue");
//var queueConnStrCallback = () =>  $"DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;QueueEndpoint={queue.Url.Replace("tcp", "http")}/devstoreaccount1;";

//    .WithReference(queue)
//    .WithEnvironment("QueueConnectionString", queueConnStrCallback)

// var functions = builder.AddAzureFunction<Projects.MPM_Betting_Functions>("functions");

var redis = builder.AddRedis("redis");

var sql = builder.AddSqlServer("sql")
    .WithDataVolume()
    .AddDatabase("MPM-Betting");

var api = builder.AddProject<MPM_Betting_Api>("api")
    .WithReference(sql)
    .WithReference(redis);

var blazor = builder.AddProject<MPM_Betting_Blazor>("blazor")
    .WithReference(api)
    .WithReference(redis)
    .WithReference(sql);

builder.Build().Run();