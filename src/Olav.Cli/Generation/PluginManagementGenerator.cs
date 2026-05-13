// <copyright file="PluginManagementGenerator.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Generation;

using System.Reflection;
using System.Text.Json;
using Olav.Infrastructure;

/// <summary>
/// Installed-plugin record exposed to the CLI layer (contains no Infrastructure types).
/// </summary>
public record InstalledPluginInfo
{
    /// <summary>Gets the plugin identifier.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Gets the installed version.</summary>
    public string Version { get; init; } = string.Empty;

    /// <summary>Gets the category.</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>Gets the delivery model.</summary>
    public string Delivery { get; init; } = string.Empty;

    /// <summary>Gets the install-time source label.</summary>
    public string Source { get; init; } = string.Empty;
}

/// <summary>
/// Named source entry exposed to the CLI layer.
/// </summary>
public record SourceInfo
{
    /// <summary>Gets the alias name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets the resolved URL.</summary>
    public string Url { get; init; } = string.Empty;

    /// <summary>Gets the scope: <c>built-in</c>, <c>global</c>, or <c>local</c>.</summary>
    public string Scope { get; init; } = string.Empty;
}

/// <summary>
/// Manages installed plugins and named source aliases for the CLI layer.
/// Bridges Commands to Infrastructure without exposing Infrastructure types.
/// </summary>
public class PluginManagementGenerator
{
    private readonly string root;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginManagementGenerator"/> class.
    /// </summary>
    /// <param name="root">Absolute path to the project root containing <c>olav.json</c>.</param>
    public PluginManagementGenerator(string root)
    {
        this.root = root;
    }

    /// <summary>Adds a named source alias to <c>~/.olav/config.json</c>.</summary>
    /// <param name="alias">The alias name.</param>
    /// <param name="url">The base URL for this source.</param>
    public static void AddGlobalSource(string alias, string url)
    {
        GlobalConfig config = GlobalConfig.Load();
        config.AddSource(alias, url);
        config.Save();
        Console.WriteLine("✓ Source '" + alias + "' added to global config.");
    }

    /// <summary>Removes a named source alias from <c>~/.olav/config.json</c>.</summary>
    /// <param name="alias">The alias name to remove.</param>
    public static void RemoveGlobalSource(string alias)
    {
        GlobalConfig config = GlobalConfig.Load();
        bool removed = config.RemoveSource(alias);
        if (!removed)
        {
            throw new InvalidOperationException(
                "✗ Source '" + alias + "' not found in global config.");
        }

        config.Save();
        Console.WriteLine("✓ Source '" + alias + "' removed from global config.");
    }

    /// <summary>Returns all installed plugins from <c>olav.json</c>.</summary>
    /// <returns>A read-only list of <see cref="InstalledPluginInfo"/>.</returns>
    public IReadOnlyList<InstalledPluginInfo> ListPlugins()
    {
        OlavConfig config = OlavConfig.Load(this.root);
        List<InstalledPluginInfo> result = new List<InstalledPluginInfo>();

        IReadOnlyList<PluginEntry> plugins = config.Plugins ?? new List<PluginEntry>();
        foreach (PluginEntry p in plugins)
        {
            result.Add(new InstalledPluginInfo
            {
                Id = p.Id,
                Version = p.Version,
                Category = p.Category,
                Delivery = p.Delivery,
                Source = p.Source,
            });
        }

        return result;
    }

    /// <summary>
    /// Removes the plugin with <paramref name="pluginId"/> from <c>olav.json</c>.
    /// Generated files are never deleted.
    /// </summary>
    /// <param name="pluginId">The plugin identifier to remove.</param>
    public void RemovePlugin(string pluginId)
    {
        OlavConfig config = OlavConfig.Load(this.root);

        bool removed = config.RemovePlugin(pluginId);
        if (!removed)
        {
            throw new InvalidOperationException(
                "✗ Plugin '" + pluginId + "' is not installed.");
        }

        config.Save(this.root);
        Console.WriteLine("✓ Plugin '" + pluginId + "' removed from olav.json.");
        Console.WriteLine("  Generated files were not deleted. Remove them manually if no longer needed.");
    }

    /// <summary>
    /// Returns the merged source list: built-in + global + local.
    /// Local entries override global entries with the same alias.
    /// </summary>
    /// <returns>A read-only list of <see cref="SourceInfo"/> ordered by scope.</returns>
    public IReadOnlyList<SourceInfo> ListSources()
    {
        List<SourceInfo> result = new List<SourceInfo>();

        Dictionary<string, Dictionary<string, string>> registry = LoadBuiltInRegistry();
        foreach (KeyValuePair<string, Dictionary<string, string>> category in registry)
        {
            foreach (KeyValuePair<string, string> entry in category.Value)
            {
                result.Add(new SourceInfo { Name = entry.Key, Url = entry.Value, Scope = "built-in" });
            }
        }

        GlobalConfig global = GlobalConfig.Load();
        if (global.Sources != null)
        {
            foreach (KeyValuePair<string, string> entry in global.Sources)
            {
                result.Add(new SourceInfo { Name = entry.Key, Url = entry.Value, Scope = "global" });
            }
        }

        OlavConfig local = OlavConfig.Load(this.root);
        if (local.Sources != null)
        {
            foreach (KeyValuePair<string, string> entry in local.Sources)
            {
                result.Add(new SourceInfo { Name = entry.Key, Url = entry.Value, Scope = "local" });
            }
        }

        return result;
    }

    private static Dictionary<string, Dictionary<string, string>> LoadBuiltInRegistry()
    {
        Assembly asm = Assembly.GetExecutingAssembly();
        using Stream stream = asm.GetManifestResourceStream("plugin-registry.json")
            ?? throw new InvalidOperationException("Built-in plugin registry resource not found.");
        using StreamReader reader = new StreamReader(stream);
        return JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(reader.ReadToEnd())
            ?? new Dictionary<string, Dictionary<string, string>>();
    }
}
