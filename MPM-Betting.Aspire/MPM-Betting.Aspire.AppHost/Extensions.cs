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

        return builder.AddResource(new ExecutableResource(name, "func", projectDirectory, args))
            .WithOtlpExporter();
    }
}
