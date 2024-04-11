using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MPM_Betting.Functions;

var host = new HostBuilder()
    .AddServiceDefaults()
    .ConfigureFunctionsWorkerDefaults((IFunctionsWorkerApplicationBuilder c) => {
        c.UseMiddleware<OpenTelemetryMiddleware>();
    })
    .ConfigureServices((context, services) => {
        services.AddSingleton(new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        });
    })
    .Build();

host.Run();
