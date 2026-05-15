using System;
using System.IO;
using Olav.Generation;
using Xunit;

namespace Olav.UnitTests.Generation;

public class DomainEnumGeneratorTests
{
    [Fact]
    public void Generate_Creates_Shared_Enum_File_When_No_Entity()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new DomainEnumGenerator("OrderStatus", "MyApp", root).Generate();

        string expectedPath = Path.Combine(root, "src", "MyApp.Domain", "Shared", "Enums", "OrderStatus.cs");
        Assert.True(File.Exists(expectedPath));
    }

    [Fact]
    public void Generate_Shared_Enum_Contains_Shared_Namespace()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new DomainEnumGenerator("OrderStatus", "MyApp", root).Generate();

        string content = File.ReadAllText(Path.Combine(root, "src", "MyApp.Domain", "Shared", "Enums", "OrderStatus.cs"));
        Assert.Contains("namespace MyApp.Domain.Shared.Enums;", content);
        Assert.Contains("public enum OrderStatus", content);
    }

    [Fact]
    public void Generate_Creates_Entity_Scoped_Enum_File_When_Entity_Provided()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new DomainEnumGenerator("OrderStatus", "MyApp", root, "Order").Generate();

        string expectedPath = Path.Combine(root, "src", "MyApp.Domain", "Order", "Enums", "OrderStatus.cs");
        Assert.True(File.Exists(expectedPath));
    }

    [Fact]
    public void Generate_Entity_Scoped_Enum_Contains_Entity_Namespace()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new DomainEnumGenerator("OrderStatus", "MyApp", root, "Order").Generate();

        string content = File.ReadAllText(Path.Combine(root, "src", "MyApp.Domain", "Order", "Enums", "OrderStatus.cs"));
        Assert.Contains("namespace MyApp.Domain.Order.Enums;", content);
        Assert.Contains("public enum OrderStatus", content);
    }
}
