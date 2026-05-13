// <copyright file="OlavPluginManifest.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Infrastructure;

using System.Text.Json.Serialization;

/// <summary>
/// Deserialised representation of an <c>olav.plugin.json</c> manifest.
/// </summary>
public class OlavPluginManifest
{
    /// <summary>Gets or sets the plugin identifier (lowercase, hyphen-separated).</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the SemVer version.</summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    /// <summary>Gets or sets the human-readable display name.</summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the short description.</summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the author handle.</summary>
    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;

    /// <summary>Gets or sets the category: <c>infrastructure</c> or <c>deployment</c>.</summary>
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    /// <summary>Gets or sets the delivery model: <c>package</c> or <c>generation</c>.</summary>
    [JsonPropertyName("delivery")]
    public string Delivery { get; set; } = string.Empty;

    /// <summary>Gets or sets the compatibility constraints.</summary>
    [JsonPropertyName("compatibility")]
    public PluginCompatibility? Compatibility { get; set; }

    /// <summary>Gets or sets the NuGet package definition (required when delivery is <c>package</c>).</summary>
    [JsonPropertyName("nugetPackage")]
    public PluginNugetPackage? NugetPackage { get; set; }

    /// <summary>Gets or sets the list of template-to-output generation entries.</summary>
    [JsonPropertyName("generates")]
    public List<PluginGenerate> Generates { get; set; } = new List<PluginGenerate>();

    /// <summary>Gets or sets the parameter definitions.</summary>
    [JsonPropertyName("parameters")]
    public List<PluginParameter> Parameters { get; set; } = new List<PluginParameter>();

    /// <summary>Gets or sets IDs of plugins that cannot coexist with this one.</summary>
    [JsonPropertyName("conflicts")]
    public List<string> Conflicts { get; set; } = new List<string>();

    /// <summary>Gets or sets IDs of plugins that must be installed before this one.</summary>
    [JsonPropertyName("requires")]
    public List<string> Requires { get; set; } = new List<string>();

    /// <summary>Gets or sets service blocks to inject into existing local-type compose files.</summary>
    [JsonPropertyName("injectCompose")]
    public List<PluginInjectCompose> InjectCompose { get; set; } = new List<PluginInjectCompose>();

    /// <summary>
    /// Gets or sets the classlib suffix used to isolate this plugin's persistence code
    /// (e.g. <c>Persistence.Postgres</c>). When set, the installer creates
    /// <c>{ns}.Infrastructure.Persistence</c> and <c>{ns}.Infrastructure.{suffix}</c>
    /// classlib projects nested under the Infrastructure directory.
    /// <see langword="null"/> for non-persistence plugins.
    /// </summary>
    [JsonPropertyName("persistenceProjectSuffix")]
    public string? PersistenceProjectSuffix { get; set; }
}

/// <summary>Compatibility version constraints for a plugin.</summary>
public class PluginCompatibility
{
    /// <summary>Gets or sets the minimum Olav CLI version required to install this plugin.</summary>
    [JsonPropertyName("olavMinVersion")]
    public string OlavMinVersion { get; set; } = string.Empty;
}

/// <summary>NuGet package reference shipped by a <c>package</c>-delivery plugin.</summary>
public class PluginNugetPackage
{
    /// <summary>Gets or sets the NuGet package ID.</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the NuGet package version.</summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional custom NuGet feed URL.</summary>
    [JsonPropertyName("source")]
    public string? Source { get; set; }
}

/// <summary>One template-to-output entry inside a plugin manifest.</summary>
public class PluginGenerate
{
    /// <summary>Gets or sets the template filename (e.g. <c>postgres-di.cs.sbn</c>).</summary>
    [JsonPropertyName("template")]
    public string Template { get; set; } = string.Empty;

    /// <summary>Gets or sets the output path expression (supports <c>{{variable}}</c> substitution).</summary>
    [JsonPropertyName("output")]
    public string Output { get; set; } = string.Empty;
}

/// <summary>A service block to inject into an existing compose file when a plugin is installed.</summary>
public class PluginInjectCompose
{
    /// <summary>Gets or sets the template filename for the service-only YAML block (e.g. <c>postgres-service.yml.sbn</c>).</summary>
    [JsonPropertyName("serviceTemplate")]
    public string ServiceTemplate { get; set; } = string.Empty;

    /// <summary>Gets or sets the Docker Compose service name to inject (e.g. <c>postgres</c>).</summary>
    [JsonPropertyName("serviceName")]
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>Gets or sets the healthcheck condition for <c>depends_on</c>, or <c>null</c> for no dependency wiring.</summary>
    [JsonPropertyName("dependsOnCondition")]
    public string? DependsOnCondition { get; set; }
}

/// <summary>A parameter definition declared in a plugin manifest.</summary>
public class PluginParameter
{
    /// <summary>Gets or sets the parameter name (camelCase).</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the parameter type (e.g. <c>string</c>).</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>Gets or sets the default value, or <c>null</c> if the parameter is required.</summary>
    [JsonPropertyName("default")]
    public string? Default { get; set; }
}
