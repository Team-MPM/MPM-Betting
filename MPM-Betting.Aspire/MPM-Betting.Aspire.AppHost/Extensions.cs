using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace MPM_Betting.Aspire.AppHost;

public static class Extensions
{
    public static IResourceBuilder<ExecutableResource> AddAzureFunction<TServiceMetadata>(
        this IDistributedApplicationBuilder builder,
        string name,
        int port = 7122,
        int debugPort = 7123)
        where TServiceMetadata : IProjectMetadata, new()
    {
        var serviceMetadata = new TServiceMetadata();
        var projectPath = serviceMetadata.ProjectPath;
        var projectDirectory = Path.GetDirectoryName(projectPath)!;

        var args = new[]
        {
            "host",
            "start",
            "--port",
            port.ToString(),
            "--nodeDebugPort",
            debugPort.ToString()
        };

        return builder.AddResource(new ExecutableResource(name, $"func host start --port {port.ToString()} --nodeDebugPort {debugPort.ToString()}", projectDirectory))
            .WithOtlpExporter();
    }
    
    public static  IResourceBuilder<ExecutableResource> AddProjectWithDotnetWatch<TServiceMetadata>(this IDistributedApplicationBuilder builder, string name) where TServiceMetadata : IProjectMetadata, new()
    {
        var serviceMetadata = new TServiceMetadata();
        var project = new ExecutableResource(name, "dotnet watch --non-interactive", Path.GetDirectoryName(serviceMetadata.ProjectPath)!);
        var executableBuilder = builder.AddResource(project);
        executableBuilder.WithEnvironment("OTEL_DOTNET_EXPERIMENTAL_OTLP_EMIT_EXCEPTION_LOG_ATTRIBUTES", "true");
        executableBuilder.WithEnvironment("OTEL_DOTNET_EXPERIMENTAL_OTLP_EMIT_EVENT_LOG_ATTRIBUTES", "true");
        executableBuilder.WithOtlpExporter();
        executableBuilder.WithEnvironment((context) =>
        {
            if (context.ExecutionContext.IsPublishMode)
            {
                return;
            }

            context.EnvironmentVariables["DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION"] = "true";
            context.EnvironmentVariables["LOGGING__CONSOLE__FORMATTERNAME"] = "simple";
            context.EnvironmentVariables["LOGGING__CONSOLE__FORMATTEROPTIONS__TIMESTAMPFORMAT"] = "HH:mm:ss";
        });
        executableBuilder.WithAnnotation(serviceMetadata);
        return executableBuilder;
    }
}
