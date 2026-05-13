// <copyright file="TestGenerator.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Generation;

using System.IO;
using System.Xml.Linq;
using Olav.Infrastructure;

/// <summary>
/// Generates tests files for a Olav project.
/// </summary>
public class TestGenerator
{
    private readonly string name;
    private readonly string root;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestGenerator"/> class.
    /// Class initializer.
    /// </summary>
    /// <param name="name">Project's name.</param>
    /// <param name="root">Repository's root.</param>
    public TestGenerator(string name, string root)
    {
        this.name = name;
        this.root = root;
    }

    /// <summary>
    /// Centralized Generate method to create dotnet tests layers.
    /// </summary>
    public void Generate()
    {
        string tests = Path.Combine(this.root, "tests");

        DotnetRunner.Run($"new xunit -n {this.name}.ArchitectureTests -f net10.0", tests);
        DotnetRunner.Run($"new xunit -n {this.name}.IntegrationTests -f net10.0", tests);

        this.AddProjectReferences();
        this.AddPackages();

        this.AddCoverageEnforcement(Path.Combine(
            this.root,
            $"tests/{this.name}.ArchitectureTests/{this.name}.ArchitectureTests.csproj"));

        this.AddCoverageEnforcement(Path.Combine(
            this.root,
            $"tests/{this.name}.IntegrationTests/{this.name}.IntegrationTests.csproj"));
    }

    /// <summary>
    /// Adds references between generated layers.
    /// </summary>
    private void AddProjectReferences()
    {
        DotnetRunner.Run(
            $"add tests/{this.name}.ArchitectureTests/{this.name}.ArchitectureTests.csproj reference src/{this.name}.Domain/{this.name}.Domain.csproj",
            this.root);
        DotnetRunner.Run(
            $"add tests/{this.name}.ArchitectureTests/{this.name}.ArchitectureTests.csproj reference src/{this.name}.Application/{this.name}.Application.csproj",
            this.root);
        DotnetRunner.Run(
            $"add tests/{this.name}.ArchitectureTests/{this.name}.ArchitectureTests.csproj reference src/{this.name}.Infrastructure/{this.name}.Infrastructure.csproj",
            this.root);
        DotnetRunner.Run(
            $"add tests/{this.name}.ArchitectureTests/{this.name}.ArchitectureTests.csproj reference src/{this.name}.Api/{this.name}.Api.csproj",
            this.root);

        DotnetRunner.Run(
            $"add tests/{this.name}.IntegrationTests/{this.name}.IntegrationTests.csproj reference src/{this.name}.Api/{this.name}.Api.csproj",
            this.root);
    }

    /// <summary>
    /// Adds required packages to generated layers.
    /// </summary>
    private void AddPackages()
    {
        DotnetRunner.Run($"add tests/{this.name}.ArchitectureTests/{this.name}.ArchitectureTests.csproj package coverlet.collector", this.root);
        DotnetRunner.Run($"add tests/{this.name}.IntegrationTests/{this.name}.IntegrationTests.csproj package coverlet.collector", this.root);

        DotnetRunner.Run($"add tests/{this.name}.ArchitectureTests/{this.name}.ArchitectureTests.csproj package NetArchTest.Rules", this.root);
        DotnetRunner.Run($"add tests/{this.name}.ArchitectureTests/{this.name}.ArchitectureTests.csproj package FluentAssertions", this.root);
    }

    /// <summary>
    /// Adds coverage enforcement to .csproj files.
    /// </summary>
    /// <param name="csprojPath">Csproj file path.</param>
    private void AddCoverageEnforcement(string csprojPath)
    {
        XDocument doc = XDocument.Load(csprojPath);

        XElement? project = doc.Element("Project");
        if (project == null)
        {
            throw new Exception("Invalid csproj format.");
        }

        XElement propertyGroup = new XElement(
            "PropertyGroup",
            new XElement("CollectCoverage", "true"),
            new XElement("CoverletOutputFormat", "lcov"),
            new XElement("Threshold", "100"),
            new XElement("ThresholdType", "line"),
            new XElement("ThresholdStat", "Total"));

        project.Add(propertyGroup);

        doc.Save(csprojPath);
    }
}
