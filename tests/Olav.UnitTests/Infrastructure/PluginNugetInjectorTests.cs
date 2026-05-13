using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;
using Olav.Infrastructure;

namespace Olav.UnitTests.Infrastructure;

public class PluginNugetInjectorTests
{
    private static string CreateCsproj(string dir, string content)
    {
        string path = Path.Combine(dir, "My.Infrastructure.csproj");
        File.WriteAllText(path, content);
        return path;
    }

    private const string CsprojWithItemGroup =
        """
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>net10.0</TargetFramework>
          </PropertyGroup>
          <ItemGroup>
            <PackageReference Include="Existing.Package" Version="2.0.0" />
          </ItemGroup>
        </Project>
        """;

    private const string CsprojNoItemGroup =
        """
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>net10.0</TargetFramework>
          </PropertyGroup>
        </Project>
        """;

    [Fact]
    public void AddPackageReference_AddsReferenceToExistingItemGroup()
    {
        string dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        string csproj = CreateCsproj(dir, CsprojWithItemGroup);

        PluginNugetInjector.AddPackageReference(csproj, "My.New.Package", "3.1.0");

        XDocument doc = XDocument.Load(csproj);
        bool found = doc.Descendants("PackageReference")
            .Any(e => e.Attribute("Include")?.Value == "My.New.Package"
                   && e.Attribute("Version")?.Value == "3.1.0");
        Assert.True(found);
    }

    [Fact]
    public void AddPackageReference_CreatesNewItemGroupWhenNoneExists()
    {
        string dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        string csproj = CreateCsproj(dir, CsprojNoItemGroup);

        PluginNugetInjector.AddPackageReference(csproj, "New.Package", "1.0.0");

        XDocument doc = XDocument.Load(csproj);
        bool found = doc.Descendants("PackageReference")
            .Any(e => e.Attribute("Include")?.Value == "New.Package");
        Assert.True(found);
    }

    [Fact]
    public void AddPackageReference_IsIdempotent()
    {
        string dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        string csproj = CreateCsproj(dir, CsprojWithItemGroup);

        PluginNugetInjector.AddPackageReference(csproj, "Idempotent.Package", "1.0.0");
        PluginNugetInjector.AddPackageReference(csproj, "Idempotent.Package", "1.0.0");

        XDocument doc = XDocument.Load(csproj);
        int count = doc.Descendants("PackageReference")
            .Count(e => e.Attribute("Include")?.Value == "Idempotent.Package");
        Assert.Equal(1, count);
    }

    [Fact]
    public void AddPackageReference_MissingFile_ThrowsInvalidOperationException()
    {
        string nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "missing.csproj");

        Assert.Throws<InvalidOperationException>(
            () => PluginNugetInjector.AddPackageReference(nonExistentPath, "Any.Package", "1.0.0"));
    }

    [Fact]
    public void AddPackageSource_CreatesNugetConfigIfMissing()
    {
        string dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        string configPath = Path.Combine(dir, "nuget.config");

        PluginNugetInjector.AddPackageSource(configPath, "my-feed", "https://pkgs.example.com/v3/index.json");

        Assert.True(File.Exists(configPath));
        XDocument doc = XDocument.Load(configPath);
        bool found = doc.Descendants("add")
            .Any(e => e.Attribute("key")?.Value == "my-feed"
                   && e.Attribute("value")?.Value == "https://pkgs.example.com/v3/index.json");
        Assert.True(found);
    }

    [Fact]
    public void AddPackageSource_PreservesNugetOrgWhenCreatingNew()
    {
        string dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        string configPath = Path.Combine(dir, "nuget.config");

        PluginNugetInjector.AddPackageSource(configPath, "my-feed", "https://pkgs.example.com/v3/index.json");

        XDocument doc = XDocument.Load(configPath);
        bool hasNugetOrg = doc.Descendants("add")
            .Any(e => e.Attribute("key")?.Value == "nuget.org");
        Assert.True(hasNugetOrg);
    }

    [Fact]
    public void AddPackageSource_IsIdempotentBySameName()
    {
        string dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        string configPath = Path.Combine(dir, "nuget.config");

        PluginNugetInjector.AddPackageSource(configPath, "my-feed", "https://pkgs.example.com/v3/index.json");
        PluginNugetInjector.AddPackageSource(configPath, "my-feed", "https://pkgs.example.com/v3/index.json");

        XDocument doc = XDocument.Load(configPath);
        int count = doc.Descendants("add")
            .Count(e => e.Attribute("key")?.Value == "my-feed");
        Assert.Equal(1, count);
    }
}
