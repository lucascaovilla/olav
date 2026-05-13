using System;
using Xunit;
using Olav.UnitTests.Templates.Helpers;
using Olav.Templates;

namespace Olav.UnitTests.Templates;

public class DockerComposeTemplateTests
{
    [Fact]
    public void Should_Be_Valid_Compose()
    {
        TemplateValidationHelper.ValidateDockerCompose(DockerComposeTemplate.GeneratePrd("Test"));
        TemplateValidationHelper.ValidateDockerCompose(DockerComposeTemplate.GenerateStaging("Test"));
        TemplateValidationHelper.ValidateDockerCompose(DockerComposeTemplate.GenerateLocal("Test"));
    }

    [Fact]
    public void GeneratePrd_ContainsWebService()
    {
        string content = DockerComposeTemplate.GeneratePrd("Test");
        Assert.Contains("web:", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GeneratePrd_UsesImageTag()
    {
        string content = DockerComposeTemplate.GeneratePrd("Test");
        Assert.Contains("${IMAGE_TAG}", content, StringComparison.Ordinal);
    }

    [Fact]
    public void GeneratePrd_HasEnvFile()
    {
        string content = DockerComposeTemplate.GeneratePrd("Test");
        Assert.Contains("env_file", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GeneratePrd_DoesNotContainBuildContext()
    {
        string content = DockerComposeTemplate.GeneratePrd("Test");
        Assert.DoesNotContain("build:", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GenerateLocal_ContainsWebService()
    {
        string content = DockerComposeTemplate.GenerateLocal("Test");
        Assert.Contains("web:", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GenerateLocal_ContainsBuildContext()
    {
        string content = DockerComposeTemplate.GenerateLocal("Test");
        Assert.Contains("build:", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GenerateLocal_ContainsEnvFile()
    {
        string content = DockerComposeTemplate.GenerateLocal("Test");
        Assert.Contains("env_file", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GenerateLocal_ContainsAspNetCoreUrls()
    {
        string content = DockerComposeTemplate.GenerateLocal("Test");
        Assert.Contains("ASPNETCORE_URLS=http://+:8080", content, StringComparison.Ordinal);
    }

    [Fact]
    public void GenerateStaging_UsesImageTag()
    {
        string content = DockerComposeTemplate.GenerateStaging("Test");
        Assert.Contains("${IMAGE_TAG}", content, StringComparison.Ordinal);
    }

    [Fact]
    public void GenerateStaging_DoesNotContainBuildContext()
    {
        string content = DockerComposeTemplate.GenerateStaging("Test");
        Assert.DoesNotContain("build:", content, StringComparison.OrdinalIgnoreCase);
    }
}
