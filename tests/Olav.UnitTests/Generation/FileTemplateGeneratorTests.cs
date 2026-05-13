using System;
using System.IO;
using Xunit;
using Olav.Generation;

namespace Olav.UnitTests.Generation;

public class FileTemplateGeneratorTests
{
    [Fact]
    public void Generate_Should_Create_All_Expected_Files()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        FileTemplateGenerator generator = new(
            name: "TestApp",
            root: root,
            owner: "TestOwner",
            license: "MIT");

        generator.Generate();

        AssertFileExists(root, ".editorconfig");
        AssertFileExists(root, "stylecop.json");
        AssertFileExists(root, "LICENSE");
        AssertFileExists(root, "Directory.Build.props");
        AssertFileExists(root, "Directory.Packages.props");
        AssertFileExists(root, "global.json");

        AssertFileExists(root, ".gitignore");
        AssertFileExists(root, ".githooks/pre-commit");
        AssertFileExists(root, ".githooks/pre-push");

        AssertFileExists(root, ".github/workflows/ci.yml");

        AssertFileExists(root, "docker/Dockerfile");
        AssertFileExists(root, "docker/docker-compose.local.yml");

        AssertFileExists(root, "src/TestApp.Api/Program.cs");

        AssertFileNotEmpty(root, "global.json");
        AssertFileNotEmpty(root, ".github/workflows/ci.yml");
        AssertFileNotEmpty(root, "Directory.Build.props");
        AssertFileNotEmpty(root, "docker/docker-compose.local.yml");
    }

    private static void AssertFileExists(string root, string relativePath)
    {
        string path = Path.Combine(root, relativePath);
        Assert.True(File.Exists(path), $"Expected file not found: {relativePath}");
    }

    private static void AssertFileNotEmpty(string root, string relativePath)
    {
        string content = File.ReadAllText(Path.Combine(root, relativePath));
        Assert.False(string.IsNullOrWhiteSpace(content), $"File is empty: {relativePath}");
    }
}