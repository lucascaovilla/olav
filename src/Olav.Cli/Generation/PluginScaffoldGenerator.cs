// <copyright file="PluginScaffoldGenerator.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Generation;

using System.IO;
using System.Text;
using Olav.Infrastructure;

/// <summary>
/// Scaffolds a new plugin authoring directory with an <c>olav.plugin.json</c>
/// manifest and a starter template file.
/// </summary>
public class PluginScaffoldGenerator
{
    private readonly string pluginId;
    private readonly string rootDir;
    private readonly string category;
    private readonly string delivery;
    private readonly string author;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginScaffoldGenerator"/> class.
    /// </summary>
    /// <param name="pluginId">The plugin identifier (lowercase, hyphen-separated).</param>
    /// <param name="rootDir">The parent directory where the plugin folder is created.</param>
    /// <param name="category">Plugin category: infrastructure or deployment.</param>
    /// <param name="delivery">Delivery model: package or generation.</param>
    /// <param name="author">Author handle written to the manifest.</param>
    public PluginScaffoldGenerator(
        string pluginId,
        string rootDir,
        string category,
        string delivery,
        string author)
    {
        this.pluginId = pluginId;
        this.rootDir = rootDir;
        this.category = category;
        this.delivery = delivery;
        this.author = author;
    }

    /// <summary>
    /// Generates the plugin scaffold directory with manifest and starter template.
    /// Fails loudly if the target directory already exists.
    /// </summary>
    public void Generate()
    {
        string pluginDir = Path.Combine(this.rootDir, this.pluginId);

        if (Directory.Exists(pluginDir))
        {
            throw new InvalidOperationException(
                "✗ Directory already exists: " + pluginDir + ". Choose a different plugin id or directory.");
        }

        string manifestContent = this.BuildManifest();
        string templateContent = this.BuildStarterTemplate();

        FileSystem.WriteFile(Path.Combine(pluginDir, "olav.plugin.json"), manifestContent);
        FileSystem.WriteFile(
            Path.Combine(pluginDir, "templates", this.pluginId + "-wiring.cs.sbn"),
            templateContent);

        Console.WriteLine("✓ Plugin scaffold created: " + pluginDir);
    }

    private static string PascalCase(string hyphenated)
    {
        string[] parts = hyphenated.Split('-');
        StringBuilder sb = new StringBuilder();
        foreach (string part in parts)
        {
            if (part.Length > 0)
            {
                sb.Append(char.ToUpperInvariant(part[0]));
                sb.Append(part.Substring(1));
            }
        }

        return sb.ToString();
    }

    private string BuildManifest()
    {
        string currentVersion = VersionConstants.ToolVersion;
        string pascalId = PascalCase(this.pluginId);
        string outputPath = "src/{{namespace}}.Infrastructure/" + pascalId + "DependencyInjection.cs";

        return "{"
            + "\n  \"id\": \"" + this.pluginId + "\","
            + "\n  \"version\": \"0.1.0\","
            + "\n  \"displayName\": \"" + this.pluginId + "\","
            + "\n  \"description\": \"\","
            + "\n  \"author\": \"" + this.author + "\","
            + "\n  \"category\": \"" + this.category + "\","
            + "\n  \"delivery\": \"" + this.delivery + "\","
            + "\n  \"compatibility\": { \"olavMinVersion\": \"" + currentVersion + "\" },"
            + "\n  \"generates\": ["
            + "\n    { \"template\": \"" + this.pluginId + "-wiring.cs.sbn\", \"output\": \"" + outputPath + "\" }"
            + "\n  ],"
            + "\n  \"parameters\": ["
            + "\n    { \"name\": \"namespace\", \"type\": \"string\" }"
            + "\n  ],"
            + "\n  \"conflicts\": [],"
            + "\n  \"requires\": []"
            + "\n}";
    }

    private string BuildStarterTemplate()
    {
        string pascalId = PascalCase(this.pluginId);
        return "// DI wiring for {{ namespace }}." + pascalId + "\n"
            + "// TODO: implement registration logic\n";
    }
}
