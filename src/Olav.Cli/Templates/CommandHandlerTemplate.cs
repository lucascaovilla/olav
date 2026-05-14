// <copyright file="CommandHandlerTemplate.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Templates;

/// <summary>
/// Provides the Application command handler implementation template.
/// </summary>
public static class CommandHandlerTemplate
{
    /// <summary>
    /// Returns the content of a CQRS command handler implementation stub.
    /// </summary>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <param name="entityName">Entity folder name (e.g. <c>User</c>).</param>
    /// <param name="commandName">Command name without suffix (e.g. <c>Login</c>).</param>
    /// <returns>Command handler implementation file content.</returns>
    public static string Generate(string projectName, string entityName, string commandName)
    {
        return $$"""
        namespace {{projectName}}.Application.{{entityName}}.Commands.{{commandName}};

        /// <summary>
        /// Implementation of <see cref="I{{commandName}}CommandHandler"/>.
        /// </summary>
        public sealed class {{commandName}}CommandHandler : I{{commandName}}CommandHandler
        {
            /// <inheritdoc/>
            public Task<{{commandName}}CommandResult> HandleAsync({{commandName}}Command command, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }
        }
        """;
    }
}
