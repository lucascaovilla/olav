// <copyright file="RepositoryGenerator.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Generation;

using System.Collections.Generic;
using System.IO;
using Olav.Infrastructure;
using Olav.Templates;

/// <summary>
/// Generates a Domain repository interface and its Infrastructure implementation.
/// When a DB plugin is installed the implementation is placed inside the plugin's
/// dedicated persistence classlib (<c>Infrastructure/Persistence/{Plugin}/</c>).
/// Without a plugin a stub is placed directly in <c>Infrastructure/</c>.
/// </summary>
public class RepositoryGenerator
{
    private static readonly Dictionary<string, string> PersistenceSuffixes =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["postgres"] = "Persistence.Postgres",
            ["sqlserver"] = "Persistence.SqlServer",
            ["redis"] = "Persistence.Redis",
        };

    private readonly string entityName;
    private readonly string projectName;
    private readonly string root;
    private readonly string? plugin;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryGenerator"/> class.
    /// </summary>
    /// <param name="entityName">Entity name the repository is for (e.g. <c>Order</c>).</param>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <param name="root">Repository root directory.</param>
    /// <param name="plugin">
    /// Installed plugin id that drives the implementation body (e.g. <c>postgres</c>),
    /// or <see langword="null"/> to emit a stub implementation in <c>Infrastructure/</c>.
    /// </param>
    public RepositoryGenerator(string entityName, string projectName, string root, string? plugin)
    {
        this.entityName = entityName;
        this.projectName = projectName;
        this.root = root;
        this.plugin = plugin;
    }

    /// <summary>
    /// Generates the domain interface and the implementation file, then registers the
    /// repository pair in the appropriate DI extension file.
    /// <list type="bullet">
    /// <item>Interface: <c>src/{projectName}.Domain/{entityName}/Repositories/I{entityName}Repository.cs</c></item>
    /// <item>Impl (no plugin): <c>src/{projectName}.Infrastructure/Repositories/{entityName}/{entityName}Repository.cs</c></item>
    /// <item>Impl (db plugin): <c>src/{projectName}.Infrastructure/Persistence/{Plugin}/Repositories/{entityName}/{entityName}Repository.cs</c></item>
    /// </list>
    /// </summary>
    public void Generate()
    {
        string interfacePath = Path.Combine(
            this.root,
            "src",
            $"{this.projectName}.Domain",
            this.entityName,
            "Repositories",
            $"I{this.entityName}Repository.cs");

        string? suffix = this.plugin != null && PersistenceSuffixes.TryGetValue(this.plugin, out string? s) ? s : null;
        string implProject = suffix != null
            ? $"{this.projectName}.Infrastructure.{suffix}"
            : $"{this.projectName}.Infrastructure";

        string implementationPath = suffix != null
            ? Path.Combine(
                this.root,
                "src",
                $"{this.projectName}.Infrastructure",
                "Persistence",
                suffix.Split('.')[^1],
                "Repositories",
                this.entityName,
                $"{this.entityName}Repository.cs")
            : Path.Combine(
                this.root,
                "src",
                $"{this.projectName}.Infrastructure",
                "Repositories",
                this.entityName,
                $"{this.entityName}Repository.cs");

        FileSystem.WriteFile(interfacePath, IRepositoryTemplate.Generate(this.projectName, this.entityName));
        FileSystem.WriteFile(implementationPath, RepositoryTemplate.Generate(this.projectName, this.entityName, this.plugin, implProject));

        this.RegisterInDi(suffix, implProject);
    }

    private void RegisterInDi(string? suffix, string implProject)
    {
        string diPath = suffix != null
            ? Path.Combine(
                this.root,
                "src",
                $"{this.projectName}.Infrastructure",
                "Persistence",
                suffix.Split('.')[^1],
                "DependencyInjection.cs")
            : Path.Combine(
                this.root,
                "src",
                $"{this.projectName}.Infrastructure",
                "DependencyInjection.cs");

        string registration =
            $"services.AddScoped<{this.projectName}.Domain.{this.entityName}.Repositories.I{this.entityName}Repository, {implProject}.Repositories.{this.entityName}.{this.entityName}Repository>();";

        if (!File.Exists(diPath))
        {
            string content = suffix != null
                ? PersistenceExtensionsTemplate.Generate(this.projectName, suffix, this.entityName)
                : InfrastructureExtensionsTemplate.Generate(this.projectName, this.entityName);

            FileSystem.WriteFile(diPath, content);
        }
        else
        {
            DiRegistrationInjector.Inject(diPath, registration);
        }
    }
}
