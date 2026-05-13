using System;
using System.IO;
using Olav.Generation;
using Xunit;

namespace Olav.UnitTests.Generation;

public class ObservabilityGeneratorTests
{
    [Fact]
    public void Should_Create_Observability_Files()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        string projectName = "TestProject";

        ObservabilityGenerator generator = new(
            projectName,
            root,
            "owner",
            "MIT");

        generator.Generate();

        string observabilityPath = Path.Combine(
            root,
            "src",
            $"{projectName}.Api",
            "Observability");

        Assert.True(Directory.Exists(observabilityPath));

        string correlationFile = Path.Combine(observabilityPath, "CorrelationMiddleware.cs");
        string extensionsFile = Path.Combine(observabilityPath, "ObservabilityExtensions.cs");

        Assert.True(File.Exists(correlationFile));
        Assert.True(File.Exists(extensionsFile));

        Assert.False(string.IsNullOrWhiteSpace(File.ReadAllText(correlationFile)));
        Assert.False(string.IsNullOrWhiteSpace(File.ReadAllText(extensionsFile)));
    }
}
