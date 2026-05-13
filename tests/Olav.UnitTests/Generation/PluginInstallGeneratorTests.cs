#nullable enable
using System;
using System.IO;
using System.Text.Json;
using Xunit;
using Olav.Infrastructure;
using Olav.Generation;

namespace Olav.UnitTests.Generation;

public class PluginInstallGeneratorTests
{
    private static string CreateTempProject()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(root);

        string srcDir = Path.Combine(root, "src", "MyApp.Infrastructure");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(
            Path.Combine(srcDir, "MyApp.Infrastructure.csproj"),
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
              <ItemGroup>
                <PackageReference Include="SomePackage" Version="1.0.0" />
              </ItemGroup>
            </Project>
            """);

        return root;
    }

    private static string CreatePluginDir(
        string id,
        string category = "infrastructure",
        string delivery = "generation",
        string[]? requires = null,
        string[]? conflicts = null,
        string? templateFileName = null,
        string? templateContent = null,
        string? outputPath = null)
    {
        string pluginDir = Path.Combine(Path.GetTempPath(), "plugin-" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(pluginDir);

        object generateEntry = new { template = templateFileName ?? string.Empty, output = outputPath ?? string.Empty };
        object[] generates = (templateFileName != null)
            ? new object[] { generateEntry }
            : Array.Empty<object>();

        string manifest = JsonSerializer.Serialize(new
        {
            id,
            version = "1.0.0",
            displayName = id + " Plugin",
            category,
            delivery,
            generates,
            parameters = Array.Empty<object>(),
            conflicts = conflicts ?? Array.Empty<string>(),
            requires = requires ?? Array.Empty<string>(),
        });

        File.WriteAllText(Path.Combine(pluginDir, "olav.plugin.json"), manifest);

        if (templateFileName != null && templateContent != null)
        {
            string templatesDir = Path.Combine(pluginDir, "templates");
            Directory.CreateDirectory(templatesDir);
            File.WriteAllText(Path.Combine(templatesDir, templateFileName), templateContent);
        }

        return pluginDir;
    }

    private static PluginInstallGenerator BuildGenerator(string root)
    {
        OlavConfig localConfig = OlavConfig.Load(root);
        GlobalConfig globalConfig = new();
        PluginSourceResolver resolver = new(localConfig, globalConfig);
        return new PluginInstallGenerator(root, resolver);
    }

    [Fact]
    public void Install_AlreadyInstalled_ThrowsInvalidOperationException()
    {
        string root = CreateTempProject();
        OlavConfig config = new();
        config.AddPlugin(new PluginEntry { Id = "my-plugin", Version = "1.0.0", Category = "infrastructure", Source = "test", Delivery = "generation" });
        config.Save(root);

        string pluginDir = CreatePluginDir("my-plugin");
        PluginInstallGenerator generator = BuildGenerator(root);

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
            () => generator.Install(pluginDir, new()));

        Assert.Contains("already installed", ex.Message);
    }

    [Fact]
    public void Install_ConflictingPluginPresent_ThrowsInvalidOperationException()
    {
        string root = CreateTempProject();
        OlavConfig config = new();
        config.AddPlugin(new PluginEntry { Id = "postgres", Version = "1.0.0", Category = "infrastructure", Source = "test", Delivery = "package" });
        config.Save(root);

        string pluginDir = CreatePluginDir("sqlserver", conflicts: new[] { "postgres" });
        PluginInstallGenerator generator = BuildGenerator(root);

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
            () => generator.Install(pluginDir, new()));

        Assert.Contains("conflicts with", ex.Message);
        Assert.Contains("postgres", ex.Message);
    }

    [Fact]
    public void Install_RequiredPluginMissing_ThrowsInvalidOperationException()
    {
        string root = CreateTempProject();

        string pluginDir = CreatePluginDir("my-plugin", requires: new[] { "base-plugin" });
        PluginInstallGenerator generator = BuildGenerator(root);

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
            () => generator.Install(pluginDir, new()));

        Assert.Contains("requires", ex.Message);
        Assert.Contains("base-plugin", ex.Message);
    }

    [Fact]
    public void Install_GenerationDelivery_CreatesOutputFile()
    {
        string root = CreateTempProject();
        string pluginDir = CreatePluginDir(
            "gen-plugin",
            templateFileName: "hello.txt",
            templateContent: "Hello from {{projectName}}!",
            outputPath: "output/hello.txt");

        PluginInstallGenerator generator = BuildGenerator(root);
        generator.Install(pluginDir, new() { { "projectName", "TestProject" } });

        string outputFile = Path.Combine(root, "output", "hello.txt");
        Assert.True(File.Exists(outputFile));
        Assert.Equal("Hello from TestProject!", File.ReadAllText(outputFile));
    }

    [Fact]
    public void Install_GenerationDelivery_SkipsExistingFile()
    {
        string root = CreateTempProject();
        string pluginDir = CreatePluginDir(
            "gen-plugin2",
            templateFileName: "existing.txt",
            templateContent: "new content",
            outputPath: "output/existing.txt");

        string outputFile = Path.Combine(root, "output", "existing.txt");
        Directory.CreateDirectory(Path.GetDirectoryName(outputFile)!);
        File.WriteAllText(outputFile, "original content");

        PluginInstallGenerator generator = BuildGenerator(root);
        generator.Install(pluginDir, new());

        Assert.Equal("original content", File.ReadAllText(outputFile));
    }

    [Fact]
    public void Install_RecordsPluginInOlavJson()
    {
        string root = CreateTempProject();
        string pluginDir = CreatePluginDir("record-plugin", category: "deployment");

        PluginInstallGenerator generator = BuildGenerator(root);
        generator.Install(pluginDir, new());

        OlavConfig config = OlavConfig.Load(root);
        PluginEntry? entry = config.Plugins?.Find(p => p.Id == "record-plugin");
        Assert.NotNull(entry);
        Assert.Equal("1.0.0", entry!.Version);
        Assert.Equal("deployment", entry.Category);
    }

    [Fact]
    public void Install_TemplateRendering_ReplacesDoubleBraceVariants()
    {
        string root = CreateTempProject();
        string pluginDir = CreatePluginDir(
            "render-plugin",
            templateFileName: "render.txt",
            templateContent: "A={{ appName }} B={{appName}}",
            outputPath: "render.txt");

        PluginInstallGenerator generator = BuildGenerator(root);
        generator.Install(pluginDir, new() { { "appName", "MyApp" } });

        string content = File.ReadAllText(Path.Combine(root, "render.txt"));
        Assert.Equal("A=MyApp B=MyApp", content);
    }
}
