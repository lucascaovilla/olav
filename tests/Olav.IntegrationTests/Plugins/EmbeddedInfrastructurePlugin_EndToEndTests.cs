#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;
using Olav.Infrastructure;
using Olav.Generation;

namespace Olav.IntegrationTests.Plugins;

/// <summary>
/// Integration tests for the bundled infrastructure plugins (postgres, sqlserver, redis).
/// Validates that embedded:// resolution, NuGet injection, DI wiring generation,
/// and Docker Compose service injection all work end-to-end.
/// </summary>
public class EmbeddedInfrastructurePlugin_EndToEndTests : IClassFixture<PluginTestFixture>
{
    private readonly PluginTestFixture fixture;

    public EmbeddedInfrastructurePlugin_EndToEndTests(PluginTestFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void AddInfrastructurePostgres_RecordsPluginInOlavJson()
    {
        string root = this.fixture.ProjectPath;
        EnsureInstalled(root, "postgres");

        OlavConfig config = OlavConfig.Load(root);
        PluginEntry? entry = config.Plugins?.Find(p => p.Id == "postgres");

        Assert.NotNull(entry);
        Assert.Equal("infrastructure", entry!.Category);
        Assert.Equal("package", entry.Delivery);
        Assert.Equal("official", entry.Source);
    }

    [Fact]
    public void AddInfrastructurePostgres_CreatesDiWiringFile()
    {
        string root = this.fixture.ProjectPath;
        EnsureInstalled(root, "postgres");

        string wiringFile = Path.Combine(
            root, "src", "PluginTestApp.Infrastructure", "PostgresDependencyInjection.cs");

        Assert.True(File.Exists(wiringFile), "Expected PostgresDependencyInjection.cs to be generated.");
        Assert.Contains("AddPostgres", File.ReadAllText(wiringFile));
    }

    [Fact]
    public void AddInfrastructurePostgres_InjectsServiceIntoLocalCompose()
    {
        string root = this.fixture.ProjectPath;
        EnsureInstalled(root, "postgres");

        string localCompose = Path.Combine(root, "docker", "docker-compose.local.yml");
        string content = File.ReadAllText(localCompose);
        Assert.Contains("postgres:", content);
        Assert.Contains("healthcheck:", content);
        Assert.Contains("depends_on:", content);
    }

    [Fact]
    public void AddInfrastructurePostgres_AddsPackageReferenceToInfrastructureCsproj()
    {
        string root = this.fixture.ProjectPath;
        EnsureInstalled(root, "postgres");

        string csprojPath = Directory.GetFiles(
            Path.Combine(root, "src"), "*.Infrastructure.csproj", SearchOption.AllDirectories)[0];

        XDocument doc = XDocument.Load(csprojPath);
        bool found = doc.Descendants("PackageReference")
            .Any(e => e.Attribute("Include")?.Value == "Olav.Infrastructure.Postgres");

        Assert.True(found, "Expected PackageReference for Olav.Infrastructure.Postgres in .csproj.");
    }

    [Fact]
    public void AddInfrastructureRedis_InjectsServiceIntoLocalCompose()
    {
        string root = this.fixture.ProjectPath;
        EnsureInstalled(root, "redis");

        string localCompose = Path.Combine(root, "docker", "docker-compose.local.yml");
        string content = File.ReadAllText(localCompose);
        Assert.Contains("redis:", content);
        Assert.Contains("healthcheck:", content);
        Assert.Contains("depends_on:", content);
    }

    [Fact]
    public void AddInfrastructureSqlserver_ConflictsWithPostgres()
    {
        string root = this.fixture.ProjectPath;
        EnsureInstalled(root, "postgres");

        PluginInstallGenerator genSql = PluginInstallGenerator.Create(root);
        genSql.FetchInfo("sqlserver");

        System.InvalidOperationException ex = Assert.Throws<System.InvalidOperationException>(
            () => genSql.Install("sqlserver", new Dictionary<string, string>()));

        Assert.Contains("conflicts", ex.Message);
    }

    private static void EnsureInstalled(string root, string pluginId)
    {
        OlavConfig config = OlavConfig.Load(root);
        if (config.Plugins?.Any(p => p.Id == pluginId) == true)
        {
            return;
        }

        PluginInstallGenerator generator = PluginInstallGenerator.Create(root);
        generator.FetchInfo(pluginId);
        generator.Install(pluginId, new Dictionary<string, string>());
    }
}
