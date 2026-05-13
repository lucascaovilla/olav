// <copyright file="DomainEnumGenerator.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Generation;

using System.IO;
using Olav.Infrastructure;
using Olav.Templates;

/// <summary>
/// Generates a Domain enum.
/// </summary>
public class DomainEnumGenerator
{
    private readonly string enumName;
    private readonly string entityName;
    private readonly string projectName;
    private readonly string root;

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainEnumGenerator"/> class.
    /// </summary>
    /// <param name="entityName">Entity folder name (e.g. <c>Order</c>).</param>
    /// <param name="enumName">Enum type name (e.g. <c>OrderStatus</c>).</param>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <param name="root">Repository root directory.</param>
    public DomainEnumGenerator(string entityName, string enumName, string projectName, string root)
    {
        this.entityName = entityName;
        this.enumName = enumName;
        this.projectName = projectName;
        this.root = root;
    }

    /// <summary>
    /// Generates the enum file at <c>src/{projectName}.Domain/{entityName}/Enums/{enumName}.cs</c>.
    /// </summary>
    public void Generate()
    {
        string outputPath = Path.Combine(
            this.root,
            "src",
            $"{this.projectName}.Domain",
            this.entityName,
            "Enums",
            $"{this.enumName}.cs");

        FileSystem.WriteFile(outputPath, DomainEnumTemplate.Generate(this.projectName, this.entityName, this.enumName));
    }
}
