// <copyright file="PluginInstallGenerator.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Generation;

using Olav.Infrastructure;

/// <summary>
/// Return value of <see cref="PluginInstallGenerator.FetchInfo"/> —
/// carries plugin metadata and parameter definitions needed by the CLI
/// layer for interactive prompting.
/// </summary>
public record PluginInfoResult
{
    /// <summary>Gets the plugin identifier.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Gets the SemVer version string.</summary>
    public string Version { get; init; } = string.Empty;

    /// <summary>Gets the human-readable display name.</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>Gets the category: <c>infrastructure</c> or <c>deployment</c>.</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>Gets the delivery model: <c>package</c> or <c>generation</c>.</summary>
    public string Delivery { get; init; } = string.Empty;

    /// <summary>Gets the ordered list of parameter definitions.</summary>
    public IReadOnlyList<PluginParameterDefinition> Parameters { get; init; } = new List<PluginParameterDefinition>();
}

/// <summary>
/// A single parameter definition from the plugin manifest,
/// surfaced to the CLI layer without exposing Infrastructure types.
/// </summary>
public record PluginParameterDefinition
{
    /// <summary>Gets the parameter name (camelCase).</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets the parameter type (e.g. <c>string</c>).</summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>Gets the default value, or <c>null</c> when the parameter is required.</summary>
    public string? Default { get; init; }
}

/// <summary>
/// Orchestrates the full plugin install flow:
/// resolve → fetch → validate → deliver → record.
/// Receives all parameters as a pre-built dictionary; never prompts the user.
/// </summary>
public class PluginInstallGenerator
{
    private readonly string root;
    private readonly PluginSourceResolver resolver;
    private OlavPluginManifest? cachedManifest;
    private string? cachedResolvedSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginInstallGenerator"/> class.
    /// </summary>
    /// <param name="root">Absolute path to the project root containing <c>olav.json</c>.</param>
    /// <param name="resolver">Resolver for source aliases and URLs.</param>
    public PluginInstallGenerator(string root, PluginSourceResolver resolver)
    {
        this.root = root;
        this.resolver = resolver;
    }

    /// <summary>
    /// Creates a fully wired <see cref="PluginInstallGenerator"/> for the project at
    /// <paramref name="root"/> using the default HTTP client.
    /// </summary>
    /// <param name="root">Project root containing <c>olav.json</c>.</param>
    /// <returns>A ready-to-use <see cref="PluginInstallGenerator"/>.</returns>
    public static PluginInstallGenerator Create(string root)
    {
        OlavConfig localConfig = OlavConfig.Load(root);
        GlobalConfig globalConfig = GlobalConfig.Load();
        PluginSourceResolver resolver = new PluginSourceResolver(localConfig, globalConfig);
        return new PluginInstallGenerator(root, resolver);
    }

    /// <summary>
    /// Resolves and fetches the manifest for <paramref name="rawSource"/>,
    /// then returns a Generation-layer summary suitable for CLI interaction.
    /// The manifest is cached so the subsequent <see cref="Install"/> call avoids a second fetch.
    /// </summary>
    /// <param name="rawSource">Raw source string (alias, URL, or local path).</param>
    /// <returns>A <see cref="PluginInfoResult"/> containing metadata and parameter definitions.</returns>
    public PluginInfoResult FetchInfo(string rawSource)
    {
        string resolved = this.resolver.Resolve(rawSource);
        OlavPluginManifest manifest = PluginManifestFetcher.Fetch(resolved);
        this.cachedManifest = manifest;
        this.cachedResolvedSource = resolved;
        return ToPluginInfoResult(manifest);
    }

    /// <summary>
    /// Executes the full install sequence for the plugin at <paramref name="rawSource"/>
    /// using the provided <paramref name="parameters"/>.
    /// </summary>
    /// <param name="rawSource">Raw source string (alias, URL, or local path).</param>
    /// <param name="parameters">Pre-collected parameter values (after interactive prompting in the Command).</param>
    public void Install(string rawSource, Dictionary<string, string> parameters)
    {
        string resolved = this.resolver.Resolve(rawSource);
        OlavPluginManifest manifest =
            (this.cachedResolvedSource == resolved && this.cachedManifest != null)
                ? this.cachedManifest
                : PluginManifestFetcher.Fetch(resolved);

        ValidateCompatibility(manifest);

        OlavConfig config = OlavConfig.Load(this.root);
        CheckDoubleInstall(config, manifest.Id);
        CheckConflicts(config, manifest);
        CheckRequires(config, manifest);

        Dictionary<string, string> resolvedParams = MergeWithDefaults(manifest, parameters);

        string ns = this.DiscoverNamespace();
        if (!string.IsNullOrEmpty(ns))
        {
            if (!resolvedParams.ContainsKey("namespace"))
            {
                resolvedParams["namespace"] = ns;
            }

            if (!resolvedParams.ContainsKey("projectName"))
            {
                resolvedParams["projectName"] = ns;
            }

            if (!resolvedParams.ContainsKey("projectNameLower"))
            {
                resolvedParams["projectNameLower"] = ns.ToLowerInvariant();
            }
        }

        if (!string.IsNullOrEmpty(manifest.PersistenceProjectSuffix) && !string.IsNullOrEmpty(ns))
        {
            this.EnsurePersistenceProjects(ns, manifest.PersistenceProjectSuffix);
        }

        if (manifest.Delivery == "package" && manifest.NugetPackage != null)
        {
            string csprojPath = !string.IsNullOrEmpty(manifest.PersistenceProjectSuffix) && !string.IsNullOrEmpty(ns)
                ? this.FindPersistenceCsproj(ns, manifest.PersistenceProjectSuffix)
                : this.FindInfrastructureCsproj();

            PluginNugetInjector.AddPackageReference(
                csprojPath,
                manifest.NugetPackage.Id,
                manifest.NugetPackage.Version);

            if (!string.IsNullOrWhiteSpace(manifest.NugetPackage.Source))
            {
                string nugetConfigPath = Path.Combine(this.root, "nuget.config");
                PluginNugetInjector.AddPackageSource(
                    nugetConfigPath,
                    manifest.Id,
                    manifest.NugetPackage.Source);
            }
        }

        foreach (PluginGenerate gen in manifest.Generates)
        {
            string templateContent = PluginTemplateCache.GetTemplateContent(
                resolved, gen.Template, manifest.Id, manifest.Version);

            string outputRelative = RenderString(gen.Output, resolvedParams);
            string fullOutputPath = Path.Combine(this.root, outputRelative);

            if (File.Exists(fullOutputPath))
            {
                Console.WriteLine("⚠ File already exists, skipping: " + outputRelative);
                continue;
            }

            string renderedContent = RenderString(templateContent, resolvedParams);
            string? dir = Path.GetDirectoryName(fullOutputPath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(fullOutputPath, renderedContent);
            Console.WriteLine("[OK] " + outputRelative);
        }

        // Inject service blocks into local-type compose files for infrastructure plugins
        foreach (PluginInjectCompose injectEntry in manifest.InjectCompose)
        {
            string serviceBlock = RenderString(
                PluginTemplateCache.GetTemplateContent(resolved, injectEntry.ServiceTemplate, manifest.Id, manifest.Version),
                resolvedParams);

            foreach (string composePath in new[]
            {
                Path.Combine(this.root, "docker", "docker-compose.local.yml"),
                Path.Combine(this.root, "docker", "docker-compose.dev.yml"),
            })
            {
                if (File.Exists(composePath))
                {
                    DockerComposeInjector.InjectService(composePath, injectEntry.ServiceName, serviceBlock, injectEntry.DependsOnCondition);
                    Console.WriteLine("[OK] Injected " + injectEntry.ServiceName + " into " + Path.GetFileName(composePath));
                }
            }
        }

        // Back-inject already-installed infra services into the docker plugin's new dev compose
        if (manifest.Id == "docker")
        {
            string devComposePath = Path.Combine(this.root, "docker", "docker-compose.dev.yml");
            if (File.Exists(devComposePath))
            {
                OlavConfig currentConfig = OlavConfig.Load(this.root);
                foreach (PluginEntry infra in (currentConfig.Plugins ?? new List<PluginEntry>()).Where(p => p.Category == "infrastructure"))
                {
                    string infraResolved = infra.Source == "official"
                        ? "embedded://" + infra.Id
                        : infra.Source;

                    OlavPluginManifest infraManifest = PluginManifestFetcher.Fetch(infraResolved);
                    foreach (PluginInjectCompose inject in infraManifest.InjectCompose)
                    {
                        string block = RenderString(
                            PluginTemplateCache.GetTemplateContent(infraResolved, inject.ServiceTemplate, infra.Id, infra.Version),
                            resolvedParams);
                        DockerComposeInjector.InjectService(devComposePath, inject.ServiceName, block, inject.DependsOnCondition);
                        Console.WriteLine("[OK] Back-injected " + inject.ServiceName + " into docker-compose.dev.yml");
                    }
                }
            }
        }

        string sourceLabel = IsOfficialSource(resolved) ? "official" : resolved;

        config.AddPlugin(new PluginEntry
        {
            Id = manifest.Id,
            Version = manifest.Version,
            Category = manifest.Category,
            Source = sourceLabel,
            Delivery = manifest.Delivery,
        });
        config.Save(this.root);

        Console.WriteLine("✓ Plugin '" + manifest.Id + "' installed successfully.");
    }

    private static bool IsOfficialSource(string resolved)
    {
        return resolved.StartsWith("embedded://", StringComparison.OrdinalIgnoreCase) ||
               resolved.StartsWith("https://plugins.olav.dev/", StringComparison.OrdinalIgnoreCase);
    }

    private static PluginInfoResult ToPluginInfoResult(OlavPluginManifest manifest)
    {
        List<PluginParameterDefinition> paramDefs = new List<PluginParameterDefinition>();
        foreach (PluginParameter p in manifest.Parameters)
        {
            paramDefs.Add(new PluginParameterDefinition { Name = p.Name, Type = p.Type, Default = p.Default });
        }

        return new PluginInfoResult
        {
            Id = manifest.Id,
            Version = manifest.Version,
            DisplayName = manifest.DisplayName,
            Category = manifest.Category,
            Delivery = manifest.Delivery,
            Parameters = paramDefs,
        };
    }

    private static void ValidateCompatibility(OlavPluginManifest manifest)
    {
        if (string.IsNullOrEmpty(manifest.Compatibility?.OlavMinVersion))
        {
            return;
        }

        Version? minRequired = ParseVersionSafe(manifest.Compatibility.OlavMinVersion);
        Version? toolVersion = ParseVersionSafe(VersionConstants.ToolVersion);

        if (minRequired != null && toolVersion != null && toolVersion < minRequired)
        {
            throw new InvalidOperationException(
                "✗ Plugin '" + manifest.Id + "' requires Olav >= " +
                manifest.Compatibility.OlavMinVersion + ". " +
                "Current version: " + VersionConstants.ToolVersion + ". Update the Olav CLI.");
        }
    }

    private static void CheckDoubleInstall(OlavConfig config, string pluginId)
    {
        bool alreadyInstalled = config.Plugins?.Any(p => p.Id == pluginId) == true;
        if (alreadyInstalled)
        {
            throw new InvalidOperationException(
                "✗ Plugin '" + pluginId + "' is already installed.");
        }
    }

    private static void CheckConflicts(OlavConfig config, OlavPluginManifest manifest)
    {
        if (config.Plugins == null)
        {
            return;
        }

        foreach (string conflictId in manifest.Conflicts)
        {
            if (config.Plugins.Any(p => p.Id == conflictId))
            {
                throw new InvalidOperationException(
                    "✗ Plugin '" + manifest.Id + "' conflicts with installed plugin '" + conflictId + "'.");
            }
        }
    }

    private static void CheckRequires(OlavConfig config, OlavPluginManifest manifest)
    {
        foreach (string requiredId in manifest.Requires)
        {
            bool found = config.Plugins?.Any(p => p.Id == requiredId) == true;
            if (!found)
            {
                throw new InvalidOperationException(
                    "✗ Plugin '" + manifest.Id + "' requires '" + requiredId +
                    "' to be installed first. Run: olav add infrastructure " + requiredId);
            }
        }
    }

    private static Dictionary<string, string> MergeWithDefaults(
        OlavPluginManifest manifest,
        Dictionary<string, string> providedParams)
    {
        Dictionary<string, string> resolved =
            new Dictionary<string, string>(providedParams, StringComparer.OrdinalIgnoreCase);

        foreach (PluginParameter param in manifest.Parameters)
        {
            if (!resolved.ContainsKey(param.Name))
            {
                if (param.Default != null)
                {
                    resolved[param.Name] = param.Default;
                }
                else
                {
                    throw new InvalidOperationException(
                        "✗ Required parameter '" + param.Name + "' was not provided.");
                }
            }
        }

        return resolved;
    }

    private static string RenderString(string template, Dictionary<string, string> variables)
    {
        foreach (KeyValuePair<string, string> kv in variables)
        {
            template = template
                .Replace("{{ " + kv.Key + " }}", kv.Value)
                .Replace("{{" + kv.Key + "}}", kv.Value);
        }

        return template;
    }

    private static Version? ParseVersionSafe(string raw)
    {
        int dots = 0;
        foreach (char c in raw)
        {
            if (c == '.')
            {
                dots++;
            }
        }

        string normalised = dots == 0 ? raw + ".0.0" : dots == 1 ? raw + ".0" : raw;
        return Version.TryParse(normalised, out Version? v) ? v : null;
    }

    private void EnsurePersistenceProjects(string ns, string suffix)
    {
        string srcDir = Path.Combine(this.root, "src");
        string infraDir = Path.Combine(srcDir, $"{ns}.Infrastructure");
        string persistenceDir = Path.Combine(infraDir, "Persistence");
        string pluginShortName = suffix.Split('.')[^1]; // e.g. "Postgres" from "Persistence.Postgres"
        string targetDir = Path.Combine(persistenceDir, pluginShortName);

        string persistenceName = $"{ns}.Infrastructure.Persistence";
        string targetName = $"{ns}.Infrastructure.{suffix}";

        string persistenceCsproj = Path.Combine(persistenceDir, $"{persistenceName}.csproj");
        string targetCsproj = Path.Combine(targetDir, $"{targetName}.csproj");

        // Infrastructure.Persistence — shared abstract layer
        if (!File.Exists(persistenceCsproj))
        {
            Directory.CreateDirectory(persistenceDir);
            DotnetRunner.Run($"new classlib -n {persistenceName} -f net10.0 -o \"{persistenceDir}\"", this.root);
            DotnetRunner.Run($"sln add \"{persistenceCsproj}\"", this.root);
            DotnetRunner.Run($"add \"{persistenceCsproj}\" reference \"src/{ns}.Application/{ns}.Application.csproj\"", this.root);
            DotnetRunner.Run($"add \"src/{ns}.Infrastructure/{ns}.Infrastructure.csproj\" reference \"{persistenceCsproj}\"", this.root);
        }

        // Infrastructure.Persistence.Postgres — concrete implementation
        if (!File.Exists(targetCsproj))
        {
            Directory.CreateDirectory(targetDir);
            DotnetRunner.Run($"new classlib -n {targetName} -f net10.0 -o \"{targetDir}\"", this.root);
            DotnetRunner.Run($"sln add \"{targetCsproj}\"", this.root);
            DotnetRunner.Run($"add \"{targetCsproj}\" reference \"src/{ns}.Domain/{ns}.Domain.csproj\"", this.root);
            DotnetRunner.Run($"add \"{targetCsproj}\" reference \"{persistenceCsproj}\"", this.root);

            // Persistence calls AddPostgres() from the concrete project, so Persistence references it
            DotnetRunner.Run($"add \"{persistenceCsproj}\" reference \"{targetCsproj}\"", this.root);
        }
    }

    private string FindPersistenceCsproj(string ns, string suffix)
    {
        string pluginShortName = suffix.Split('.')[^1];
        string targetDir = Path.Combine(
            this.root, "src", $"{ns}.Infrastructure", "Persistence", pluginShortName);
        string targetCsproj = Path.Combine(targetDir, $"{ns}.Infrastructure.{suffix}.csproj");

        if (!File.Exists(targetCsproj))
        {
            throw new InvalidOperationException(
                "✗ Persistence project not found: " + targetCsproj +
                ". Run 'olav add infrastructure' to install the persistence plugin first.");
        }

        return targetCsproj;
    }

    private string DiscoverNamespace()
    {
        string srcDir = Path.Combine(this.root, "src");
        if (!Directory.Exists(srcDir))
        {
            return string.Empty;
        }

        // Only match the base Infrastructure project (name ends exactly with ".Infrastructure")
        string[] csprojFiles = Directory.GetFiles(srcDir, "*.Infrastructure.csproj", SearchOption.AllDirectories)
            .Where(f => Path.GetFileNameWithoutExtension(f).EndsWith(".Infrastructure", StringComparison.Ordinal))
            .ToArray();

        if (csprojFiles.Length == 0)
        {
            return string.Empty;
        }

        string fileNameNoExt = Path.GetFileNameWithoutExtension(csprojFiles[0]);
        string[] parts = fileNameNoExt.Split('.');
        if (parts.Length > 1)
        {
            return string.Join(".", parts, 0, parts.Length - 1);
        }

        return fileNameNoExt;
    }

    private string FindInfrastructureCsproj()
    {
        string srcDir = Path.Combine(this.root, "src");
        if (!Directory.Exists(srcDir))
        {
            throw new InvalidOperationException(
                "✗ No 'src' directory found. Is this an Olav project?");
        }

        string[] csprojFiles = Directory.GetFiles(srcDir, "*.Infrastructure.csproj", SearchOption.AllDirectories)
            .Where(f => Path.GetFileNameWithoutExtension(f).EndsWith(".Infrastructure", StringComparison.Ordinal))
            .ToArray();

        if (csprojFiles.Length == 0)
        {
            throw new InvalidOperationException(
                "✗ No Infrastructure project found. Run 'olav new' to scaffold a project first.");
        }

        return csprojFiles[0];
    }
}
