using System;
using System.IO;
using Xunit;
using Olav.Generation;

namespace Olav.UnitTests.Generation;

public class DockerComposeInjectorTests : IDisposable
{
    private readonly string tempDir;

    public DockerComposeInjectorTests()
    {
        this.tempDir = Path.Combine(Path.GetTempPath(), "injector-tests-" + Guid.NewGuid());
        Directory.CreateDirectory(this.tempDir);
    }

    public void Dispose() => Directory.Delete(this.tempDir, true);

    private string WriteCompose(string content)
    {
        string path = Path.Combine(this.tempDir, "docker-compose.local.yml");
        File.WriteAllText(path, content);
        return path;
    }

    private static string MinimalLocalCompose() =>
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
        """;

    private static string PostgresServiceBlock() =>
        """
          postgres:
            image: postgres:16
            restart: always
            ports:
              - "5432:5432"
            healthcheck:
              test: ["CMD-SHELL", "pg_isready -U postgres"]
              interval: 5s
              timeout: 5s
              retries: 10
        """;

    private static string RedisServiceBlock() =>
        """
          redis:
            image: redis:7-alpine
            restart: always
            ports:
              - "6379:6379"
            healthcheck:
              test: ["CMD", "redis-cli", "ping"]
              interval: 5s
              timeout: 5s
              retries: 10
        """;

    [Fact]
    public void InjectService_AddsServiceToCompose_WhenServiceAbsent()
    {
        string path = WriteCompose(MinimalLocalCompose());

        DockerComposeInjector.InjectService(path, "postgres", PostgresServiceBlock(), null);

        string result = File.ReadAllText(path);
        Assert.Contains("postgres:", result);
        Assert.Contains("pg_isready", result);
    }

    [Fact]
    public void InjectService_IsIdempotent_WhenServiceAlreadyPresent()
    {
        string path = WriteCompose(MinimalLocalCompose());
        DockerComposeInjector.InjectService(path, "postgres", PostgresServiceBlock(), null);
        string afterFirst = File.ReadAllText(path);

        DockerComposeInjector.InjectService(path, "postgres", PostgresServiceBlock(), null);
        string afterSecond = File.ReadAllText(path);

        Assert.Equal(afterFirst, afterSecond);
    }

    [Fact]
    public void InjectService_InsertsBeforeVolumesSection()
    {
        string compose = MinimalLocalCompose() + "\nvolumes:\n  mydata:\n";
        string path = WriteCompose(compose);

        DockerComposeInjector.InjectService(path, "redis", RedisServiceBlock(), null);

        string result = File.ReadAllText(path);
        int redisIdx = result.IndexOf("redis:", StringComparison.Ordinal);
        int volumesIdx = result.IndexOf("\nvolumes:", StringComparison.Ordinal);
        Assert.True(redisIdx < volumesIdx, "redis service should appear before the volumes section");
    }

    [Fact]
    public void InjectService_AppendsTopLevelVolumes_WhenServiceBlockContainsVolumes()
    {
        string path = WriteCompose(MinimalLocalCompose());
        string blockWithVolume =
            """
              postgres:
                image: postgres:16
                volumes:
                  - postgres_data:/var/lib/postgresql/data

            volumes:
              postgres_data:
            """;

        DockerComposeInjector.InjectService(path, "postgres", blockWithVolume, null);

        string result = File.ReadAllText(path);
        Assert.Contains("postgres_data:", result);
        Assert.Contains("volumes:", result);
    }

    [Fact]
    public void InjectService_AddsDependsOnToWebService_WhenConditionProvided()
    {
        string path = WriteCompose(MinimalLocalCompose());

        DockerComposeInjector.InjectService(path, "postgres", PostgresServiceBlock(), "service_healthy");

        string result = File.ReadAllText(path);
        Assert.Contains("depends_on:", result);
        Assert.Contains("postgres:", result);
        Assert.Contains("condition: service_healthy", result);
    }

    [Fact]
    public void InjectService_DoesNotAddDependsOn_WhenConditionIsNull()
    {
        string path = WriteCompose(MinimalLocalCompose());

        DockerComposeInjector.InjectService(path, "redis", RedisServiceBlock(), null);

        string result = File.ReadAllText(path);
        Assert.DoesNotContain("depends_on:", result);
    }

    [Fact]
    public void InjectService_AppendsDependsOn_WhenWebAlreadyHasOne()
    {
        string compose =
            """
            services:
              web:
                build:
                  context: ..
                  dockerfile: docker/Dockerfile
                ports:
                  - "8080:8080"
                depends_on:
                  postgres:
                    condition: service_healthy

              postgres:
                image: postgres:16
                ports:
                  - "5432:5432"
            """;
        string path = WriteCompose(compose);

        DockerComposeInjector.InjectService(path, "redis", RedisServiceBlock(), "service_healthy");

        string result = File.ReadAllText(path);
        Assert.Contains("postgres:", result);
        Assert.Contains("redis:", result);
        int firstDependsOn = result.IndexOf("depends_on:", StringComparison.Ordinal);
        Assert.True(firstDependsOn >= 0);
        string afterDependsOn = result.Substring(firstDependsOn);
        Assert.Contains("redis:", afterDependsOn);
    }

    [Fact]
    public void InjectService_Throws_WhenFileDoesNotExist()
    {
        string nonExistentPath = Path.Combine(this.tempDir, "no-such-file.yml");

        Assert.Throws<InvalidOperationException>(() =>
            DockerComposeInjector.InjectService(nonExistentPath, "postgres", PostgresServiceBlock(), null));
    }
}
