using System;
using System.IO;
using Olav.Generation;
using Xunit;

namespace Olav.UnitTests.Generation;

public class EntityGeneratorTests
{
    [Fact]
    public void Generate_Creates_Entity_File_At_Correct_Path()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new EntityGenerator("Order", "MyApp", root).Generate();

        string expectedPath = Path.Combine(root, "src", "MyApp.Domain", "Order", "Entities", "Order.cs");
        Assert.True(File.Exists(expectedPath));
    }

    [Fact]
    public void Generate_Entity_File_Contains_Correct_Namespace_And_Class()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new EntityGenerator("Order", "MyApp", root).Generate();

        string content = File.ReadAllText(Path.Combine(root, "src", "MyApp.Domain", "Order", "Entities", "Order.cs"));
        Assert.Contains("namespace MyApp.Domain.Order.Entities;", content);
        Assert.Contains("public class Order", content);
        Assert.Contains("public Guid Id", content);
    }
}
