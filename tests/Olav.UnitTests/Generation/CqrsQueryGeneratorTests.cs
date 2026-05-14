using System;
using System.IO;
using Olav.Generation;
using Xunit;

namespace Olav.UnitTests.Generation;

public class CqrsQueryGeneratorTests
{
    [Fact]
    public void Generate_Creates_Query_And_Result_Files()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new CqrsQueryGenerator("User", "GetUser", "MyApp", root, withHandler: false, withEndpoint: false).Generate();

        Assert.True(File.Exists(Path.Combine(root, "src", "MyApp.Application", "User", "Queries", "GetUser", "GetUserQuery.cs")));
        Assert.True(File.Exists(Path.Combine(root, "src", "MyApp.Application", "User", "Queries", "GetUser", "GetUserQueryResult.cs")));
    }

    [Fact]
    public void Generate_Does_Not_Create_Handler_Without_Flag()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new CqrsQueryGenerator("User", "GetUser", "MyApp", root, withHandler: false, withEndpoint: false).Generate();

        Assert.False(File.Exists(Path.Combine(root, "src", "MyApp.Application", "User", "Queries", "GetUser", "IGetUserQueryHandler.cs")));
        Assert.False(File.Exists(Path.Combine(root, "src", "MyApp.Application", "User", "Queries", "GetUser", "GetUserQueryHandler.cs")));
    }

    [Fact]
    public void Generate_Query_Record_Contains_Correct_Namespace()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new CqrsQueryGenerator("User", "GetUser", "MyApp", root, withHandler: false, withEndpoint: false).Generate();

        string content = File.ReadAllText(Path.Combine(root, "src", "MyApp.Application", "User", "Queries", "GetUser", "GetUserQuery.cs"));
        Assert.Contains("namespace MyApp.Application.User.Queries.GetUser;", content);
        Assert.Contains("public sealed record GetUserQuery", content);
        Assert.Contains("Guid Id", content);
    }

    [Fact]
    public void Generate_WithHandler_Creates_All_Four_Files()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new CqrsQueryGenerator("User", "GetUser", "MyApp", root, withHandler: true, withEndpoint: false).Generate();

        string dir = Path.Combine(root, "src", "MyApp.Application", "User", "Queries", "GetUser");
        Assert.True(File.Exists(Path.Combine(dir, "GetUserQuery.cs")));
        Assert.True(File.Exists(Path.Combine(dir, "GetUserQueryResult.cs")));
        Assert.True(File.Exists(Path.Combine(dir, "IGetUserQueryHandler.cs")));
        Assert.True(File.Exists(Path.Combine(dir, "GetUserQueryHandler.cs")));
    }

    [Fact]
    public void Generate_WithHandler_Interface_Has_Nullable_Result()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new CqrsQueryGenerator("User", "GetUser", "MyApp", root, withHandler: true, withEndpoint: false).Generate();

        string content = File.ReadAllText(Path.Combine(root, "src", "MyApp.Application", "User", "Queries", "GetUser", "IGetUserQueryHandler.cs"));
        Assert.Contains("Task<GetUserQueryResult?>", content);
    }

    [Fact]
    public void Generate_WithHandler_Registers_In_DI()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new CqrsQueryGenerator("User", "GetUser", "MyApp", root, withHandler: true, withEndpoint: false).Generate();

        string diPath = Path.Combine(root, "src", "MyApp.Application", "DependencyInjection.cs");
        Assert.True(File.Exists(diPath));
        string content = File.ReadAllText(diPath);
        Assert.Contains("IGetUserQueryHandler", content);
        Assert.Contains("GetUserQueryHandler", content);
        Assert.Contains("AddScoped", content);
    }

    [Fact]
    public void Generate_WithHandler_DI_Accumulates_Multiple_Handlers()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new CqrsQueryGenerator("User", "GetUser", "MyApp", root, withHandler: true, withEndpoint: false).Generate();
        new CqrsQueryGenerator("User", "ListUsers", "MyApp", root, withHandler: true, withEndpoint: false).Generate();

        string diPath = Path.Combine(root, "src", "MyApp.Application", "DependencyInjection.cs");
        string content = File.ReadAllText(diPath);
        Assert.Contains("IGetUserQueryHandler", content);
        Assert.Contains("IListUsersQueryHandler", content);
    }

    [Fact]
    public void Generate_WithEndpoint_Creates_Controller_With_Get_Action()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new CqrsQueryGenerator("User", "GetUser", "MyApp", root, withHandler: false, withEndpoint: true).Generate();

        string controllerPath = Path.Combine(root, "src", "MyApp.Api", "Controllers", "UserController.cs");
        Assert.True(File.Exists(controllerPath));
        string content = File.ReadAllText(controllerPath);
        Assert.Contains("UserController", content);
        Assert.Contains("[HttpGet", content);
        Assert.Contains("GetUser", content);
    }
}
