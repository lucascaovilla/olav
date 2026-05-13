using System;
using System.IO;
using Olav.Generation;
using Xunit;

namespace Olav.UnitTests.Generation;

public class DomainEnumGeneratorTests
{
    [Fact]
    public void Generate_Creates_Enum_File_At_Correct_Path()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new DomainEnumGenerator("Order", "OrderStatus", "MyApp", root).Generate();

        string expectedPath = Path.Combine(root, "src", "MyApp.Domain", "Order", "Enums", "OrderStatus.cs");
        Assert.True(File.Exists(expectedPath));
    }

    [Fact]
    public void Generate_Enum_File_Contains_Correct_Namespace_And_Type()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new DomainEnumGenerator("Order", "OrderStatus", "MyApp", root).Generate();

        string content = File.ReadAllText(Path.Combine(root, "src", "MyApp.Domain", "Order", "Enums", "OrderStatus.cs"));
        Assert.Contains("namespace MyApp.Domain.Order.Enums;", content);
        Assert.Contains("public enum OrderStatus", content);
    }
}
