using MPM_Betting.Aspire.AppHost;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

if (!builder.ExecutionContext.IsPublishMode)
{
    var azurite = builder.AddContainer("azurite", "mcr.microsoft.com/azure-storage/azurite")
        .WithEndpoint(port: 10000, name: "blob", targetPort: 11000)
        .WithEndpoint(port: 10001, name: "queue", targetPort: 11001)
        .WithEndpoint(port: 10002, name: "table", targetPort: 11002);

    var queueConnStrCallback = () =>  $"DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;QueueEndpoint={azurite.GetEndpoint("queue").Url.Replace("tcp", "http")}/devstoreaccount1;";

    //    .WithReference(queue)
    //    .WithEnvironment("QueueConnectionString", queueConnStrCallback)

    var functions = builder.AddAzureFunction<MPM_Betting_Functions>("functions")
        .WithReference(azurite.GetEndpoint("queue"))
        .WithEnvironment("QueueConnectionString", queueConnStrCallback);
}


var grafana = builder.AddContainer("grafana", "grafana/grafana")
    .WithBindMount(GetFullPath("../grafana/config"), "/etc/grafana", isReadOnly: false)
    .WithBindMount(GetFullPath("../grafana/dashboards"), "/var/lib/grafana/dashboards", isReadOnly: false)
    .WithHttpEndpoint(port: 3000, targetPort: 3000);

var prometheus = builder.AddContainer("prometheus", "prom/prometheus")
    .WithBindMount(GetFullPath("../prometheus"), "/etc/prometheus")
    .WithHttpEndpoint(port: 9090, targetPort: 9090);

var redis = builder.AddRedis("redis")
    .WithPersistence()
    .WithDataVolume();

var sql = builder.AddSqlServer("sql", password: builder.CreateStablePassword("MPM-Betting-Password"))
    .WithDataVolume()
    .PublishAsAzureSqlDatabase()
    .AddDatabase("MPM-Betting");

if (builder.ExecutionContext.IsPublishMode || Environment.GetEnvironmentVariable("CI") == "true")
{
    var api = builder.AddProject<MPM_Betting_Api>("api")
        .WithExternalHttpEndpoints()
        .WithReference(sql)
        .WithReference(redis);

    var blazor = builder.AddProject<MPM_Betting_Blazor>("blazor")
        .WithExternalHttpEndpoints()
        .WithReference(api)
        .WithReference(redis)
        .WithReference(sql);
}
else
{
    var mailDev = builder.AddMailDev("maildev", 9324, 9325);
    
    var api = builder.AddProjectWithDotnetWatch<MPM_Betting_Api>("api")
        .WithHttpEndpoint(targetPort: 5241, port: 5241, isProxied: false)
        .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
        .WithEnvironment("DOTNET_ENVIRONMENT", "Development")
        .WithReference(sql)
        .WithReference(redis)
        .WithReference(mailDev);
    
    var blazor = builder.AddProjectWithDotnetWatch<MPM_Betting_Blazor>("blazor")
        .WithHttpEndpoint(targetPort: 5023, port: 5023, isProxied: false)
        .WithEnvironment("services__api__http__0", "http://localhost:5241")
        .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
        .WithEnvironment("DOTNET_ENVIRONMENT", "Development")
        .WithReference(redis)
        .WithReference(sql)
        .WithReference(mailDev);
}

var dbManager = builder.AddProject<MPM_Betting_DbManager>("dbmanager")
    .WithReference(sql);


builder.Build().Run();

// BUG: Workaround for https://github.com/dotnet/aspire/issues/3323
static string GetFullPath(string relativePath) =>
    Path.GetFullPath(Path.Combine(MPM_Betting_Aspire_AppHost.ProjectPath, relativePath));
