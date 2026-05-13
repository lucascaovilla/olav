// <copyright file="GlobalConfig.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Infrastructure;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Reader / writer for <c>~/.olav/config.json</c> (global, machine-wide source aliases).
/// The file is created on first write; a missing file is treated as an empty config.
/// </summary>
public class GlobalConfig
{
    private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>Gets or sets the globally defined named source aliases.</summary>
    [JsonPropertyName("sources")]
    public Dictionary<string, string>? Sources { get; set; }

    /// <summary>Returns the absolute path to the global config file.</summary>
    /// <returns>Absolute path to <c>~/.olav/config.json</c>.</returns>
    public static string GetConfigPath()
    {
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(home, ".olav", "config.json");
    }

    /// <summary>
    /// Loads and deserialises <c>~/.olav/config.json</c>.
    /// Returns an empty config if the file does not exist.
    /// </summary>
    /// <returns>A <see cref="GlobalConfig"/> instance (never null).</returns>
    public static GlobalConfig Load()
    {
        string path = GetConfigPath();
        if (!File.Exists(path))
        {
            return new GlobalConfig();
        }

        string json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<GlobalConfig>(json, SerializerOptions) ?? new GlobalConfig();
    }

    /// <summary>
    /// Serialises this config and writes it to <c>~/.olav/config.json</c>,
    /// creating the <c>.olav</c> directory if needed.
    /// </summary>
    public void Save()
    {
        string path = GetConfigPath();
        string? dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.WriteAllText(path, JsonSerializer.Serialize(this, SerializerOptions));
    }

    /// <summary>Adds or overwrites a named source alias (does not save).</summary>
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

    /// <summary>Removes a named source alias (does not save).</summary>
    /// <param name="alias">The alias name to remove.</param>
    /// <returns><c>true</c> if the alias existed and was removed; otherwise <c>false</c>.</returns>
    public bool RemoveSource(string alias)
    {
        return this.Sources?.Remove(alias) ?? false;
    }
}
