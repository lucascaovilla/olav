// <copyright file="EntityGenerator.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Generation;

using System.IO;
using Olav.Infrastructure;
using Olav.Templates;

/// <summary>
/// Generates a Domain entity class.
/// </summary>
public class EntityGenerator
{
    private readonly string entityName;
    private readonly string projectName;
    private readonly string root;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityGenerator"/> class.
    /// </summary>
    /// <param name="entityName">Entity class name (e.g. <c>Order</c>).</param>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <param name="root">Repository root directory.</param>
    public EntityGenerator(string entityName, string projectName, string root)
    {
        this.entityName = entityName;
        this.projectName = projectName;
        this.root = root;
    }

    /// <summary>
    /// Generates the entity file at <c>src/{projectName}.Domain/Entities/{entityName}.cs</c>.
    /// </summary>
    public void Generate()
    {
        string outputPath = Path.Combine(
            this.root,
            "src",
            $"{this.projectName}.Domain",
            "Entities",
            $"{this.entityName}.cs");

        FileSystem.WriteFile(outputPath, EntityTemplate.Generate(this.projectName, this.entityName));
    }
}
