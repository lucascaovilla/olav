using System.IO;
using Xunit;
using Olav.IntegrationTests.Generation.Fixtures;

namespace Olav.IntegrationTests.Cli;

[Collection("GeneratedProject")]
public class AddArtifactCommand_EndToEndTests(GeneratedProjectFixture fixture)
{
    private readonly GeneratedProjectFixture fixture = fixture;

    [Fact]
    public void AddEntity_Creates_Entity_File_With_Correct_Content()
    {
        string root = this.fixture.ProjectPath;
        string name = this.fixture.ProjectName;

        System.IO.Directory.SetCurrentDirectory(root);
        Olav.Program.Main(["add", "entity", "Order"]);

        string path = Path.Combine(root, "src", $"{name}.Domain", "Order", "Entities", "Order.cs");
        Assert.True(File.Exists(path));

        string content = File.ReadAllText(path);
        Assert.Contains($"namespace {name}.Domain.Order.Entities;", content);
        Assert.Contains("public class Order", content);
        Assert.Contains("public Guid Id", content);
    }

    [Fact]
    public void AddEnum_Creates_Enum_File_With_Correct_Content()
    {
        string root = this.fixture.ProjectPath;
        string name = this.fixture.ProjectName;

        System.IO.Directory.SetCurrentDirectory(root);
        Olav.Program.Main(["add", "enum", "OrderStatus"]);

        string path = Path.Combine(root, "src", $"{name}.Domain", "Shared", "Enums", "OrderStatus.cs");
        Assert.True(File.Exists(path));

        string content = File.ReadAllText(path);
        Assert.Contains($"namespace {name}.Domain.Shared.Enums;", content);
        Assert.Contains("public enum OrderStatus", content);
    }

    [Fact]
    public void AddRepository_Creates_Interface_And_Stub_Implementation()
    {
        string root = this.fixture.ProjectPath;
        string name = this.fixture.ProjectName;

        System.IO.Directory.SetCurrentDirectory(root);
        Olav.Program.Main(["add", "repository", "Product"]);

        string interfacePath = Path.Combine(root, "src", $"{name}.Domain", "Product", "Repositories", "IProductRepository.cs");
        string implPath = Path.Combine(root, "src", $"{name}.Infrastructure", "Repositories", "ProductRepository.cs");

        Assert.True(File.Exists(interfacePath));
        Assert.True(File.Exists(implPath));

        string interfaceContent = File.ReadAllText(interfacePath);
        Assert.Contains($"namespace {name}.Domain.Product.Repositories;", interfaceContent);
        Assert.Contains("public interface IProductRepository", interfaceContent);
        Assert.Contains("GetByIdAsync", interfaceContent);

        string implContent = File.ReadAllText(implPath);
        Assert.Contains($"namespace {name}.Infrastructure.Repositories;", implContent);
        Assert.Contains("public class ProductRepository : IProductRepository", implContent);
        Assert.Contains("NotImplementedException", implContent);
    }

    [Fact]
    public void AddService_Creates_Interface_And_Implementation_Without_Entity()
    {
        string root = this.fixture.ProjectPath;
        string name = this.fixture.ProjectName;

        System.IO.Directory.SetCurrentDirectory(root);
        Olav.Program.Main(["add", "service", "CatalogService"]);

        string interfacePath = Path.Combine(root, "src", $"{name}.Application", "Services", "ICatalogService.cs");
        string implPath = Path.Combine(root, "src", $"{name}.Application", "Services", "CatalogService.cs");

        Assert.True(File.Exists(interfacePath));
        Assert.True(File.Exists(implPath));

        string interfaceContent = File.ReadAllText(interfacePath);
        Assert.Contains($"namespace {name}.Application.Services;", interfaceContent);
        Assert.Contains("public interface ICatalogService", interfaceContent);

        string implContent = File.ReadAllText(implPath);
        Assert.Contains("public class CatalogService : ICatalogService", implContent);
        Assert.DoesNotContain("Repository", implContent);
    }

    [Fact]
    public void AddService_With_Entity_Injects_Repository_Dependency()
    {
        string root = this.fixture.ProjectPath;
        string name = this.fixture.ProjectName;

        System.IO.Directory.SetCurrentDirectory(root);
        Olav.Program.Main(["add", "service", "OrderService", "--entity", "Order"]);

        string implPath = Path.Combine(root, "src", $"{name}.Application", "Services", "OrderService.cs");
        Assert.True(File.Exists(implPath));

        string content = File.ReadAllText(implPath);
        Assert.Contains($"{name}.Domain.Order.Repositories", content);
        Assert.Contains("IOrderRepository", content);
        Assert.Contains("public OrderService(IOrderRepository repository)", content);
    }
}
