using System;
using System.IO;
using Olav.Generation;
using Xunit;

namespace Olav.UnitTests.Generation;

public class EfMigrationGeneratorTests
{
    [Fact]
    public void Generate_Throws_When_Plugin_Not_EfCore_Capable()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        var ex = Assert.Throws<InvalidOperationException>(() =>
            new EfMigrationGenerator("redis", "InitialCreate", root).Generate());

        Assert.Contains("redis", ex.Message);
        Assert.Contains("does not support EF Core migrations", ex.Message);
    }

    [Fact]
    public void Generate_Throws_When_Plugin_Not_EfCore_Capable_For_Docker()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        var ex = Assert.Throws<InvalidOperationException>(() =>
            new EfMigrationGenerator("docker", "InitialCreate", root).Generate());

        Assert.Contains("does not support EF Core migrations", ex.Message);
    }

    [Fact]
    public void Generate_Throws_When_Plugin_Not_Installed()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "olav.json"), "{\"toolVersion\":\"0.1.0\"}");

        var ex = Assert.Throws<InvalidOperationException>(() =>
            new EfMigrationGenerator("postgres", "InitialCreate", root).Generate());

        Assert.Contains("not installed", ex.Message);
        Assert.Contains("olav add infrastructure postgres", ex.Message);
    }

    [Fact]
    public void Generate_Throws_When_Namespace_Cannot_Be_Discovered()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(root);

        string olavJson = """
            {
              "toolVersion": "0.1.0",
              "plugins": [
                { "id": "postgres", "version": "0.1.0", "category": "infrastructure", "source": "official", "delivery": "package" }
              ]
            }
            """;
        File.WriteAllText(Path.Combine(root, "olav.json"), olavJson);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            new EfMigrationGenerator("postgres", "InitialCreate", root).Generate());

        Assert.Contains("project namespace", ex.Message);
    }

    [Fact]
    public void Generate_Throws_When_Persistence_Project_Missing()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        string srcDir = Path.Combine(root, "src", "MyApp.Infrastructure");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(Path.Combine(srcDir, "MyApp.Infrastructure.csproj"), "<Project/>");

        string olavJson = """
            {
              "toolVersion": "0.1.0",
              "plugins": [
                { "id": "postgres", "version": "0.1.0", "category": "infrastructure", "source": "official", "delivery": "package" }
              ]
            }
            """;
        File.WriteAllText(Path.Combine(root, "olav.json"), olavJson);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            new EfMigrationGenerator("postgres", "InitialCreate", root).Generate());

        Assert.Contains("Persistence project not found", ex.Message);
    }
}
