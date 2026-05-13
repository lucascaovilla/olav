#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;
using Xunit;
using Olav.Infrastructure;
using Olav.Generation;

namespace Olav.IntegrationTests.Plugins;

/// <summary>
/// Integration tests for package-delivery plugins (NuGet injection path).
/// Covers: olav add infrastructure &lt;pkg-plugin&gt;, double-install, conflict.
/// Uses local mock plugin manifests — no network access required.
/// </summary>
public class PackageDelivery_EndToEndTests : IClassFixture<PluginTestFixture>
{
    private readonly PluginTestFixture fixture;

    public PackageDelivery_EndToEndTests(PluginTestFixture fixture)
    {
        this.fixture = fixture;
    }

    private static string CreatePackagePlugin(
        string id,
        string nugetId,
        string nugetVersion,
        string[]? conflicts = null,
        string[]? generates = null,
        string[]? requires = null)
    {
        string pluginDir = Path.Combine(Path.GetTempPath(), "pkg-plugin-" + Guid.NewGuid());
        Directory.CreateDirectory(pluginDir);

        object[] generatesArr = generates != null
            ? generates.Select(g => (object)new { template = g + ".txt", output = g + "-output.txt" }).ToArray()
            : Array.Empty<object>();

        if (generates != null)
        {
            string templatesDir = Path.Combine(pluginDir, "templates");
            Directory.CreateDirectory(templatesDir);
            foreach (string g in generates)
            {
                File.WriteAllText(Path.Combine(templatesDir, g + ".txt"), "// DI wiring for " + id);
            }
        }

        string manifest = JsonSerializer.Serialize(new
        {
            id,
            version = "1.0.0",
            displayName = id + " Plugin",
            category = "infrastructure",
            delivery = "package",
            nugetPackage = new { id = nugetId, version = nugetVersion },
            generates = generatesArr,
            parameters = Array.Empty<object>(),
            conflicts = conflicts ?? Array.Empty<string>(),
            requires = requires ?? Array.Empty<string>(),
        });

        File.WriteAllText(Path.Combine(pluginDir, "olav.plugin.json"), manifest);
        return pluginDir;
    }

    [Fact]
    public void AddPackagePlugin_AddsPackageReferenceToInfrastructureCsproj()
    {
        string root = this.fixture.ProjectPath;
        string pluginDir = CreatePackagePlugin("mock-db", "Mock.Db.EfCore", "3.0.0");

        PluginInstallGenerator generator = PluginInstallGenerator.Create(root);
        generator.FetchInfo(pluginDir);
        generator.Install(pluginDir, new Dictionary<string, string>());

        string csprojPath = Directory.GetFiles(
            Path.Combine(root, "src"), "*.Infrastructure.csproj", SearchOption.AllDirectories)[0];

        XDocument doc = XDocument.Load(csprojPath);
        bool found = doc.Descendants("PackageReference")
            .Any(e => e.Attribute("Include")?.Value == "Mock.Db.EfCore"
                   && e.Attribute("Version")?.Value == "3.0.0");

        Assert.True(found, "Expected PackageReference for Mock.Db.EfCore to be added to the .csproj.");
    }

    [Fact]
    public void AddPackagePlugin_RecordsPluginInOlavJson()
    {
        string root = this.fixture.ProjectPath;
        string pluginId = "record-pkg-" + Guid.NewGuid().ToString("N").Substring(0, 8);
        string pluginDir = CreatePackagePlugin(pluginId, "Record.Pkg", "1.0.0");

        PluginInstallGenerator generator = PluginInstallGenerator.Create(root);
        generator.FetchInfo(pluginDir);
        generator.Install(pluginDir, new Dictionary<string, string>());

        OlavConfig config = OlavConfig.Load(root);
        PluginEntry? entry = config.Plugins?.Find(p => p.Id == pluginId);

        Assert.NotNull(entry);
        Assert.Equal("infrastructure", entry!.Category);
        Assert.Equal("package", entry.Delivery);
        Assert.Equal("1.0.0", entry.Version);
    }

    [Fact]
    public void AddPackagePlugin_DoubleInstall_HardError_OlavJsonHasExactlyOneEntry()
    {
        string root = this.fixture.ProjectPath;
        string pluginId = "double-pkg-" + Guid.NewGuid().ToString("N").Substring(0, 8);
        string pluginDir = CreatePackagePlugin(pluginId, "Double.Pkg", "1.0.0");

        PluginInstallGenerator first = PluginInstallGenerator.Create(root);
        first.FetchInfo(pluginDir);
        first.Install(pluginDir, new Dictionary<string, string>());

        PluginInstallGenerator second = PluginInstallGenerator.Create(root);
        second.FetchInfo(pluginDir);

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
            () => second.Install(pluginDir, new Dictionary<string, string>()));

        Assert.Contains("already installed", ex.Message);

        OlavConfig config = OlavConfig.Load(root);
        int count = config.Plugins?.Count(p => p.Id == pluginId) ?? 0;
        Assert.Equal(1, count);
    }

    [Fact]
    public void AddSqlserver_AfterPostgres_ConflictsError()
    {
        string root = this.fixture.ProjectPath;

        string postgresId = "pg-conflict-" + Guid.NewGuid().ToString("N").Substring(0, 8);
        string sqlserverId = "sql-conflict-" + Guid.NewGuid().ToString("N").Substring(0, 8);

        string postgresDir = CreatePackagePlugin(postgresId, "Pg.Pkg", "1.0.0");
        string sqlserverDir = CreatePackagePlugin(sqlserverId, "Sql.Pkg", "1.0.0", conflicts: new[] { postgresId });

        PluginInstallGenerator genPg = PluginInstallGenerator.Create(root);
        genPg.FetchInfo(postgresDir);
        genPg.Install(postgresDir, new Dictionary<string, string>());

        PluginInstallGenerator genSql = PluginInstallGenerator.Create(root);
        genSql.FetchInfo(sqlserverDir);

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
            () => genSql.Install(sqlserverDir, new Dictionary<string, string>()));

        Assert.Contains("conflicts with", ex.Message);
        Assert.Contains(postgresId, ex.Message);
    }

    [Fact]
    public void AddPlugin_WhenRequiredPluginMissing_ThrowsError()
    {
        string root = this.fixture.ProjectPath;
        string depId = "dep-plugin-" + Guid.NewGuid().ToString("N").Substring(0, 8);
        string pluginId = "requiring-plugin-" + Guid.NewGuid().ToString("N").Substring(0, 8);

        string pluginDir = CreatePackagePlugin(pluginId, "Requiring.Pkg", "1.0.0", requires: new[] { depId });

        PluginInstallGenerator gen = PluginInstallGenerator.Create(root);
        gen.FetchInfo(pluginDir);

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
            () => gen.Install(pluginDir, new Dictionary<string, string>()));

        Assert.Contains("requires", ex.Message);
        Assert.Contains(depId, ex.Message);
    }

    [Fact]
    public void AddPackagePlugin_WithGenerates_CreatesWiringFile()
    {
        string root = this.fixture.ProjectPath;
        string pluginId = "wiring-pkg-" + Guid.NewGuid().ToString("N").Substring(0, 8);
        string pluginDir = CreatePackagePlugin(pluginId, "Wiring.Pkg", "1.0.0", generates: new[] { "di-wiring" });

        PluginInstallGenerator generator = PluginInstallGenerator.Create(root);
        generator.FetchInfo(pluginDir);
        generator.Install(pluginDir, new Dictionary<string, string>());

        string wiringFile = Path.Combine(root, "di-wiring-output.txt");
        Assert.True(File.Exists(wiringFile), "Expected DI wiring output file to be generated.");
        Assert.Contains("DI wiring for " + pluginId, File.ReadAllText(wiringFile));
    }
}
