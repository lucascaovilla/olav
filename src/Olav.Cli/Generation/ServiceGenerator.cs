// <copyright file="ServiceGenerator.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Generation;

using System.IO;
using Olav.Infrastructure;
using Olav.Templates;

/// <summary>
/// Generates a service interface in Application and its implementation in the requested layer.
/// </summary>
public class ServiceGenerator
{
    private readonly string serviceName;
    private readonly string projectName;
    private readonly string root;
    private readonly string entityName;
    private readonly ServiceLayer layer;
    private readonly string? plugin;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceGenerator"/> class.
    /// </summary>
    /// <param name="entityName">Entity folder name (e.g. <c>Order</c>).</param>
    /// <param name="serviceName">Service class name (e.g. <c>OrderService</c>).</param>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <param name="root">Repository root directory.</param>
    /// <param name="layer">Target layer for the implementation.</param>
    /// <param name="plugin">
    /// Optional installed infrastructure plugin id (e.g. <c>postgres</c>).
    /// Passed to the template for future plugin-aware generation variants.
    /// </param>
    public ServiceGenerator(
        string entityName,
        string serviceName,
        string projectName,
        string root,
        ServiceLayer layer = ServiceLayer.Application,
        string? plugin = null)
    {
        this.entityName = entityName;
        this.serviceName = serviceName;
        this.projectName = projectName;
        this.root = root;
        this.layer = layer;
        this.plugin = plugin;
    }

    /// <summary>Target layer for the service implementation.</summary>
    public enum ServiceLayer
    {
        /// <summary>Interface and implementation both in Application.</summary>
        Application,

        /// <summary>Interface in Application; implementation in Infrastructure.</summary>
        Infrastructure,
    }

    /// <summary>
    /// Generates the interface in Application and the implementation in the target layer,
    /// then registers the pair in the matching DI extension file.
    /// </summary>
    public void Generate()
    {
        this.WriteInterface();

        if (this.layer == ServiceLayer.Infrastructure)
        {
            this.WriteInfrastructureImplementation();
            this.RegisterInInfrastructureDi();
        }
        else
        {
            this.WriteApplicationImplementation();
            this.RegisterInApplicationDi();
        }
    }

    private void WriteInterface()
    {
        string interfacePath = Path.Combine(
            this.root,
            "src",
            $"{this.projectName}.Application",
            "Services",
            this.entityName,
            $"I{this.serviceName}.cs");

        FileSystem.WriteFile(interfacePath, IServiceTemplate.Generate(this.projectName, this.entityName, this.serviceName));
    }

    private void WriteApplicationImplementation()
    {
        string implPath = Path.Combine(
            this.root,
            "src",
            $"{this.projectName}.Application",
            "Services",
            this.entityName,
            $"{this.serviceName}.cs");

        FileSystem.WriteFile(implPath, ServiceTemplate.Generate(this.projectName, this.entityName, this.serviceName, this.plugin));
    }

    private void WriteInfrastructureImplementation()
    {
        string implPath = Path.Combine(
            this.root,
            "src",
            $"{this.projectName}.Infrastructure",
            "Services",
            this.entityName,
            $"{this.serviceName}.cs");

        FileSystem.WriteFile(implPath, InfrastructureServiceTemplate.Generate(this.projectName, this.entityName, this.serviceName));
    }

    private void RegisterInApplicationDi()
    {
        string diPath = Path.Combine(
            this.root,
            "src",
            $"{this.projectName}.Application",
            "DependencyInjection.cs");

        string registration =
            $"services.AddScoped<{this.projectName}.Application.Services.{this.entityName}.I{this.serviceName}, {this.projectName}.Application.Services.{this.entityName}.{this.serviceName}>();";

        if (!File.Exists(diPath))
        {
            FileSystem.WriteFile(diPath, ApplicationExtensionsTemplate.Generate(this.projectName, this.entityName, this.serviceName));
        }
        else
        {
            DiRegistrationInjector.Inject(diPath, registration);
        }
    }

    private void RegisterInInfrastructureDi()
    {
        string diPath = Path.Combine(
            this.root,
            "src",
            $"{this.projectName}.Infrastructure",
            "DependencyInjection.cs");

        string registration =
            $"services.AddScoped<{this.projectName}.Application.Services.{this.entityName}.I{this.serviceName}, {this.projectName}.Infrastructure.Services.{this.entityName}.{this.serviceName}>();";

        if (!File.Exists(diPath))
        {
            FileSystem.WriteFile(diPath, InfrastructureExtensionsTemplate.GenerateForService(this.projectName, this.entityName, this.serviceName));
        }
        else
        {
            DiRegistrationInjector.Inject(diPath, registration);
        }
    }
}
