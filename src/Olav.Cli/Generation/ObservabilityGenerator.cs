// <copyright file="ObservabilityGenerator.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Generation;

using System.IO;
using Olav.Infrastructure;
using Olav.Templates;

/// <summary>
/// Generates observability files for a Olav project.
/// </summary>
public class ObservabilityGenerator
{
    private readonly string name;
    private readonly string root;
    private readonly string owner;
    private readonly string license;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservabilityGenerator"/> class.
    /// Class initializer.
    /// </summary>
    /// <param name="name">Project's name.</param>
    /// <param name="root">Repository's root.</param>
    /// <param name="owner">Repository's owner.</param>
    /// <param name="license">Repository's license.</param>
    public ObservabilityGenerator(string name, string root, string owner, string license)
    {
        this.name = name;
        this.root = root;
        this.owner = owner;
        this.license = license;
    }

    /// <summary>
    /// Centralized Generate method to properly inject observability files.
    /// </summary>
    public void Generate()
    {
        string web = Path.Combine(this.root, "src", $"{this.name}.Api");
        string observability = Path.Combine(web, "Observability");

        FileSystem.CreateDirectory(observability);

        FileSystem.WriteFile(
            Path.Combine(observability, "CorrelationMiddleware.cs"),
            CorrelationMiddlewareTemplate.Generate(this.name, this.owner, this.license));

        FileSystem.WriteFile(
            Path.Combine(observability, "ObservabilityExtensions.cs"),
            ObservabilityExtensionsTemplate.Generate(this.name, this.owner, this.license));
    }
}
