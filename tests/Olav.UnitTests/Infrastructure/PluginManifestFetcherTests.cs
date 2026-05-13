using System;
using System.IO;
using System.Text.Json;
using Xunit;
using Olav.Infrastructure;

namespace Olav.UnitTests.Infrastructure;

public class PluginManifestFetcherTests
{
    private static string WriteManifest(string dir, string json)
    {
        string path = Path.Combine(dir, "olav.plugin.json");
        File.WriteAllText(path, json);
        return path;
    }

    private static string MinimalManifest(string id = "test-plugin") => JsonSerializer.Serialize(new
    {
        id,
        version = "1.0.0",
        displayName = "Test Plugin",
        category = "infrastructure",
        delivery = "generation",
        generates = Array.Empty<object>(),
        parameters = Array.Empty<object>(),
        conflicts = Array.Empty<string>(),
        requires = Array.Empty<string>(),
    });

    [Fact]
    public void Fetch_LocalDirectory_LoadsManifest()
    {
        string dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        WriteManifest(dir, MinimalManifest("my-plugin"));

        OlavPluginManifest manifest = PluginManifestFetcher.Fetch(dir);

        Assert.Equal("my-plugin", manifest.Id);
        Assert.Equal("1.0.0", manifest.Version);
        Assert.Equal("infrastructure", manifest.Category);
    }

    [Fact]
    public void Fetch_DirectManifestPath_LoadsManifest()
    {
        string dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        string manifestPath = WriteManifest(dir, MinimalManifest("direct-plugin"));

        OlavPluginManifest manifest = PluginManifestFetcher.Fetch(manifestPath);

        Assert.Equal("direct-plugin", manifest.Id);
    }

    [Fact]
    public void Fetch_MissingManifest_ThrowsInvalidOperationException()
    {
        string dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
            () => PluginManifestFetcher.Fetch(dir));

        Assert.Contains("olav.plugin.json", ex.Message);
    }

    [Fact]
    public void Fetch_InvalidJson_ThrowsInvalidOperationException()
    {
        string dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "olav.plugin.json"), "not-valid-json{{{");

        Assert.Throws<InvalidOperationException>(
            () => PluginManifestFetcher.Fetch(dir));
    }

    [Fact]
    public void Fetch_EmbeddedAzure_ReturnsAzureManifest()
    {
        OlavPluginManifest manifest = PluginManifestFetcher.Fetch("embedded://azure");

        Assert.Equal("azure", manifest.Id);
        Assert.Equal("deployment", manifest.Category);
        Assert.Equal("generation", manifest.Delivery);
    }
}
