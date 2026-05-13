using System;
using System.IO;

namespace Olav.IntegrationTests.Plugins;

/// <summary>
/// Creates an isolated project structure for plugin integration tests
/// without running olav new (avoids CWD-manipulation race conditions).
/// Each test class gets its own instance via IClassFixture.
/// </summary>
public class PluginTestFixture : IDisposable
{
    public string ProjectPath { get; }
    public string ProjectName { get; } = "PluginTestApp";

    public PluginTestFixture()
    {
        ProjectPath = Path.Combine(Path.GetTempPath(), "olav-plugin-test-" + Guid.NewGuid().ToString());

        string infraDir = Path.Combine(ProjectPath, "src", ProjectName + ".Infrastructure");
        Directory.CreateDirectory(infraDir);
        Directory.CreateDirectory(Path.Combine(ProjectPath, ".github", "workflows"));
        Directory.CreateDirectory(Path.Combine(ProjectPath, "docker"));

        File.WriteAllText(
            Path.Combine(infraDir, ProjectName + ".Infrastructure.csproj"),
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
              <ItemGroup>
                <PackageReference Include="SomePackage" Version="1.0.0" />
              </ItemGroup>
            </Project>
            """);

        File.WriteAllText(
            Path.Combine(ProjectPath, "olav.json"),
            """{ "toolVersion": "0.1.0" }""");

        File.WriteAllText(
            Path.Combine(ProjectPath, "docker", "docker-compose.local.yml"),
            """
            services:
              web:
                build:
                  context: ..
                  dockerfile: docker/Dockerfile
                ports:
                  - "8080:8080"
                env_file: ../.env
                environment:
                  - ASPNETCORE_ENVIRONMENT=Development
                  - ASPNETCORE_URLS=http://+:8080
            """);
    }

    public void Dispose()
    {
        if (Directory.Exists(ProjectPath))
        {
            Directory.Delete(ProjectPath, true);
        }
    }
}
