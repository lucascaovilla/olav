// <copyright file="FileTemplateGenerator.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Generation;

using System.IO;
using Olav.Infrastructure;
using Olav.Templates;

/// <summary>
/// Generates all template-based files for a Olav project.
/// </summary>
public class FileTemplateGenerator
{
    private readonly string name;
    private readonly string root;
    private readonly string owner;
    private readonly string license;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileTemplateGenerator"/> class.
    /// Class initializer.
    /// </summary>
    /// <param name="name">Project's name.</param>
    /// <param name="root">Repository's root.</param>
    /// <param name="owner">Repository's owner.</param>
    /// <param name="license">Repository's license.</param>
    public FileTemplateGenerator(string name, string root, string owner, string license)
    {
        this.name = name;
        this.root = root;
        this.owner = owner;
        this.license = license;
    }

    /// <summary>
    /// Centralized Generate method to properly inject configuration files.
    /// </summary>
    public void Generate()
    {
        this.GenerateRootFiles();
        this.GenerateGithub();
        this.GenerateGit();
        this.GenerateDocker();
        this.GenerateWebFiles();
        this.GenerateArchitectureTests();
        this.GenerateIntegrationTests();
        this.CleanupTemplates();
    }

    private void GenerateRootFiles()
    {
        FileSystem.WriteFile(
            Path.Combine(this.root, ".editorconfig"),
            EditorConfigTemplate.Generate());

        FileSystem.WriteFile(
            Path.Combine(this.root, "stylecop.json"),
            StyleCopJsonTemplate.Generate(this.owner, this.license));

        FileSystem.WriteFile(
            Path.Combine(this.root, "LICENSE"),
            LicenseTemplate.Generate(this.owner, this.license));

        FileSystem.WriteFile(
            Path.Combine(this.root, "Directory.Build.props"),
            DirectoryBuildPropsTemplate.Generate());

        FileSystem.WriteFile(
            Path.Combine(this.root, "Directory.Packages.props"),
            DirectoryPackagePropsTemplate.Generate());

        FileSystem.WriteFile(
            Path.Combine(this.root, "global.json"),
            GlobalJsonTemplate.Generate());
    }

    private void GenerateGithub()
    {
        FileSystem.WriteFile(
            Path.Combine(this.root, ".github/workflows/ci.yml"),
            CiYmlTemplate.Generate());
    }

    private void GenerateGit()
    {
        FileSystem.WriteFile(
            Path.Combine(this.root, ".gitignore"),
            GitignoreTemplate.Generate());

        FileSystem.WriteFile(
            Path.Combine(this.root, ".githooks/pre-commit"),
            PreCommitTemplate.Generate());

        FileSystem.WriteFile(
            Path.Combine(this.root, ".githooks/pre-push"),
            PrePushTemplate.Generate());
    }

    private void GenerateDocker()
    {
        FileSystem.WriteFile(
            Path.Combine(this.root, "docker/Dockerfile"),
            DockerfileTemplate.Generate(this.name));

        FileSystem.WriteFile(
            Path.Combine(this.root, "docker/.dockerignore"),
            DockerignoreTemplate.Generate());

        FileSystem.WriteFile(
            Path.Combine(this.root, "docker/docker-compose.local.yml"),
            DockerComposeTemplate.GenerateLocal(this.name));
    }

    private void GenerateWebFiles()
    {
        string webPath = Path.Combine(this.root, "src", $"{this.name}.Api");

        FileSystem.WriteFile(
            Path.Combine(webPath, "Program.cs"),
            ProgramFileTemplate.Generate(this.name, this.owner, this.license));
    }

    private void GenerateArchitectureTests()
    {
        string archTestsPath = Path.Combine(this.root, "tests", $"{this.name}.ArchitectureTests");

        FileSystem.WriteFile(
            Path.Combine(archTestsPath, "DependencyRulesTests.cs"),
            DependencyRulesTestsTemplate.Generate(this.name, this.owner, this.license));

        FileSystem.WriteFile(
            Path.Combine(archTestsPath, "ObservabilityRulesTests.cs"),
            ObservabilityRulesTestsTemplate.Generate(this.name, this.owner, this.license));
    }

    private void GenerateIntegrationTests()
    {
        string integrationTestsPath = Path.Combine(this.root, "tests", $"{this.name}.IntegrationTests");

        FileSystem.WriteFile(
            Path.Combine(integrationTestsPath, "InitialIntegrationTests.cs"),
            InitialIntegrationTestTemplate.Generate(this.name, this.owner, this.license));
    }

    private void CleanupTemplates()
    {
        string src = Path.Combine(this.root, "src");
        string tests = Path.Combine(this.root, "tests");

        FileSystem.DeleteIfExists(
            Path.Combine(src, $"{this.name}.Domain/Class1.cs"));

        FileSystem.DeleteIfExists(
            Path.Combine(src, $"{this.name}.Application/Class1.cs"));

        FileSystem.DeleteIfExists(
            Path.Combine(src, $"{this.name}.Infrastructure/Class1.cs"));

        FileSystem.DeleteIfExists(
            Path.Combine(src, $"{this.name}.Api/Controllers/WeatherForecastController.cs"));

        FileSystem.DeleteIfExists(
            Path.Combine(src, $"{this.name}.Api/WeatherForecast.cs"));

        FileSystem.DeleteIfExists(
            Path.Combine(tests, $"{this.name}.ArchitectureTests/UnitTest1.cs"));

        FileSystem.DeleteIfExists(
            Path.Combine(tests, $"{this.name}.IntegrationTests/UnitTest1.cs"));
    }
}