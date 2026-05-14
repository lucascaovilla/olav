// <copyright file="EfMigrationGenerator.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Generation;

using Olav.Infrastructure;

/// <summary>
/// Runs <c>dotnet ef migrations add</c> targeting the persistence project of an
/// installed EF Core infrastructure plugin (postgres or sqlserver).
/// </summary>
public class EfMigrationGenerator
{
    private static readonly HashSet<string> EfCorePlugins =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "postgres", "sqlserver" };

    private static readonly Dictionary<string, string> PersistenceSuffixes =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["postgres"] = "Persistence.Postgres",
            ["sqlserver"] = "Persistence.SqlServer",
        };

    private readonly string plugin;
    private readonly string migrationName;
    private readonly string root;

    /// <summary>
    /// Initializes a new instance of the <see cref="EfMigrationGenerator"/> class.
    /// </summary>
    /// <param name="plugin">Infrastructure plugin id (e.g. <c>postgres</c>).</param>
    /// <param name="migrationName">EF Core migration name (e.g. <c>InitialCreate</c>).</param>
    /// <param name="root">Repository root directory.</param>
    public EfMigrationGenerator(string plugin, string migrationName, string root)
    {
        this.plugin = plugin;
        this.migrationName = migrationName;
        this.root = root;
    }

    /// <summary>
    /// Validates prerequisites and runs <c>dotnet ef migrations add</c>.
    /// </summary>
    public void Generate()
    {
        if (!EfCorePlugins.Contains(this.plugin))
        {
            throw new InvalidOperationException(
                $"✗ Plugin '{this.plugin}' does not support EF Core migrations. " +
                $"Supported plugins: {string.Join(", ", EfCorePlugins)}.");
        }

        OlavConfig config = OlavConfig.Load(this.root);
        bool installed = config.Plugins?.Any(p => string.Equals(p.Id, this.plugin, StringComparison.OrdinalIgnoreCase)) == true;
        if (!installed)
        {
            throw new InvalidOperationException(
                $"✗ Plugin '{this.plugin}' is not installed. Run: olav add infrastructure {this.plugin}");
        }

        string ns = this.DiscoverNamespace();
        if (string.IsNullOrEmpty(ns))
        {
            throw new InvalidOperationException(
                "✗ Could not determine project namespace. Is this an Olav project?");
        }

        string suffix = PersistenceSuffixes[this.plugin];
        string pluginShortName = suffix.Split('.')[^1];

        string persistenceCsproj = Path.Combine(
            this.root,
            "src",
            $"{ns}.Infrastructure",
            "Persistence",
            pluginShortName,
            $"{ns}.Infrastructure.{suffix}.csproj");

        string apiCsproj = Path.Combine(this.root, "src", $"{ns}.Api", $"{ns}.Api.csproj");

        if (!File.Exists(persistenceCsproj))
        {
            throw new InvalidOperationException(
                $"✗ Persistence project not found: {persistenceCsproj}. " +
                $"Run 'olav add infrastructure {this.plugin}' first.");
        }

        if (!File.Exists(apiCsproj))
        {
            throw new InvalidOperationException(
                $"✗ API project not found: {apiCsproj}. Is this an Olav project?");
        }

        Console.WriteLine($"Running: dotnet ef migrations add {this.migrationName}");

        DotnetRunner.Run(
            $"ef migrations add {this.migrationName} " +
            $"--project \"{persistenceCsproj}\" " +
            $"--startup-project \"{apiCsproj}\" " +
            $"--output-dir Migrations",
            this.root);

        Console.WriteLine($"✓ Migration '{this.migrationName}' created.");
    }

    private string DiscoverNamespace()
    {
        string srcDir = Path.Combine(this.root, "src");
        if (!Directory.Exists(srcDir))
        {
            return string.Empty;
        }

        string[] csprojFiles = Directory.GetFiles(srcDir, "*.Infrastructure.csproj", SearchOption.AllDirectories)
            .Where(f => Path.GetFileNameWithoutExtension(f).EndsWith(".Infrastructure", StringComparison.Ordinal))
            .ToArray();

        if (csprojFiles.Length == 0)
        {
            return string.Empty;
        }

        string fileNameNoExt = Path.GetFileNameWithoutExtension(csprojFiles[0]);
        string[] parts = fileNameNoExt.Split('.');
        return parts.Length > 1 ? string.Join(".", parts, 0, parts.Length - 1) : fileNameNoExt;
    }
}
