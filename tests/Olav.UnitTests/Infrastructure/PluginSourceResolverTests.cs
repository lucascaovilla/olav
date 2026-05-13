using System;
using System.IO;
using Xunit;
using Olav.Infrastructure;

namespace Olav.UnitTests.Infrastructure;

public class PluginSourceResolverTests
{
    [Fact]
    public void Resolve_AbsoluteHttpsUrl_ReturnsSameUrl()
    {
        OlavConfig local = new();
        GlobalConfig global = new();
        PluginSourceResolver resolver = new(local, global);

        string result = resolver.Resolve("https://example.com/plugins/my-plugin");

        Assert.Equal("https://example.com/plugins/my-plugin", result);
    }

    [Fact]
    public void Resolve_AbsoluteHttpUrl_ReturnsSameUrl()
    {
        OlavConfig local = new();
        GlobalConfig global = new();
        PluginSourceResolver resolver = new(local, global);

        string result = resolver.Resolve("http://example.com/plugins");

        Assert.Equal("http://example.com/plugins", result);
    }

    [Fact]
    public void Resolve_LocalRelativePath_ReturnsSamePath()
    {
        OlavConfig local = new();
        GlobalConfig global = new();
        PluginSourceResolver resolver = new(local, global);

        string result = resolver.Resolve("./plugins/my-plugin");

        Assert.Equal("./plugins/my-plugin", result);
    }

    [Fact]
    public void Resolve_LocalAbsolutePath_ReturnsSamePath()
    {
        OlavConfig local = new();
        GlobalConfig global = new();
        PluginSourceResolver resolver = new(local, global);

        string path = Path.Combine(Path.GetTempPath(), "my-plugin");
        string result = resolver.Resolve(path);

        Assert.Equal(path, result);
    }

    [Fact]
    public void Resolve_LocalConfigAlias_ReturnsLocalUrl()
    {
        OlavConfig local = new();
        local.AddSource("myalias", "https://local.example.com/plugins");
        GlobalConfig global = new();
        PluginSourceResolver resolver = new(local, global);

        string result = resolver.Resolve("myalias");

        Assert.Equal("https://local.example.com/plugins", result);
    }

    [Fact]
    public void Resolve_GlobalConfigAlias_ReturnsGlobalUrl()
    {
        OlavConfig local = new();
        GlobalConfig global = new();
        global.AddSource("globalias", "https://global.example.com/plugins");
        PluginSourceResolver resolver = new(local, global);

        string result = resolver.Resolve("globalias");

        Assert.Equal("https://global.example.com/plugins", result);
    }

    [Fact]
    public void Resolve_LocalAliasOverridesGlobal()
    {
        OlavConfig local = new();
        local.AddSource("shared", "https://local.example.com/plugins");
        GlobalConfig global = new();
        global.AddSource("shared", "https://global.example.com/plugins");
        PluginSourceResolver resolver = new(local, global);

        string result = resolver.Resolve("shared");

        Assert.Equal("https://local.example.com/plugins", result);
    }

    [Fact]
    public void Resolve_BuiltInAzureAlias_ReturnsEmbeddedScheme()
    {
        OlavConfig local = new();
        GlobalConfig global = new();
        PluginSourceResolver resolver = new(local, global);

        string result = resolver.Resolve("azure");

        Assert.Equal("embedded://azure", result);
    }

    [Fact]
    public void Resolve_AliasSlashPluginId_AppendsPluginIdToBaseUrl()
    {
        OlavConfig local = new();
        local.AddSource("myrepo", "https://repo.example.com");
        GlobalConfig global = new();
        PluginSourceResolver resolver = new(local, global);

        string result = resolver.Resolve("myrepo/redis");

        Assert.Equal("https://repo.example.com/redis", result);
    }

    [Fact]
    public void Resolve_UnknownAlias_ThrowsPluginSourceNotFoundException()
    {
        OlavConfig local = new();
        GlobalConfig global = new();
        PluginSourceResolver resolver = new(local, global);

        Olav.Infrastructure.PluginSourceNotFoundException ex =
            Assert.Throws<Olav.Infrastructure.PluginSourceNotFoundException>(
                () => resolver.Resolve("nonexistent-plugin-alias-xyz"));

        Assert.Equal("nonexistent-plugin-alias-xyz", ex.Alias);
        Assert.Contains("nonexistent-plugin-alias-xyz", ex.Message);
    }
}
