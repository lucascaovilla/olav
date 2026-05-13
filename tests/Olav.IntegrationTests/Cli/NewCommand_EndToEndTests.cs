// NewCommand_EndToEndTests.cs — absorbs the deleted generator tests
using System.IO;
using Xunit;
using Olav.IntegrationTests.Generation.Fixtures;

namespace Olav.IntegrationTests.Cli;

[Collection("GeneratedProject")]
public class NewCommand_EndToEndTests(GeneratedProjectFixture fixture)
{
    private readonly GeneratedProjectFixture fixture = fixture;

    [Fact]
    public void Should_Create_Project_Structure()
    {
        string root = this.fixture.ProjectPath;

        Assert.True(Directory.Exists(root));
        Assert.True(Directory.Exists(Path.Combine(root, "src")));
        Assert.True(Directory.Exists(Path.Combine(root, "tests")));
        Assert.True(Directory.Exists(Path.Combine(root, "docker")));
    }

    [Fact]
    public void Should_Create_Solution_File()
    {
        string root = this.fixture.ProjectPath;
        string[] slnFiles = Directory.GetFiles(root, "*.slnx");

        Assert.Single(slnFiles);
    }

    [Fact]
    public void Should_Create_Source_Projects()
    {
        string src = Path.Combine(this.fixture.ProjectPath, "src");
        string name = this.fixture.ProjectName;

        Assert.True(Directory.Exists(Path.Combine(src, $"{name}.Domain")));
        Assert.True(Directory.Exists(Path.Combine(src, $"{name}.Application")));
        Assert.True(Directory.Exists(Path.Combine(src, $"{name}.Infrastructure")));
        Assert.True(Directory.Exists(Path.Combine(src, $"{name}.Api")));
    }

    [Fact]
    public void Should_Create_Test_Projects()
    {
        string tests = Path.Combine(this.fixture.ProjectPath, "tests");
        string name = this.fixture.ProjectName;

        Assert.True(Directory.Exists(Path.Combine(tests, $"{name}.ArchitectureTests")));
        Assert.True(Directory.Exists(Path.Combine(tests, $"{name}.IntegrationTests")));
    }

    [Fact]
    public void Should_Enforce_Coverage_In_Test_Projects()
    {
        string tests = Path.Combine(this.fixture.ProjectPath, "tests");
        string name = this.fixture.ProjectName;

        foreach (string proj in new[] { $"{name}.ArchitectureTests", $"{name}.IntegrationTests" })
        {
            string csproj = Path.Combine(tests, proj, $"{proj}.csproj");
            string content = File.ReadAllText(csproj);

            Assert.Contains("<CollectCoverage>true</CollectCoverage>", content);
            Assert.Contains("<Threshold>100</Threshold>", content);
        }
    }

    [Fact]
    public void Should_Initialize_Git_Repository()
    {
        string root = this.fixture.ProjectPath;

        Assert.True(Directory.Exists(Path.Combine(root, ".git")));
        Assert.True(File.Exists(Path.Combine(root, ".git", "HEAD")));

        string config = File.ReadAllText(Path.Combine(root, ".git", "config"));
        Assert.Contains("hooksPath", config);
        Assert.Contains(".githooks", config);
    }

    [Fact]
    public void Should_Create_OlavJson_At_Root()
    {
        Assert.True(File.Exists(Path.Combine(this.fixture.ProjectPath, "olav.json")));
    }
}
