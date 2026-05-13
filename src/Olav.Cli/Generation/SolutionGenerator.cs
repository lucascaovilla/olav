// <copyright file="SolutionGenerator.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Generation;

using System.IO;
using Olav.Infrastructure;

/// <summary>
/// Generates Olav project solution.
/// </summary>
public class SolutionGenerator
{
    private readonly string name;
    private readonly string root;

    /// <summary>
    /// Initializes a new instance of the <see cref="SolutionGenerator"/> class.
    /// </summary>
    /// <param name="name">Project's name.</param>
    /// <param name="root">Repository's root.</param>
    public SolutionGenerator(string name, string root)
    {
        this.name = name;
        this.root = root;
    }

    /// <summary>
    /// Centralized Generate method to create base directories.
    /// </summary>
    public void Generate()
    {
        FileSystem.CreateDirectory(this.root);
        FileSystem.CreateDirectory(Path.Combine(this.root, "src"));
        FileSystem.CreateDirectory(Path.Combine(this.root, "tests"));
        FileSystem.CreateDirectory(Path.Combine(this.root, "docker"));

        DotnetRunner.Run($"new sln -n {this.name}", this.root);

        this.AddProjectsToSolution();
    }

    /// <summary>
    /// Adds project's layers to project's solution file.
    /// </summary>
    private void AddProjectsToSolution()
    {
        DotnetRunner.Run($"sln add src/{this.name}.Domain/{this.name}.Domain.csproj", this.root);
        DotnetRunner.Run($"sln add src/{this.name}.Application/{this.name}.Application.csproj", this.root);
        DotnetRunner.Run($"sln add src/{this.name}.Infrastructure/{this.name}.Infrastructure.csproj", this.root);
        DotnetRunner.Run($"sln add src/{this.name}.Api/{this.name}.Api.csproj", this.root);

        DotnetRunner.Run($"sln add tests/{this.name}.ArchitectureTests/{this.name}.ArchitectureTests.csproj", this.root);
        DotnetRunner.Run($"sln add tests/{this.name}.IntegrationTests/{this.name}.IntegrationTests.csproj", this.root);
    }
}
