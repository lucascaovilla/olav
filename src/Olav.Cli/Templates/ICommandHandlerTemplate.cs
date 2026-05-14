// <copyright file="ICommandHandlerTemplate.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Templates;

/// <summary>
/// Provides the Application command handler interface template.
/// </summary>
public static class ICommandHandlerTemplate
{
    /// <summary>
    /// Returns the content of a CQRS command handler interface.
    /// </summary>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <param name="entityName">Entity folder name (e.g. <c>User</c>).</param>
    /// <param name="commandName">Command name without suffix (e.g. <c>Login</c>).</param>
    /// <returns>Command handler interface file content.</returns>
    public static string Generate(string projectName, string entityName, string commandName)
    {
        return $$"""
        namespace {{projectName}}.Application.{{entityName}}.Commands.{{commandName}};

        /// <summary>
        /// Handles <see cref="{{commandName}}Command"/>.
        /// </summary>
        public interface I{{commandName}}CommandHandler
        {
            /// <summary>
            /// Executes the command and returns its result.
            /// </summary>
            /// <param name="command">The command payload.</param>
            /// <param name="cancellationToken">Cancellation token.</param>
            public Task<{{commandName}}CommandResult> HandleAsync({{commandName}}Command command, CancellationToken cancellationToken = default);
        }
        """;
    }
}
