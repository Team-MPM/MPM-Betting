﻿using System.Diagnostics;
using System.Reflection;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Publishing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.Hosting;

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

        return builder.AddExecutable(name, "func", projectDirectory, args)
            .WithHttpEndpoint(7122, 7122, isProxied: false)
            .WithOtlpExporter();
    }
    
    public static  IResourceBuilder<ExecutableResource> AddProjectWithDotnetWatch<TServiceMetadata>(this IDistributedApplicationBuilder builder, string name) where TServiceMetadata : IProjectMetadata, new()
    {
        var serviceMetadata = new TServiceMetadata();
        var executableBuilder = builder.AddExecutable(name, "dotnet", Path.GetDirectoryName(serviceMetadata.ProjectPath)!, ["watch", "--non-interactive"]);
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
    
    /// <summary>
    /// Creates a password parameter that in the development environment is generated once and stored in the user secrets store.
    /// </summary>
    /// <remarks>
    /// The password is only stable when in the development environment and the application is running. In all other cases, the password is generated each time.
    /// </remarks>
    public static IResourceBuilder<ParameterResource> CreateStablePassword(this IDistributedApplicationBuilder builder, string name,
        bool lower = true, bool upper = true, bool numeric = true, bool special = true,
        int minLower = 0, int minUpper = 0, int minNumeric = 0, int minSpecial = 0)
    {
        ParameterDefault generatedPassword = new GenerateParameterDefault
        {
            MinLength = 22, // enough to give 128 bits of entropy when using the default 67 possible characters. See remarks in PasswordGenerator.Generate
            Lower = lower,
            Upper = upper,
            Numeric = numeric,
            Special = special,
            MinLower = minLower,
            MinUpper = minUpper,
            MinNumeric = minNumeric,
            MinSpecial = minSpecial
        };

        if (builder.Environment.IsDevelopment() && builder.ExecutionContext.IsRunMode)
        {
            // In development mode, generate a new password each time the application starts
            generatedPassword = new UserSecretsParameterDefault(builder.Environment.ApplicationName, name, generatedPassword);
        }

        var parameterResource = new ParameterResource(name, parameterDefault => GetParameterValue(builder.Configuration, name, parameterDefault), true)
        {
            Default = generatedPassword
        };

        return ResourceBuilder.Create(parameterResource, builder);
    }

    private static string GetParameterValue(IConfiguration configuration, string name, ParameterDefault? parameterDefault)
    {
        var configurationKey = $"Parameters:{name}";
        return configuration[configurationKey]
            ?? parameterDefault?.GetDefaultValue()
            ?? throw new DistributedApplicationException($"Parameter resource could not be used because configuration key '{configurationKey}' is missing and the Parameter has no default value.");
    }

    private class UserSecretsParameterDefault(string applicationName, string parameterName, ParameterDefault parameterDefault) : ParameterDefault
    {
        public override string GetDefaultValue()
        {
            var value = parameterDefault.GetDefaultValue();
            var configurationKey = $"Parameters:{parameterName}";
            TrySetUserSecret(applicationName, configurationKey, value);
            return value;
        }

        public override void WriteToManifest(ManifestPublishingContext context) => parameterDefault.WriteToManifest(context);

        private static bool TrySetUserSecret(string applicationName, string name, string value)
        {
            if (string.IsNullOrEmpty(applicationName)) 
                return false;
            
            var appAssembly = Assembly.Load(new AssemblyName(applicationName));
            if (appAssembly?.GetCustomAttribute<UserSecretsIdAttribute>()?.UserSecretsId is not { } userSecretsId) 
                return false;
            
            // Save the value to the secret store
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                new List<string>(["user-secrets", "set", name, value, "--id", userSecretsId]).ForEach(startInfo.ArgumentList.Add);
                var setUserSecrets = Process.Start(startInfo);
                setUserSecrets?.WaitForExit(TimeSpan.FromSeconds(10));
                return setUserSecrets?.ExitCode == 0;
            }
            catch (Exception)
            {
                // ignored
            }

            return false;
        }
    }

    private static class ResourceBuilder
    {
        public static IResourceBuilder<T> Create<T>(T resource, IDistributedApplicationBuilder distributedApplicationBuilder) where T : IResource
        {
            return new ResourceBuilder<T>(resource, distributedApplicationBuilder);
        }
    }

    private class ResourceBuilder<T>(T resource, IDistributedApplicationBuilder distributedApplicationBuilder) : IResourceBuilder<T> where T : IResource
    {
        public IDistributedApplicationBuilder ApplicationBuilder { get; } = distributedApplicationBuilder;

        public T Resource { get; } = resource;

        public IResourceBuilder<T> WithAnnotation<TAnnotation>(TAnnotation annotation, ResourceAnnotationMutationBehavior behavior = ResourceAnnotationMutationBehavior.Append) where TAnnotation : IResourceAnnotation
        {
            throw new NotImplementedException();
        }
    }
}
