// <copyright file="ProjectGenerator.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Generation;

using System.IO;
using Olav.Infrastructure;

/// <summary>
/// Generates Olav project.
/// </summary>
public class ProjectGenerator
{
    private readonly string name;
    private readonly string root;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectGenerator"/> class.
    /// </summary>
    /// <param name="name">Project's name.</param>
    /// <param name="root">Repository's root.</param>
    public ProjectGenerator(string name, string root)
    {
        this.name = name;
        this.root = root;
    }

    /// <summary>
    /// Centralized Generate method to create project's layers and files.
    /// </summary>
    public void Generate()
    {
        string src = Path.Combine(this.root, "src");

        DotnetRunner.Run($"new classlib -n {this.name}.Domain -f net10.0", src);
        DotnetRunner.Run($"new classlib -n {this.name}.Application -f net10.0", src);
        DotnetRunner.Run($"new classlib -n {this.name}.Infrastructure -f net10.0", src);
        DotnetRunner.Run($"new webapi -n {this.name}.Api -f net10.0 --no-https", src);

        this.AddPackages();
    }

    /// <summary>
    /// Adds references between generated layers.
    /// </summary>
    private void AddProjectReferences()
    {
        DotnetRunner.Run($"add src/{this.name}.Application/{this.name}.Application.csproj reference src/{this.name}.Domain/{this.name}.Domain.csproj", this.root);

        DotnetRunner.Run($"add src/{this.name}.Infrastructure/{this.name}.Infrastructure.csproj reference src/{this.name}.Application/{this.name}.Application.csproj", this.root);
        DotnetRunner.Run($"add src/{this.name}.Infrastructure/{this.name}.Infrastructure.csproj reference src/{this.name}.Domain/{this.name}.Domain.csproj", this.root);

        DotnetRunner.Run($"add src/{this.name}.Api/{this.name}.Api.csproj reference src/{this.name}.Application/{this.name}.Application.csproj", this.root);
        DotnetRunner.Run($"add src/{this.name}.Api/{this.name}.Api.csproj reference src/{this.name}.Infrastructure/{this.name}.Infrastructure.csproj", this.root);

        DotnetRunner.Run($"add tests/{this.name}.ArchitectureTests/{this.name}.ArchitectureTests.csproj reference src/{this.name}.Application/{this.name}.Application.csproj", this.root);
        DotnetRunner.Run($"add tests/{this.name}.ArchitectureTests/{this.name}.ArchitectureTests.csproj reference src/{this.name}.Domain/{this.name}.Domain.csproj", this.root);

        DotnetRunner.Run($"add tests/{this.name}.IntegrationTests/{this.name}.IntegrationTests.csproj reference src/{this.name}.Api/{this.name}.Api.csproj", this.root);
    }

    /// <summary>
    /// Adds required packages to generated layers.
    /// </summary>
    private void AddPackages()
    {
        DotnetRunner.Run($"add src/{this.name}.Api/{this.name}.Api.csproj package Serilog.AspNetCore", this.root);
        DotnetRunner.Run($"add src/{this.name}.Api/{this.name}.Api.csproj package Serilog.Sinks.Console", this.root);

        DotnetRunner.Run($"add src/{this.name}.Api/{this.name}.Api.csproj package OpenTelemetry.Extensions.Hosting", this.root);
        DotnetRunner.Run($"add src/{this.name}.Api/{this.name}.Api.csproj package OpenTelemetry.Instrumentation.AspNetCore", this.root);
        DotnetRunner.Run($"add src/{this.name}.Api/{this.name}.Api.csproj package OpenTelemetry.Exporter.Console", this.root);
    }
}
