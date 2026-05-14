// <copyright file="CqrsQueryGenerator.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Generation;

using Olav.Infrastructure;
using Olav.Templates;

/// <summary>
/// Generates CQRS query artifacts for a given entity.
/// Always generates the query record and its result record.
/// Optionally generates the handler interface and implementation when <c>withHandler</c> is true.
/// Optionally generates or updates the API controller when <c>withEndpoint</c> is true (implies withHandler).
/// </summary>
public class CqrsQueryGenerator
{
    private readonly string entityName;
    private readonly string queryName;
    private readonly string projectName;
    private readonly string root;
    private readonly bool withHandler;
    private readonly bool withEndpoint;

    /// <summary>
    /// Initializes a new instance of the <see cref="CqrsQueryGenerator"/> class.
    /// </summary>
    /// <param name="entityName">Entity name (e.g. <c>User</c>).</param>
    /// <param name="queryName">Query name without suffix (e.g. <c>GetUser</c>).</param>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <param name="root">Repository root directory.</param>
    /// <param name="withHandler">When true, also generates the handler interface and implementation.</param>
    /// <param name="withEndpoint">When true, also generates/updates the API controller. Forces withHandler.</param>
    public CqrsQueryGenerator(
        string entityName,
        string queryName,
        string projectName,
        string root,
        bool withHandler,
        bool withEndpoint)
    {
        this.entityName = entityName;
        this.queryName = queryName;
        this.projectName = projectName;
        this.root = root;
        this.withHandler = withHandler || withEndpoint;
        this.withEndpoint = withEndpoint;
    }

    /// <summary>
    /// Generates the query artifacts according to the configured flags.
    /// </summary>
    public void Generate()
    {
        string queryDir = Path.Combine(
            this.root,
            "src",
            $"{this.projectName}.Application",
            this.entityName,
            "Queries",
            this.queryName);

        FileSystem.WriteFile(
            Path.Combine(queryDir, $"{this.queryName}Query.cs"),
            QueryTemplate.Generate(this.projectName, this.entityName, this.queryName));

        FileSystem.WriteFile(
            Path.Combine(queryDir, $"{this.queryName}QueryResult.cs"),
            QueryResultTemplate.Generate(this.projectName, this.entityName, this.queryName));

        if (this.withHandler)
        {
            FileSystem.WriteFile(
                Path.Combine(queryDir, $"I{this.queryName}QueryHandler.cs"),
                IQueryHandlerTemplate.Generate(this.projectName, this.entityName, this.queryName));

            FileSystem.WriteFile(
                Path.Combine(queryDir, $"{this.queryName}QueryHandler.cs"),
                QueryHandlerTemplate.Generate(this.projectName, this.entityName, this.queryName));

            this.RegisterInDi();
        }

        if (this.withEndpoint)
        {
            new ApiEndpointGenerator(
                this.entityName,
                this.queryName,
                this.projectName,
                this.root,
                ApiEndpointGenerator.EndpointKind.Query).Generate();
        }
    }

    private void RegisterInDi()
    {
        string diPath = Path.Combine(
            this.root,
            "src",
            $"{this.projectName}.Application",
            "DependencyInjection.cs");

        string registration =
            $"services.AddScoped<{this.projectName}.Application.{this.entityName}.Queries.{this.queryName}.I{this.queryName}QueryHandler, " +
            $"{this.projectName}.Application.{this.entityName}.Queries.{this.queryName}.{this.queryName}QueryHandler>();";

        if (!File.Exists(diPath))
        {
            FileSystem.WriteFile(diPath, ApplicationExtensionsTemplate.GenerateEmpty(this.projectName));
        }

        DiRegistrationInjector.Inject(diPath, registration);

        string programPath = Path.Combine(
            this.root,
            "src",
            $"{this.projectName}.Api",
            "Program.cs");
        ProgramDiInjector.Inject(programPath, "builder.Services.AddApplication();");
        ProgramDiInjector.InjectUsing(programPath, $"{this.projectName}.Application");
    }
}
