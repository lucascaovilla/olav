// <copyright file="CommandTemplate.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Templates;

/// <summary>
/// Provides the Application command record template.
/// </summary>
public static class CommandTemplate
{
    /// <summary>
    /// Returns the content of a CQRS command record.
    /// </summary>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <param name="entityName">Entity folder name (e.g. <c>User</c>).</param>
    /// <param name="commandName">Command name without suffix (e.g. <c>Login</c>).</param>
    /// <returns>Command record file content.</returns>
    public static string Generate(string projectName, string entityName, string commandName)
    {
        return $$"""
        namespace {{projectName}}.Application.{{entityName}}.Commands.{{commandName}};

        /// <summary>
        /// Command to execute the {{commandName}} use case on <c>{{entityName}}</c>.
        /// </summary>
        public sealed record {{commandName}}Command();
        """;
    }
}
