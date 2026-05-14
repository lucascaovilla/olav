// <copyright file="CqrsCommandGenerator.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Generation;

using Olav.Infrastructure;
using Olav.Templates;

/// <summary>
/// Generates CQRS command artifacts for a given entity.
/// Always generates the command record and its result record.
/// Optionally generates the handler interface and implementation when <c>withHandler</c> is true.
/// Optionally generates or updates the API controller when <c>withEndpoint</c> is true (implies withHandler).
/// </summary>
public class CqrsCommandGenerator
{
    private readonly string entityName;
    private readonly string commandName;
    private readonly string projectName;
    private readonly string root;
    private readonly bool withHandler;
    private readonly bool withEndpoint;

    /// <summary>
    /// Initializes a new instance of the <see cref="CqrsCommandGenerator"/> class.
    /// </summary>
    /// <param name="entityName">Entity name (e.g. <c>User</c>).</param>
    /// <param name="commandName">Command name without suffix (e.g. <c>Login</c>).</param>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <param name="root">Repository root directory.</param>
    /// <param name="withHandler">When true, also generates the handler interface and implementation.</param>
    /// <param name="withEndpoint">When true, also generates/updates the API controller. Forces withHandler.</param>
    public CqrsCommandGenerator(
        string entityName,
        string commandName,
        string projectName,
        string root,
        bool withHandler,
        bool withEndpoint)
    {
        this.entityName = entityName;
        this.commandName = commandName;
        this.projectName = projectName;
        this.root = root;
        this.withHandler = withHandler || withEndpoint;
        this.withEndpoint = withEndpoint;
    }

    /// <summary>
    /// Generates the command artifacts according to the configured flags.
    /// </summary>
    public void Generate()
    {
        string commandDir = Path.Combine(
            this.root,
            "src",
            $"{this.projectName}.Application",
            this.entityName,
            "Commands",
            this.commandName);

        FileSystem.WriteFile(
            Path.Combine(commandDir, $"{this.commandName}Command.cs"),
            CommandTemplate.Generate(this.projectName, this.entityName, this.commandName));

        FileSystem.WriteFile(
            Path.Combine(commandDir, $"{this.commandName}CommandResult.cs"),
            CommandResultTemplate.Generate(this.projectName, this.entityName, this.commandName));

        if (this.withHandler)
        {
            FileSystem.WriteFile(
                Path.Combine(commandDir, $"I{this.commandName}CommandHandler.cs"),
                ICommandHandlerTemplate.Generate(this.projectName, this.entityName, this.commandName));

            FileSystem.WriteFile(
                Path.Combine(commandDir, $"{this.commandName}CommandHandler.cs"),
                CommandHandlerTemplate.Generate(this.projectName, this.entityName, this.commandName));

            this.RegisterInDi();
        }

        if (this.withEndpoint)
        {
            new ApiEndpointGenerator(
                this.entityName,
                this.commandName,
                this.projectName,
                this.root,
                ApiEndpointGenerator.EndpointKind.Command).Generate();
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
            $"services.AddScoped<{this.projectName}.Application.{this.entityName}.Commands.{this.commandName}.I{this.commandName}CommandHandler, " +
            $"{this.projectName}.Application.{this.entityName}.Commands.{this.commandName}.{this.commandName}CommandHandler>();";

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
