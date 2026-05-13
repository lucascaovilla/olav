#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Xunit;
using Olav.Infrastructure;
using Olav.Generation;

namespace Olav.IntegrationTests.Plugins;

public class PluginManagement_EndToEndTests : IClassFixture<PluginTestFixture>
{
    private readonly PluginTestFixture fixture;

    public PluginManagement_EndToEndTests(PluginTestFixture fixture)
    {
        this.fixture = fixture;
    }

    private string InstallLocalPlugin(string root, string pluginId, string category = "infrastructure")
    {
        string pluginDir = Path.Combine(Path.GetTempPath(), "mgmt-plugin-" + System.Guid.NewGuid().ToString());
        Directory.CreateDirectory(pluginDir);

        string manifest = JsonSerializer.Serialize(new
        {
            id = pluginId,
            version = "1.0.0",
            displayName = pluginId + " Plugin",
            category,
            delivery = "generation",
            generates = System.Array.Empty<object>(),
            parameters = System.Array.Empty<object>(),
            conflicts = System.Array.Empty<string>(),
            requires = System.Array.Empty<string>(),
        });

        File.WriteAllText(Path.Combine(pluginDir, "olav.plugin.json"), manifest);

        PluginInstallGenerator generator = PluginInstallGenerator.Create(root);
        generator.FetchInfo(pluginDir);
        generator.Install(pluginDir, new Dictionary<string, string>());

        return pluginDir;
    }

    [Fact]
    public void RemovePlugin_RemovesEntryFromOlavJson()
    {
        string root = this.fixture.ProjectPath;
        string pluginId = "removable-plugin-" + System.Guid.NewGuid().ToString("N").Substring(0, 8);

        InstallLocalPlugin(root, pluginId);

        OlavConfig before = OlavConfig.Load(root);
        Assert.NotNull(before.Plugins?.Find(p => p.Id == pluginId));

        PluginManagementGenerator manager = new(root);
        manager.RemovePlugin(pluginId);

        OlavConfig after = OlavConfig.Load(root);
        Assert.Null(after.Plugins?.Find(p => p.Id == pluginId));
    }

    [Fact]
    public void RemovePlugin_NonExistent_ThrowsError()
    {
        string root = this.fixture.ProjectPath;
        PluginManagementGenerator manager = new(root);

        System.InvalidOperationException ex = Assert.Throws<System.InvalidOperationException>(
            () => manager.RemovePlugin("plugin-that-does-not-exist-xyz"));

        Assert.Contains("not installed", ex.Message);
    }

    [Fact]
    public void ListPlugins_ReturnsInstalledPlugins()
    {
        string root = this.fixture.ProjectPath;
        string pluginId = "list-test-plugin-" + System.Guid.NewGuid().ToString("N").Substring(0, 8);

        InstallLocalPlugin(root, pluginId);

        PluginManagementGenerator manager = new(root);
        System.Collections.Generic.IReadOnlyList<InstalledPluginInfo> plugins = manager.ListPlugins();

        InstalledPluginInfo? found = null;
        foreach (InstalledPluginInfo p in plugins)
        {
            if (p.Id == pluginId)
            {
                found = p;
                break;
            }
        }

        Assert.NotNull(found);
        Assert.Equal("1.0.0", found!.Version);
    }

    [Fact]
    public void AddGlobalSource_ListSources_RemoveGlobalSource_WorksTogether()
    {
        string alias = "test-source-" + System.Guid.NewGuid().ToString("N").Substring(0, 8);
        string url = "https://example.com/plugins/" + alias;

        PluginManagementGenerator.AddGlobalSource(alias, url);

        try
        {
            PluginManagementGenerator manager = new(this.fixture.ProjectPath);
            System.Collections.Generic.IReadOnlyList<SourceInfo> sources = manager.ListSources();

            SourceInfo? found = null;
            foreach (SourceInfo s in sources)
            {
                if (s.Name == alias)
                {
                    found = s;
                    break;
                }
            }

            Assert.NotNull(found);
            Assert.Equal(url, found!.Url);
            Assert.Equal("global", found.Scope);
        }
        finally
        {
            PluginManagementGenerator.RemoveGlobalSource(alias);
        }
    }

    [Fact]
    public void RemovePlugin_GeneratedFilesRemainOnDisk()
    {
        string root = this.fixture.ProjectPath;
        string pluginId = "file-plugin-" + System.Guid.NewGuid().ToString("N").Substring(0, 8);

        string pluginDir = Path.Combine(Path.GetTempPath(), "file-plugin-" + System.Guid.NewGuid().ToString());
        Directory.CreateDirectory(pluginDir);

        string manifest = JsonSerializer.Serialize(new
        {
            id = pluginId,
            version = "1.0.0",
            displayName = "File Plugin",
            category = "infrastructure",
            delivery = "generation",
            generates = new object[] { new { template = "hello.txt", output = "generated-hello.txt" } },
            parameters = System.Array.Empty<object>(),
            conflicts = System.Array.Empty<string>(),
            requires = System.Array.Empty<string>(),
        });

        File.WriteAllText(Path.Combine(pluginDir, "olav.plugin.json"), manifest);
        Directory.CreateDirectory(Path.Combine(pluginDir, "templates"));
        File.WriteAllText(Path.Combine(pluginDir, "templates", "hello.txt"), "Hello!");

        PluginInstallGenerator generator = PluginInstallGenerator.Create(root);
        generator.FetchInfo(pluginDir);
        generator.Install(pluginDir, new Dictionary<string, string>());

        string generatedFile = Path.Combine(root, "generated-hello.txt");
        Assert.True(File.Exists(generatedFile), "Expected generated file to exist before removal.");

        PluginManagementGenerator manager = new(root);
        manager.RemovePlugin(pluginId);

        Assert.True(File.Exists(generatedFile), "Generated file must not be deleted on plugin removal.");
    }
}
