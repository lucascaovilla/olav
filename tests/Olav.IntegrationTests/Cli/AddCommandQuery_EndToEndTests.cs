using System;
using System.IO;
using Xunit;
using Olav.IntegrationTests.Generation.Fixtures;

namespace Olav.IntegrationTests.Cli;

[Collection("GeneratedProject")]
public class AddCommandQuery_EndToEndTests(GeneratedProjectFixture fixture)
{
    private readonly GeneratedProjectFixture fixture = fixture;

    [Fact]
    public void AddCommand_Creates_Command_And_Result_Records()
    {
        string root = this.fixture.ProjectPath;
        string name = this.fixture.ProjectName;

        System.IO.Directory.SetCurrentDirectory(root);
        Olav.Program.Main(["add", "command", "Invoice", "Create"]);

        string dir = Path.Combine(root, "src", $"{name}.Application", "Invoice", "Commands", "Create");
        Assert.True(File.Exists(Path.Combine(dir, "CreateCommand.cs")));
        Assert.True(File.Exists(Path.Combine(dir, "CreateCommandResult.cs")));
        Assert.False(File.Exists(Path.Combine(dir, "ICreateCommandHandler.cs")), "Handler should not be created without --with-handler");
    }

    [Fact]
    public void AddCommand_WithHandler_Creates_All_Four_Files()
    {
        string root = this.fixture.ProjectPath;
        string name = this.fixture.ProjectName;

        System.IO.Directory.SetCurrentDirectory(root);
        Olav.Program.Main(["add", "command", "Invoice", "Cancel", "--with-handler"]);

        string dir = Path.Combine(root, "src", $"{name}.Application", "Invoice", "Commands", "Cancel");
        Assert.True(File.Exists(Path.Combine(dir, "CancelCommand.cs")));
        Assert.True(File.Exists(Path.Combine(dir, "CancelCommandResult.cs")));
        Assert.True(File.Exists(Path.Combine(dir, "ICancelCommandHandler.cs")));
        Assert.True(File.Exists(Path.Combine(dir, "CancelCommandHandler.cs")));
    }

    [Fact]
    public void AddCommand_WithHandler_Content_Is_Correct()
    {
        string root = this.fixture.ProjectPath;
        string name = this.fixture.ProjectName;

        System.IO.Directory.SetCurrentDirectory(root);
        Olav.Program.Main(["add", "command", "Invoice", "Approve", "--with-handler"]);

        string handler = File.ReadAllText(
            Path.Combine(root, "src", $"{name}.Application", "Invoice", "Commands", "Approve", "ApproveCommandHandler.cs"));

        Assert.Contains($"namespace {name}.Application.Invoice.Commands.Approve;", handler);
        Assert.Contains("public sealed class ApproveCommandHandler : IApproveCommandHandler", handler);
        Assert.Contains("NotImplementedException", handler);
    }

    [Fact]
    public void AddCommand_WithHandler_Writes_DI_Registration()
    {
        string root = this.fixture.ProjectPath;
        string name = this.fixture.ProjectName;

        System.IO.Directory.SetCurrentDirectory(root);
        Olav.Program.Main(["add", "command", "Invoice", "Archive", "--with-handler"]);

        string diPath = Path.Combine(root, "src", $"{name}.Application", "DependencyInjection.cs");
        Assert.True(File.Exists(diPath));
        string content = File.ReadAllText(diPath);
        Assert.Contains("IArchiveCommandHandler", content);
        Assert.Contains("ArchiveCommandHandler", content);
    }

    [Fact]
    public void AddQuery_Creates_Query_And_Result_Records()
    {
        string root = this.fixture.ProjectPath;
        string name = this.fixture.ProjectName;

        System.IO.Directory.SetCurrentDirectory(root);
        Olav.Program.Main(["add", "query", "Invoice", "GetById"]);

        string dir = Path.Combine(root, "src", $"{name}.Application", "Invoice", "Queries", "GetById");
        Assert.True(File.Exists(Path.Combine(dir, "GetByIdQuery.cs")));
        Assert.True(File.Exists(Path.Combine(dir, "GetByIdQueryResult.cs")));
        Assert.False(File.Exists(Path.Combine(dir, "IGetByIdQueryHandler.cs")), "Handler should not be created without --with-handler");
    }

    [Fact]
    public void AddQuery_WithHandler_Creates_All_Four_Files()
    {
        string root = this.fixture.ProjectPath;
        string name = this.fixture.ProjectName;

        System.IO.Directory.SetCurrentDirectory(root);
        Olav.Program.Main(["add", "query", "Invoice", "ListAll", "--with-handler"]);

        string dir = Path.Combine(root, "src", $"{name}.Application", "Invoice", "Queries", "ListAll");
        Assert.True(File.Exists(Path.Combine(dir, "ListAllQuery.cs")));
        Assert.True(File.Exists(Path.Combine(dir, "ListAllQueryResult.cs")));
        Assert.True(File.Exists(Path.Combine(dir, "IListAllQueryHandler.cs")));
        Assert.True(File.Exists(Path.Combine(dir, "ListAllQueryHandler.cs")));
    }

    [Fact]
    public void AddQuery_WithHandler_Interface_Has_Nullable_Return_Type()
    {
        string root = this.fixture.ProjectPath;
        string name = this.fixture.ProjectName;

        System.IO.Directory.SetCurrentDirectory(root);
        Olav.Program.Main(["add", "query", "Invoice", "FindByNumber", "--with-handler"]);

        string content = File.ReadAllText(
            Path.Combine(root, "src", $"{name}.Application", "Invoice", "Queries", "FindByNumber", "IFindByNumberQueryHandler.cs"));

        Assert.Contains("Task<FindByNumberQueryResult?>", content);
    }

    [Fact]
    public void AddCommand_WithEndpoint_Creates_Controller()
    {
        string root = this.fixture.ProjectPath;
        string name = this.fixture.ProjectName;

        System.IO.Directory.SetCurrentDirectory(root);
        Olav.Program.Main(["add", "command", "Report", "Generate", "--with-endpoint"]);

        string controllerPath = Path.Combine(root, "src", $"{name}.Api", "Controllers", "ReportController.cs");
        Assert.True(File.Exists(controllerPath));
        string content = File.ReadAllText(controllerPath);
        Assert.Contains("ReportController", content);
        Assert.Contains("[HttpPost", content);
        Assert.Contains("generate", content);
    }

    [Fact]
    public void AddQuery_WithEndpoint_Creates_Controller_With_Get_Action()
    {
        string root = this.fixture.ProjectPath;
        string name = this.fixture.ProjectName;

        System.IO.Directory.SetCurrentDirectory(root);
        Olav.Program.Main(["add", "query", "Report", "GetSummary", "--with-endpoint"]);

        string controllerPath = Path.Combine(root, "src", $"{name}.Api", "Controllers", "ReportController.cs");

        // Controller may already exist from AddCommand_WithEndpoint_Creates_Controller — action is injected
        Assert.True(File.Exists(controllerPath));
        string content = File.ReadAllText(controllerPath);
        Assert.Contains("[HttpGet", content);
        Assert.Contains("GetSummary", content);
    }

    [Fact]
    public void MakeMigration_Prints_Error_When_Plugin_Not_Installed()
    {
        string root = this.fixture.ProjectPath;

        System.IO.Directory.SetCurrentDirectory(root);

        using StringWriter sw = new StringWriter();
        TextWriter original = Console.Out;
        Console.SetOut(sw);
        try
        {
            Olav.Program.Main(["make", "migration", "sqlserver", "InitialCreate"]);
        }
        finally
        {
            Console.SetOut(original);
        }

        string output = sw.ToString();
        Assert.Contains("not installed", output);
    }
}
