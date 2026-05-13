using System;
using System.IO;
using Olav.Extensions;

namespace Olav.IntegrationTests.Generation.Fixtures;

public class GeneratedProjectFixture : IDisposable
{
    public string Root { get; }
    public string ProjectPath { get; }
    public string ProjectName { get; } = "TestProject";
    private readonly string originalDirectory;

    public GeneratedProjectFixture()
    {
        this.originalDirectory = Directory.GetCurrentDirectory();

        Root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(Root);

        Directory.SetCurrentDirectory(Root);

        Olav.Program.Main(["new", ProjectName]);

        ProjectPath = Path.Combine(Root, ProjectName.ToDashCase());
    }

    public void Dispose()
    {
        Directory.SetCurrentDirectory(this.originalDirectory);
        Directory.Delete(Root, true);
    }
}
