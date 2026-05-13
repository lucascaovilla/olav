// <copyright file="DockerLocalComposeTests.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.IntegrationTests.Generation;

using System;
using System.IO;
using Olav.Infrastructure;
using Olav.Templates;
using Xunit;

/// <summary>
/// Integration tests that validate the content and Docker Compose syntax of
/// the generated <c>docker-compose.local.yml</c> file.
/// Docker-dependent tests call <see cref="ProcessRunner"/> directly, which throws
/// on non-zero exit — Docker must be installed; tests fail loudly if it is absent.
/// </summary>
public class DockerLocalComposeTests
{
    [Fact]
    public void GenerateLocal_DoesNotContainPostgres()
    {
        string content = DockerComposeTemplate.GenerateLocal("TestProject");
        Assert.DoesNotContain("postgres:", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GenerateLocal_ContainsWebService()
    {
        string content = DockerComposeTemplate.GenerateLocal("TestProject");
        Assert.Contains("web:", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GenerateLocal_ContainsEnvFile()
    {
        string content = DockerComposeTemplate.GenerateLocal("TestProject");
        Assert.Contains("env_file: ../.env", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GenerateLocal_ContainsAspNetCoreUrls()
    {
        string content = DockerComposeTemplate.GenerateLocal("TestProject");
        Assert.Contains("ASPNETCORE_URLS=http://+:8080", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GenerateLocal_ContainsPortMapping_8080()
    {
        string content = DockerComposeTemplate.GenerateLocal("TestProject");
        Assert.Contains("8080:8080", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GenerateLocal_DoesNotContainConnectionStrings()
    {
        string content = DockerComposeTemplate.GenerateLocal("TestProject");
        Assert.DoesNotContain("ConnectionStrings", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GenerateLocal_IsDockerComposeValid()
    {
        string content = DockerComposeTemplate.GenerateLocal("TestProject");
        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        string filePath = Path.Combine(tempDir, "docker-compose.local.yml");

        try
        {
            File.WriteAllText(filePath, content);
            ProcessRunner.Run("docker", "compose -f " + filePath + " config", tempDir);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
