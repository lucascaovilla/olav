// <copyright file="OlavConfig.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Infrastructure;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Typed reader / writer for <c>olav.json</c>.
/// Preserves all unrecognised fields on round-trip via <see cref="ExtraData"/>.
/// </summary>
public class OlavConfig
{
    private static readonly HashSet<string> DbPluginIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "postgres",
        "sqlserver",
        "redis",
    };

    private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>Gets or sets the Olav CLI version that created or last updated the file.</summary>
    [JsonPropertyName("toolVersion")]
    public string? ToolVersion { get; set; }

    /// <summary>Gets or sets the template schema version.</summary>
    [JsonPropertyName("templateVersion")]
    public string? TemplateVersion { get; set; }

    /// <summary>Gets or sets the ISO-8601 timestamp of initial project creation.</summary>
    [JsonPropertyName("createdAt")]
    public string? CreatedAt { get; set; }

    /// <summary>Gets or sets the ISO-8601 timestamp of the last Olav-managed update.</summary>
    [JsonPropertyName("updatedAt")]
    public string? UpdatedAt { get; set; }

    /// <summary>Gets or sets the project-scoped named source aliases.</summary>
    [JsonPropertyName("sources")]
    public Dictionary<string, string>? Sources { get; set; }

    /// <summary>Gets or sets the list of installed plugin records.</summary>
    [JsonPropertyName("plugins")]
    public List<PluginEntry>? Plugins { get; set; }

    /// <summary>Gets or sets extra fields not mapped by this version of Olav (round-trip preservation).</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtraData { get; set; }

    /// <summary>
    /// Loads and deserialises <c>olav.json</c> from <paramref name="root"/>.
    /// Returns an empty config if the file does not exist.
    /// </summary>
    /// <param name="root">Absolute path to the project root containing <c>olav.json</c>.</param>
    /// <returns>A <see cref="OlavConfig"/> instance (never null).</returns>
    public static OlavConfig Load(string root)
    {
        string path = Path.Combine(root, "olav.json");
        if (!File.Exists(path))
        {
            return new OlavConfig();
        }

        string json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<OlavConfig>(json, SerializerOptions) ?? new OlavConfig();
    }

    /// <summary>
    /// Serialises this config and writes it to <c>olav.json</c> in <paramref name="root"/>.
    /// </summary>
    /// <param name="root">Absolute path to the project root.</param>
    public void Save(string root)
    {
        string path = Path.Combine(root, "olav.json");
        File.WriteAllText(path, JsonSerializer.Serialize(this, SerializerOptions));
    }

    /// <summary>
    /// Returns the id of the first installed infrastructure DB plugin, or <see langword="null"/> if none.
    /// </summary>
    /// <returns>Plugin id string, or <see langword="null"/>.</returns>
    public string? ResolveInfrastructurePlugin()
    {
        return this.Plugins?
            .FirstOrDefault(p => p.Category == "infrastructure" && DbPluginIds.Contains(p.Id))
            ?.Id;
    }

    /// <summary>Appends a plugin entry to the in-memory list (does not save).</summary>
    /// <param name="entry">The plugin entry to add.</param>
    public void AddPlugin(PluginEntry entry)
    {
        if (this.Plugins == null)
        {
            this.Plugins = new List<PluginEntry>();
        }

        this.Plugins.Add(entry);
    }

    /// <summary>Removes the plugin with the given <paramref name="id"/> (does not save).</summary>
    /// <param name="id">The plugin identifier to remove.</param>
    /// <returns><c>true</c> if a matching plugin was found and removed; otherwise <c>false</c>.</returns>
    public bool RemovePlugin(string id)
    {
        int removed = this.Plugins?.RemoveAll(p => p.Id == id) ?? 0;
        return removed > 0;
    }

    /// <summary>Adds or overwrites a local source alias (does not save).</summary>
    /// <param name="alias">The alias name.</param>
    /// <param name="url">The base URL for this source.</param>
    public void AddSource(string alias, string url)
    {
        if (this.Sources == null)
        {
            this.Sources = new Dictionary<string, string>();
        }

        this.Sources[alias] = url;
    }

    /// <summary>Removes a local source alias (does not save).</summary>
    /// <param name="alias">The alias name to remove.</param>
    /// <returns><c>true</c> if the alias existed and was removed; otherwise <c>false</c>.</returns>
    public bool RemoveSource(string alias)
    {
        return this.Sources?.Remove(alias) ?? false;
    }
}
