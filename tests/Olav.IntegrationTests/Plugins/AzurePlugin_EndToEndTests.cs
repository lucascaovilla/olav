#nullable enable
using System.Collections.Generic;
using System.IO;
using Xunit;
using Olav.Infrastructure;
using Olav.Generation;

namespace Olav.IntegrationTests.Plugins;

public class AzurePlugin_EndToEndTests : IClassFixture<PluginTestFixture>
{
    private readonly PluginTestFixture fixture;

    public AzurePlugin_EndToEndTests(PluginTestFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void AddDeploymentAzure_CreatesWorkflowFile()
    {
        string root = this.fixture.ProjectPath;
        OlavConfig config = OlavConfig.Load(root);

        if (config.Plugins?.Find(p => p.Id == "azure") == null)
        {
            PluginInstallGenerator generator = PluginInstallGenerator.Create(root);
            generator.FetchInfo("azure");
            generator.Install(
                "azure",
                new Dictionary<string, string>
                {
                    { "appServiceName", "my-app-service" },
                    { "resourceGroup", "my-resource-group" },
                    { "environment", "production" },
                    { "publishProfileSecret", "AZURE_PUBLISH_PROFILE" },
                });
        }

        string workflowPath = Path.Combine(root, ".github", "workflows", "deploy-azure.yml");
        Assert.True(File.Exists(workflowPath), "Expected deploy-azure.yml to be created.");
    }

    [Fact]
    public void AddDeploymentAzure_WorkflowContainsAppServiceName()
    {
        string root = this.fixture.ProjectPath;
        OlavConfig config = OlavConfig.Load(root);
        PluginEntry? existing = config.Plugins?.Find(p => p.Id == "azure");

        if (existing == null)
        {
            PluginInstallGenerator generator = PluginInstallGenerator.Create(root);
            generator.FetchInfo("azure");
            generator.Install(
                "azure",
                new Dictionary<string, string>
                {
                    { "appServiceName", "my-app-service" },
                    { "resourceGroup", "my-resource-group" },
                    { "environment", "production" },
                    { "publishProfileSecret", "AZURE_PUBLISH_PROFILE" },
                });
        }

        string workflowPath = Path.Combine(root, ".github", "workflows", "deploy-azure.yml");
        string content = File.ReadAllText(workflowPath);

        Assert.Contains("my-app-service", content);
        Assert.Contains("my-resource-group", content);
        Assert.Contains("production", content);
        Assert.Contains("AZURE_PUBLISH_PROFILE", content);
    }

    [Fact]
    public void AddDeploymentAzure_RecordsPluginInOlavJson()
    {
        string root = this.fixture.ProjectPath;
        OlavConfig config = OlavConfig.Load(root);
        PluginEntry? existing = config.Plugins?.Find(p => p.Id == "azure");

        if (existing == null)
        {
            PluginInstallGenerator generator = PluginInstallGenerator.Create(root);
            generator.FetchInfo("azure");
            generator.Install(
                "azure",
                new Dictionary<string, string>
                {
                    { "appServiceName", "my-app" },
                    { "resourceGroup", "my-rg" },
                    { "environment", "staging" },
                    { "publishProfileSecret", "MY_SECRET" },
                });
        }

        OlavConfig reloaded = OlavConfig.Load(root);
        PluginEntry? entry = reloaded.Plugins?.Find(p => p.Id == "azure");

        Assert.NotNull(entry);
        Assert.Equal("deployment", entry!.Category);
        Assert.Equal("generation", entry.Delivery);
        Assert.Equal("official", entry.Source);
    }

    [Fact]
    public void AddDeploymentAzure_DoubleInstall_ThrowsError()
    {
        string root = this.fixture.ProjectPath;
        OlavConfig config = OlavConfig.Load(root);

        if (config.Plugins?.Find(p => p.Id == "azure") == null)
        {
            PluginInstallGenerator first = PluginInstallGenerator.Create(root);
            first.FetchInfo("azure");
            first.Install(
                "azure",
                new Dictionary<string, string>
                {
                    { "appServiceName", "app" },
                    { "resourceGroup", "rg" },
                    { "environment", "prod" },
                    { "publishProfileSecret", "SECRET" },
                });
        }

        PluginInstallGenerator second = PluginInstallGenerator.Create(root);
        second.FetchInfo("azure");

        System.InvalidOperationException ex = Assert.Throws<System.InvalidOperationException>(
            () => second.Install(
                "azure",
                new Dictionary<string, string>
                {
                    { "appServiceName", "app2" },
                    { "resourceGroup", "rg2" },
                    { "environment", "prod" },
                    { "publishProfileSecret", "SECRET2" },
                }));

        Assert.Contains("already installed", ex.Message);
    }
}
