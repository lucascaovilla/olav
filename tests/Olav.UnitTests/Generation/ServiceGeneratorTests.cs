using System;
using System.IO;
using Olav.Generation;
using Xunit;

namespace Olav.UnitTests.Generation;

public class ServiceGeneratorTests
{
    [Fact]
    public void Generate_Creates_Interface_And_Implementation_Files()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new ServiceGenerator("Order", "OrderService", "MyApp", root).Generate();

        Assert.True(File.Exists(Path.Combine(root, "src", "MyApp.Application", "Services", "IOrderService.cs")));
        Assert.True(File.Exists(Path.Combine(root, "src", "MyApp.Application", "Services", "OrderService.cs")));
    }

    [Fact]
    public void Generate_Interface_Contains_Correct_Namespace()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new ServiceGenerator("Order", "OrderService", "MyApp", root).Generate();

        string content = File.ReadAllText(Path.Combine(root, "src", "MyApp.Application", "Services", "IOrderService.cs"));
        Assert.Contains("namespace MyApp.Application.Services;", content);
        Assert.Contains("public interface IOrderService", content);
    }

    [Fact]
    public void Generate_Implementation_Contains_Repository_Dependency()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new ServiceGenerator("Order", "OrderService", "MyApp", root).Generate();

        string content = File.ReadAllText(Path.Combine(root, "src", "MyApp.Application", "Services", "OrderService.cs"));
        Assert.Contains("namespace MyApp.Application.Services;", content);
        Assert.Contains("MyApp.Domain.Order.Repositories", content);
        Assert.Contains("IOrderRepository", content);
        Assert.Contains("public OrderService(IOrderRepository repository)", content);
    }

    [Fact]
    public void Generate_Creates_DI_Registration_File()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new ServiceGenerator("Order", "OrderService", "MyApp", root).Generate();

        string diPath = Path.Combine(root, "src", "MyApp.Application", "DependencyInjection.cs");
        Assert.True(File.Exists(diPath));
        string content = File.ReadAllText(diPath);
        Assert.Contains("AddScoped", content);
        Assert.Contains("IOrderService", content);
        Assert.Contains("OrderService", content);
    }

    [Fact]
    public void Generate_Appends_DI_Registration_When_File_Already_Exists()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new ServiceGenerator("Order", "OrderService", "MyApp", root).Generate();
        new ServiceGenerator("Product", "ProductService", "MyApp", root).Generate();

        string diPath = Path.Combine(root, "src", "MyApp.Application", "DependencyInjection.cs");
        string content = File.ReadAllText(diPath);
        Assert.Contains("IOrderService", content);
        Assert.Contains("IProductService", content);
    }

    [Fact]
    public void Generate_Infrastructure_Creates_Interface_In_Application_And_Impl_In_Infrastructure()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new ServiceGenerator("Email", "EmailService", "MyApp", root, ServiceGenerator.ServiceLayer.Infrastructure).Generate();

        Assert.True(File.Exists(Path.Combine(root, "src", "MyApp.Application", "Services", "IEmailService.cs")));
        Assert.True(File.Exists(Path.Combine(root, "src", "MyApp.Infrastructure", "Services", "EmailService.cs")));
    }

    [Fact]
    public void Generate_Infrastructure_Interface_Has_Application_Namespace()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new ServiceGenerator("Email", "EmailService", "MyApp", root, ServiceGenerator.ServiceLayer.Infrastructure).Generate();

        string content = File.ReadAllText(Path.Combine(root, "src", "MyApp.Application", "Services", "IEmailService.cs"));
        Assert.Contains("namespace MyApp.Application.Services;", content);
        Assert.Contains("public interface IEmailService", content);
    }

    [Fact]
    public void Generate_Infrastructure_Impl_Has_Infrastructure_Namespace_And_Implements_Interface()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new ServiceGenerator("Email", "EmailService", "MyApp", root, ServiceGenerator.ServiceLayer.Infrastructure).Generate();

        string content = File.ReadAllText(Path.Combine(root, "src", "MyApp.Infrastructure", "Services", "EmailService.cs"));
        Assert.Contains("namespace MyApp.Infrastructure.Services;", content);
        Assert.Contains("using MyApp.Application.Services;", content);
        Assert.Contains("public class EmailService : IEmailService", content);
    }

    [Fact]
    public void Generate_Infrastructure_Creates_DI_Registration_In_Infrastructure()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new ServiceGenerator("Email", "EmailService", "MyApp", root, ServiceGenerator.ServiceLayer.Infrastructure).Generate();

        string diPath = Path.Combine(root, "src", "MyApp.Infrastructure", "DependencyInjection.cs");
        Assert.True(File.Exists(diPath));
        string content = File.ReadAllText(diPath);
        Assert.Contains("MyApp.Application.Services.IEmailService", content);
        Assert.Contains("MyApp.Infrastructure.Services.EmailService", content);
    }

    [Fact]
    public void Generate_Infrastructure_Appends_DI_When_File_Already_Exists()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new ServiceGenerator("Email", "EmailService", "MyApp", root, ServiceGenerator.ServiceLayer.Infrastructure).Generate();
        new ServiceGenerator("Sms", "SmsService", "MyApp", root, ServiceGenerator.ServiceLayer.Infrastructure).Generate();

        string diPath = Path.Combine(root, "src", "MyApp.Infrastructure", "DependencyInjection.cs");
        string content = File.ReadAllText(diPath);
        Assert.Contains("IEmailService", content);
        Assert.Contains("ISmsService", content);
    }

    private static string CreateMinimalProgramCs(string root, string projectName)
    {
        string programPath = Path.Combine(root, "src", $"{projectName}.Api", "Program.cs");
        Directory.CreateDirectory(Path.GetDirectoryName(programPath)!);
        File.WriteAllText(programPath, $$"""
            namespace {{projectName}}.Api;
            using Microsoft.AspNetCore.Builder;
            public static class Program
            {
                public static void Main(string[] args)
                {
                    var builder = WebApplication.CreateBuilder(args);
                    WebApplication app = builder.Build();
                    app.Run();
                }
            }
            """);
        return programPath;
    }

    [Fact]
    public void Generate_Application_Service_Wires_AddApplication_Into_Program()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        string programPath = CreateMinimalProgramCs(root, "MyApp");

        new ServiceGenerator("Order", "OrderService", "MyApp", root).Generate();

        string content = File.ReadAllText(programPath);
        Assert.Contains("builder.Services.AddApplication();", content);
    }

    [Fact]
    public void Generate_Infrastructure_Service_Wires_AddInfrastructure_Into_Program()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        string programPath = CreateMinimalProgramCs(root, "MyApp");

        new ServiceGenerator("Email", "EmailService", "MyApp", root, ServiceGenerator.ServiceLayer.Infrastructure).Generate();

        string content = File.ReadAllText(programPath);
        Assert.Contains("builder.Services.AddInfrastructure();", content);
    }
}
