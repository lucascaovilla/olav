using System;
using System.IO;
using Xunit;
using Olav.IntegrationTests.Generation.Fixtures;
using Olav.Infrastructure;

namespace Olav.IntegrationTests.Cli;

[Collection("GeneratedProject")]
public class DockerBuild_EndToEndTests(GeneratedProjectFixture fixture)
{
    private readonly GeneratedProjectFixture _fixture = fixture;

    [Fact]
    public void Dockerfile_Should_Build_Successfully()
    {
        string dockerPath = Path.Combine(this._fixture.ProjectPath, "docker");

        Assert.True(File.Exists(Path.Combine(dockerPath, "Dockerfile")));

        Console.WriteLine("[Docker] Starting image build...");
        ProcessRunner.Run("docker", "compose -f docker-compose.local.yml build --no-cache --progress=plain", dockerPath);
        Console.WriteLine("[Docker] Image build completed.");
    }

    [Fact]
    public void Docker_Compose_Should_Validate_Successfully()
    {
        string dockerPath = Path.Combine(this._fixture.ProjectPath, "docker");

        string[] composeFiles = ["docker-compose.local.yml"];
        foreach (string f in composeFiles)
        {
            Console.WriteLine($"[Docker] Validating {f}...");
            ProcessRunner.Run("docker", $"compose -f {f} config", dockerPath);
            Console.WriteLine($"[Docker] {f} is valid.");
        }
    }
}
