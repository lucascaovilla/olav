// <copyright file="PluginScaffoldGeneratorTests.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
#nullable enable
namespace Olav.UnitTests.Generation;

using System;
using System.IO;
using System.Text.Json;
using Olav.Generation;
using Xunit;

/// <summary>
/// Unit tests for <see cref="PluginScaffoldGenerator"/>.
/// </summary>
public class PluginScaffoldGeneratorTests
{
    [Fact]
    public void Generate_CreatesManifestFile()
    {
        string rootDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(rootDir);

        PluginScaffoldGenerator generator = new PluginScaffoldGenerator(
            "my-plugin", rootDir, "infrastructure", "package", "test-author");

        generator.Generate();

        string manifestPath = Path.Combine(rootDir, "my-plugin", "olav.plugin.json");
        Assert.True(File.Exists(manifestPath), "Expected olav.plugin.json to be created.");
    }

    [Fact]
    public void Generate_CreatesStarterTemplate()
    {
        string rootDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(rootDir);

        PluginScaffoldGenerator generator = new PluginScaffoldGenerator(
            "my-plugin", rootDir, "infrastructure", "package", "test-author");

        generator.Generate();

        string templatePath = Path.Combine(rootDir, "my-plugin", "templates", "my-plugin-wiring.cs.sbn");
        Assert.True(File.Exists(templatePath), "Expected starter template to be created.");
    }

    [Fact]
    public void Generate_ManifestContainsCorrectId()
    {
        string rootDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(rootDir);

        PluginScaffoldGenerator generator = new PluginScaffoldGenerator(
            "my-plugin", rootDir, "infrastructure", "package", "test-author");

        generator.Generate();

        string manifestJson = File.ReadAllText(Path.Combine(rootDir, "my-plugin", "olav.plugin.json"));
        using JsonDocument doc = JsonDocument.Parse(manifestJson);
        string id = doc.RootElement.GetProperty("id").GetString() ?? string.Empty;

        Assert.Equal("my-plugin", id);
    }

    [Fact]
    public void Generate_ManifestContainsCorrectCategoryAndDelivery()
    {
        string rootDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(rootDir);

        PluginScaffoldGenerator generator = new PluginScaffoldGenerator(
            "my-plugin", rootDir, "deployment", "generation", "test-author");

        generator.Generate();

        string manifestJson = File.ReadAllText(Path.Combine(rootDir, "my-plugin", "olav.plugin.json"));
        using JsonDocument doc = JsonDocument.Parse(manifestJson);
        string category = doc.RootElement.GetProperty("category").GetString() ?? string.Empty;
        string delivery = doc.RootElement.GetProperty("delivery").GetString() ?? string.Empty;

        Assert.Equal("deployment", category);
        Assert.Equal("generation", delivery);
    }

    [Fact]
    public void Generate_ManifestContainsAuthor()
    {
        string rootDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(rootDir);

        PluginScaffoldGenerator generator = new PluginScaffoldGenerator(
            "my-plugin", rootDir, "infrastructure", "package", "mycompany");

        generator.Generate();

        string manifestJson = File.ReadAllText(Path.Combine(rootDir, "my-plugin", "olav.plugin.json"));
        using JsonDocument doc = JsonDocument.Parse(manifestJson);
        string author = doc.RootElement.GetProperty("author").GetString() ?? string.Empty;

        Assert.Equal("mycompany", author);
    }

    [Fact]
    public void Generate_WhenDirectoryAlreadyExists_ThrowsInvalidOperationException()
    {
        string rootDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        string pluginDir = Path.Combine(rootDir, "my-plugin");
        Directory.CreateDirectory(pluginDir);

        PluginScaffoldGenerator generator = new PluginScaffoldGenerator(
            "my-plugin", rootDir, "infrastructure", "package", "test-author");

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
            () => generator.Generate());

        Assert.Contains("already exists", ex.Message);
    }

    [Fact]
    public void Generate_ManifestIsValidJson()
    {
        string rootDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(rootDir);

        PluginScaffoldGenerator generator = new PluginScaffoldGenerator(
            "test-plugin", rootDir, "infrastructure", "package", "me");

        generator.Generate();

        string manifestJson = File.ReadAllText(Path.Combine(rootDir, "test-plugin", "olav.plugin.json"));
        Exception? ex = Record.Exception(() => JsonDocument.Parse(manifestJson));

        Assert.Null(ex);
    }

    [Fact]
    public void Generate_ManifestGeneratesArrayContainsStarterTemplate()
    {
        string rootDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(rootDir);

        PluginScaffoldGenerator generator = new PluginScaffoldGenerator(
            "my-plugin", rootDir, "infrastructure", "package", "test-author");

        generator.Generate();

        string manifestJson = File.ReadAllText(Path.Combine(rootDir, "my-plugin", "olav.plugin.json"));
        using JsonDocument doc = JsonDocument.Parse(manifestJson);
        JsonElement generates = doc.RootElement.GetProperty("generates");

        Assert.True(generates.GetArrayLength() > 0, "Expected at least one generates entry.");
        string templateName = generates[0].GetProperty("template").GetString() ?? string.Empty;
        Assert.Equal("my-plugin-wiring.cs.sbn", templateName);
    }
}
