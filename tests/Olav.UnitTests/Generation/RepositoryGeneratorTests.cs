using System;
using System.IO;
using Olav.Generation;
using Xunit;

namespace Olav.UnitTests.Generation;

public class RepositoryGeneratorTests
{
    [Fact]
    public void Generate_Creates_Interface_And_Implementation_Files()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new RepositoryGenerator("Order", "MyApp", root, null).Generate();

        Assert.True(File.Exists(Path.Combine(root, "src", "MyApp.Domain", "Order", "Repositories", "IOrderRepository.cs")));
        Assert.True(File.Exists(Path.Combine(root, "src", "MyApp.Infrastructure", "Repositories", "Order", "OrderRepository.cs")));
    }

    [Fact]
    public void Generate_Interface_Contains_Correct_Namespace_And_Methods()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new RepositoryGenerator("Order", "MyApp", root, null).Generate();

        string content = File.ReadAllText(Path.Combine(root, "src", "MyApp.Domain", "Order", "Repositories", "IOrderRepository.cs"));
        Assert.Contains("namespace MyApp.Domain.Order.Repositories;", content);
        Assert.Contains("public interface IOrderRepository", content);
        Assert.Contains("GetByIdAsync", content);
        Assert.Contains("GetAllAsync", content);
        Assert.Contains("AddAsync", content);
        Assert.Contains("UpdateAsync", content);
        Assert.Contains("DeleteAsync", content);
    }

    [Fact]
    public void Generate_Stub_Implementation_Contains_NotImplementedException()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new RepositoryGenerator("Order", "MyApp", root, null).Generate();

        string content = File.ReadAllText(Path.Combine(root, "src", "MyApp.Infrastructure", "Repositories", "Order", "OrderRepository.cs"));
        Assert.Contains("namespace MyApp.Infrastructure.Repositories.Order;", content);
        Assert.Contains("public class OrderRepository : IOrderRepository", content);
        Assert.Contains("NotImplementedException", content);
    }

    [Fact]
    public void Generate_EfCore_Implementation_When_Plugin_Is_Postgres()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new RepositoryGenerator("Order", "MyApp", root, "postgres").Generate();

        string implPath = Path.Combine(root, "src", "MyApp.Infrastructure", "Persistence", "Postgres", "Repositories", "Order", "OrderRepository.cs");
        Assert.True(File.Exists(implPath));
        string content = File.ReadAllText(implPath);
        Assert.Contains("Microsoft.EntityFrameworkCore", content);
        Assert.Contains("MyAppDbContext", content);
        Assert.Contains("SaveChangesAsync", content);
    }

    [Fact]
    public void Generate_EfCore_Implementation_When_Plugin_Is_Sqlserver()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new RepositoryGenerator("Order", "MyApp", root, "sqlserver").Generate();

        string implPath = Path.Combine(root, "src", "MyApp.Infrastructure", "Persistence", "SqlServer", "Repositories", "Order", "OrderRepository.cs");
        Assert.True(File.Exists(implPath));
        string content = File.ReadAllText(implPath);
        Assert.Contains("Microsoft.EntityFrameworkCore", content);
    }

    [Fact]
    public void Generate_Redis_Implementation_When_Plugin_Is_Redis()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new RepositoryGenerator("Order", "MyApp", root, "redis").Generate();

        string implPath = Path.Combine(root, "src", "MyApp.Infrastructure", "Persistence", "Redis", "Repositories", "Order", "OrderRepository.cs");
        Assert.True(File.Exists(implPath));
        string content = File.ReadAllText(implPath);
        Assert.Contains("StackExchange.Redis", content);
        Assert.Contains("IConnectionMultiplexer", content);
    }

    [Fact]
    public void Generate_Creates_DI_Registration_File()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new RepositoryGenerator("Order", "MyApp", root, null).Generate();

        string diPath = Path.Combine(root, "src", "MyApp.Infrastructure", "DependencyInjection.cs");
        Assert.True(File.Exists(diPath));
        string content = File.ReadAllText(diPath);
        Assert.Contains("AddScoped", content);
        Assert.Contains("IOrderRepository", content);
        Assert.Contains("OrderRepository", content);
    }

    [Fact]
    public void Generate_Appends_DI_Registration_When_File_Already_Exists()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new RepositoryGenerator("Order", "MyApp", root, null).Generate();
        new RepositoryGenerator("Product", "MyApp", root, null).Generate();

        string diPath = Path.Combine(root, "src", "MyApp.Infrastructure", "DependencyInjection.cs");
        string content = File.ReadAllText(diPath);
        Assert.Contains("IOrderRepository", content);
        Assert.Contains("IProductRepository", content);
    }

    [Fact]
    public void Generate_WithPostgresPlugin_CreatesImplInPersistencePostgresPath()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new RepositoryGenerator("Order", "MyApp", root, "postgres").Generate();

        Assert.True(File.Exists(Path.Combine(root, "src", "MyApp.Infrastructure", "Persistence", "Postgres", "Repositories", "Order", "OrderRepository.cs")));
        Assert.False(File.Exists(Path.Combine(root, "src", "MyApp.Infrastructure", "Repositories", "Order", "OrderRepository.cs")));
    }

    [Fact]
    public void Generate_WithPostgresPlugin_UsesCorrectNamespace()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new RepositoryGenerator("Order", "MyApp", root, "postgres").Generate();

        string content = File.ReadAllText(Path.Combine(root, "src", "MyApp.Infrastructure", "Persistence", "Postgres", "Repositories", "Order", "OrderRepository.cs"));
        Assert.Contains("namespace MyApp.Infrastructure.Persistence.Postgres.Repositories.Order;", content);
    }

    [Fact]
    public void Generate_WithPostgresPlugin_RegistersDiInPersistenceProject()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new RepositoryGenerator("Order", "MyApp", root, "postgres").Generate();

        string diPath = Path.Combine(root, "src", "MyApp.Infrastructure", "Persistence", "Postgres", "DependencyInjection.cs");
        Assert.True(File.Exists(diPath));
        string content = File.ReadAllText(diPath);
        Assert.Contains("MyApp.Domain.Order.Repositories.IOrderRepository", content);
        Assert.Contains("MyApp.Infrastructure.Persistence.Postgres.Repositories.Order.OrderRepository", content);
        Assert.False(File.Exists(Path.Combine(root, "src", "MyApp.Infrastructure", "DependencyInjection.cs")));
    }

    [Fact]
    public void Generate_WithPostgresPlugin_AppendsDiWhenFileAlreadyExists()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new RepositoryGenerator("Order", "MyApp", root, "postgres").Generate();
        new RepositoryGenerator("Product", "MyApp", root, "postgres").Generate();

        string diPath = Path.Combine(root, "src", "MyApp.Infrastructure", "Persistence", "Postgres", "DependencyInjection.cs");
        string content = File.ReadAllText(diPath);
        Assert.Contains("IOrderRepository", content);
        Assert.Contains("IProductRepository", content);
    }
}
