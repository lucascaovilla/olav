using System;
using System.IO;
using Olav.Generation;
using Xunit;

namespace Olav.UnitTests.Generation;

public class CqrsCommandGeneratorTests
{
    [Fact]
    public void Generate_Creates_Command_And_Result_Files()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new CqrsCommandGenerator("User", "Login", "MyApp", root, withHandler: false, withEndpoint: false).Generate();

        Assert.True(File.Exists(Path.Combine(root, "src", "MyApp.Application", "User", "Commands", "Login", "LoginCommand.cs")));
        Assert.True(File.Exists(Path.Combine(root, "src", "MyApp.Application", "User", "Commands", "Login", "LoginCommandResult.cs")));
    }

    [Fact]
    public void Generate_Does_Not_Create_Handler_Without_Flag()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new CqrsCommandGenerator("User", "Login", "MyApp", root, withHandler: false, withEndpoint: false).Generate();

        Assert.False(File.Exists(Path.Combine(root, "src", "MyApp.Application", "User", "Commands", "Login", "ILoginCommandHandler.cs")));
        Assert.False(File.Exists(Path.Combine(root, "src", "MyApp.Application", "User", "Commands", "Login", "LoginCommandHandler.cs")));
    }

    [Fact]
    public void Generate_Command_Record_Contains_Correct_Namespace()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new CqrsCommandGenerator("User", "Login", "MyApp", root, withHandler: false, withEndpoint: false).Generate();

        string content = File.ReadAllText(Path.Combine(root, "src", "MyApp.Application", "User", "Commands", "Login", "LoginCommand.cs"));
        Assert.Contains("namespace MyApp.Application.User.Commands.Login;", content);
        Assert.Contains("public sealed record LoginCommand", content);
    }

    [Fact]
    public void Generate_WithHandler_Creates_All_Four_Files()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new CqrsCommandGenerator("User", "Login", "MyApp", root, withHandler: true, withEndpoint: false).Generate();

        string dir = Path.Combine(root, "src", "MyApp.Application", "User", "Commands", "Login");
        Assert.True(File.Exists(Path.Combine(dir, "LoginCommand.cs")));
        Assert.True(File.Exists(Path.Combine(dir, "LoginCommandResult.cs")));
        Assert.True(File.Exists(Path.Combine(dir, "ILoginCommandHandler.cs")));
        Assert.True(File.Exists(Path.Combine(dir, "LoginCommandHandler.cs")));
    }

    [Fact]
    public void Generate_WithHandler_Creates_Correct_Interface_Namespace_And_Signature()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new CqrsCommandGenerator("User", "Login", "MyApp", root, withHandler: true, withEndpoint: false).Generate();

        string content = File.ReadAllText(Path.Combine(root, "src", "MyApp.Application", "User", "Commands", "Login", "ILoginCommandHandler.cs"));
        Assert.Contains("namespace MyApp.Application.User.Commands.Login;", content);
        Assert.Contains("public interface ILoginCommandHandler", content);
        Assert.Contains("Task<LoginCommandResult> HandleAsync(LoginCommand command", content);
    }

    [Fact]
    public void Generate_WithHandler_Creates_Correct_Handler_Implementation()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new CqrsCommandGenerator("User", "Login", "MyApp", root, withHandler: true, withEndpoint: false).Generate();

        string content = File.ReadAllText(Path.Combine(root, "src", "MyApp.Application", "User", "Commands", "Login", "LoginCommandHandler.cs"));
        Assert.Contains("namespace MyApp.Application.User.Commands.Login;", content);
        Assert.Contains("public sealed class LoginCommandHandler : ILoginCommandHandler", content);
        Assert.Contains("NotImplementedException", content);
    }

    [Fact]
    public void Generate_WithHandler_Registers_In_DI()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new CqrsCommandGenerator("User", "Login", "MyApp", root, withHandler: true, withEndpoint: false).Generate();

        string diPath = Path.Combine(root, "src", "MyApp.Application", "DependencyInjection.cs");
        Assert.True(File.Exists(diPath));
        string content = File.ReadAllText(diPath);
        Assert.Contains("ILoginCommandHandler", content);
        Assert.Contains("LoginCommandHandler", content);
        Assert.Contains("AddScoped", content);
    }

    [Fact]
    public void Generate_WithHandler_DI_Is_Idempotent()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new CqrsCommandGenerator("User", "Login", "MyApp", root, withHandler: true, withEndpoint: false).Generate();
        new CqrsCommandGenerator("User", "Login", "MyApp", root, withHandler: true, withEndpoint: false).Generate();

        string diPath = Path.Combine(root, "src", "MyApp.Application", "DependencyInjection.cs");
        string content = File.ReadAllText(diPath);
        int count = CountOccurrences(content, "ILoginCommandHandler");
        Assert.Equal(1, count);
    }

    [Fact]
    public void Generate_WithEndpoint_Creates_Controller_File()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new CqrsCommandGenerator("User", "Login", "MyApp", root, withHandler: false, withEndpoint: true).Generate();

        string controllerPath = Path.Combine(root, "src", "MyApp.Api", "Controllers", "UserController.cs");
        Assert.True(File.Exists(controllerPath));
        string content = File.ReadAllText(controllerPath);
        Assert.Contains("UserController", content);
        Assert.Contains("[HttpPost", content);
        Assert.Contains("Login", content);
    }

    [Fact]
    public void Generate_WithEndpoint_Also_Creates_Handler_Files()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        new CqrsCommandGenerator("User", "Login", "MyApp", root, withHandler: false, withEndpoint: true).Generate();

        Assert.True(File.Exists(Path.Combine(root, "src", "MyApp.Application", "User", "Commands", "Login", "ILoginCommandHandler.cs")));
        Assert.True(File.Exists(Path.Combine(root, "src", "MyApp.Application", "User", "Commands", "Login", "LoginCommandHandler.cs")));
    }

    private static int CountOccurrences(string text, string pattern)
    {
        int count = 0;
        int index = 0;
        while ((index = text.IndexOf(pattern, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += pattern.Length;
        }

        return count;
    }
}
