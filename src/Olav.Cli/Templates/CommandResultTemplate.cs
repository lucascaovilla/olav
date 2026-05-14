// <copyright file="CommandResultTemplate.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Templates;

/// <summary>
/// Provides the Application command result record template.
/// </summary>
public static class CommandResultTemplate
{
    /// <summary>
    /// Returns the content of a CQRS command result record.
    /// </summary>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <param name="entityName">Entity folder name (e.g. <c>User</c>).</param>
    /// <param name="commandName">Command name without suffix (e.g. <c>Login</c>).</param>
    /// <returns>Command result record file content.</returns>
    public static string Generate(string projectName, string entityName, string commandName)
    {
        return $$"""
        namespace {{projectName}}.Application.{{entityName}}.Commands.{{commandName}};

        /// <summary>
        /// Result of <see cref="{{commandName}}Command"/>.
        /// </summary>
        public sealed record {{commandName}}CommandResult();
        """;
    }
}
