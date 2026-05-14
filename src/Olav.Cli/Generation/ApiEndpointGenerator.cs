// <copyright file="ApiEndpointGenerator.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Generation;

using Olav.Infrastructure;
using Olav.Templates;

/// <summary>
/// Creates or updates the API controller for a given entity.
/// When the controller file does not exist it is created from scratch.
/// When it already exists a new action is injected before the class closing brace.
/// A console warning is printed when injection is required so the developer knows
/// to update the constructor parameter list manually.
/// </summary>
public class ApiEndpointGenerator
{
    private readonly string entityName;
    private readonly string operationName;
    private readonly string projectName;
    private readonly string root;
    private readonly EndpointKind kind;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiEndpointGenerator"/> class.
    /// </summary>
    /// <param name="entityName">Entity name (e.g. <c>User</c>).</param>
    /// <param name="operationName">Command or query name without suffix (e.g. <c>Login</c>).</param>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <param name="root">Repository root directory.</param>
    /// <param name="kind">Whether this is a command (POST) or query (GET) endpoint.</param>
    public ApiEndpointGenerator(
        string entityName,
        string operationName,
        string projectName,
        string root,
        EndpointKind kind)
    {
        this.entityName = entityName;
        this.operationName = operationName;
        this.projectName = projectName;
        this.root = root;
        this.kind = kind;
    }

    /// <summary>
    /// Discriminates between command (POST) and query (GET) endpoints.
    /// </summary>
    public enum EndpointKind
    {
        /// <summary>HTTP POST action targeting a command handler.</summary>
        Command,

        /// <summary>HTTP GET action targeting a query handler.</summary>
        Query,
    }

    /// <summary>
    /// Generates or updates the controller file.
    /// </summary>
    public void Generate()
    {
        string controllerPath = Path.Combine(
            this.root,
            "src",
            $"{this.projectName}.Api",
            "Controllers",
            $"{this.entityName}Controller.cs");

        string handlerFieldName = char.ToLowerInvariant(this.operationName[0]) + this.operationName[1..] + "Handler";

        if (!File.Exists(controllerPath))
        {
            string content = this.kind == EndpointKind.Command
                ? ControllerTemplate.GenerateForCommand(this.projectName, this.entityName, this.operationName, handlerFieldName)
                : ControllerTemplate.GenerateForQuery(this.projectName, this.entityName, this.operationName, handlerFieldName);

            FileSystem.WriteFile(controllerPath, content);
        }
        else
        {
            string actionSnippet = this.kind == EndpointKind.Command
                ? ControllerActionTemplate.GenerateCommandActionStub(this.projectName, this.entityName, this.operationName)
                : ControllerActionTemplate.GenerateQueryActionStub(this.projectName, this.entityName, this.operationName);

            string usingLine = this.kind == EndpointKind.Command
                ? ControllerActionTemplate.CommandUsingLine(this.projectName, this.entityName, this.operationName)
                : ControllerActionTemplate.QueryUsingLine(this.projectName, this.entityName, this.operationName);

            string handlerInterface = this.kind == EndpointKind.Command
                ? $"I{this.operationName}CommandHandler"
                : $"I{this.operationName}QueryHandler";

            ControllerActionInjector.Inject(controllerPath, this.operationName, actionSnippet, usingLine);

            Console.WriteLine($"⚠ Add constructor parameter '{handlerInterface} {handlerFieldName}' to {this.entityName}Controller manually.");
        }
    }
}
