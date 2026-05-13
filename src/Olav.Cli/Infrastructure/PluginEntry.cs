// <copyright file="PluginEntry.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Infrastructure;

using System.Text.Json.Serialization;

/// <summary>
/// Represents one installed-plugin record stored in <c>olav.json</c>.
/// </summary>
public class PluginEntry
{
    /// <summary>Gets or sets the plugin identifier.</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the installed version.</summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    /// <summary>Gets or sets the category: <c>infrastructure</c> or <c>deployment</c>.</summary>
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    /// <summary>Gets or sets the source used at install time: <c>official</c> or a resolved URL.</summary>
    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    /// <summary>Gets or sets the delivery model: <c>package</c> or <c>generation</c>.</summary>
    [JsonPropertyName("delivery")]
    public string Delivery { get; set; } = string.Empty;
}
