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
    private readonly string projectName;
    private readonly string root;
    private readonly string? entityName;

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainEnumGenerator"/> class.
    /// </summary>
    /// <param name="enumName">Enum type name (e.g. <c>OrderStatus</c>).</param>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <param name="root">Repository root directory.</param>
    /// <param name="entityName">
    /// Aggregate root this enum belongs to (e.g. <c>Order</c>).
    /// When <see langword="null"/> the enum is placed under <c>Shared/Enums/</c>.
    /// </param>
    public DomainEnumGenerator(string enumName, string projectName, string root, string? entityName = null)
    {
        this.enumName = enumName;
        this.projectName = projectName;
        this.root = root;
        this.entityName = entityName;
    }

    /// <summary>
    /// Generates the enum file.
    /// With entity: <c>src/{projectName}.Domain/{entityName}/Enums/{enumName}.cs</c>.
    /// Without entity: <c>src/{projectName}.Domain/Shared/Enums/{enumName}.cs</c>.
    /// </summary>
    public void Generate()
    {
        string outputPath = this.entityName != null
            ? Path.Combine(
                this.root,
                "src",
                $"{this.projectName}.Domain",
                this.entityName,
                "Enums",
                $"{this.enumName}.cs")
            : Path.Combine(
                this.root,
                "src",
                $"{this.projectName}.Domain",
                "Shared",
                "Enums",
                $"{this.enumName}.cs");

        FileSystem.WriteFile(outputPath, DomainEnumTemplate.Generate(this.projectName, this.enumName, this.entityName));
    }
}
