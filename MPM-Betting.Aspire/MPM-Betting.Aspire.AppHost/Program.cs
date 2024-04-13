using MPM_Betting.Aspire.AppHost;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var azurite = builder.AddContainer("azurite", "mcr.microsoft.com/azure-storage/azurite")
    .WithEndpoint(containerPort: 10000, name: "blob", hostPort: 11000)
    .WithEndpoint(containerPort: 10001, name: "queue", hostPort: 11001)
    .WithEndpoint(containerPort: 10002, name: "table", hostPort: 11002);

var queueConnStrCallback = () =>  $"DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;QueueEndpoint={azurite.GetEndpoint("queue").Url.Replace("tcp", "http")}/devstoreaccount1;";

//    .WithReference(queue)
//    .WithEnvironment("QueueConnectionString", queueConnStrCallback)

var functions = builder.AddAzureFunction<MPM_Betting_Functions>("functions")
    .WithReference(azurite.GetEndpoint("queue"))
    .WithEnvironment("QueueConnectionString", queueConnStrCallback);

var grafana = builder.AddContainer("grafana", "grafana/grafana")
    .WithBindMount(GetFullPath("../grafana/config"), "/etc/grafana")
    .WithBindMount(GetFullPath("../grafana/dashboards"), "/var/lib/grafana/dashboards")
    .WithEndpoint(containerPort: 3000, hostPort: 3000, name: "grafana-http", scheme: "http");

var prometheus = builder.AddContainer("prometheus", "prom/prometheus")
    .WithBindMount(GetFullPath("../prometheus"), "/etc/prometheus")
    .WithEndpoint(containerPort: 9090, hostPort: 9090);

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

// BUG: Workaround for https://github.com/dotnet/aspire/issues/3323
static string GetFullPath(string relativePath) =>
    Path.GetFullPath(Path.Combine(MPM_Betting_Aspire_AppHost.ProjectPath, relativePath));
