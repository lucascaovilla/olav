// <copyright file="QueryResultTemplate.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Templates;

/// <summary>
/// Provides the Application query result record template.
/// </summary>
public static class QueryResultTemplate
{
    /// <summary>
    /// Returns the content of a CQRS query result record.
    /// </summary>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <param name="entityName">Entity folder name (e.g. <c>User</c>).</param>
    /// <param name="queryName">Query name without suffix (e.g. <c>GetUser</c>).</param>
    /// <returns>Query result record file content.</returns>
    public static string Generate(string projectName, string entityName, string queryName)
    {
        return $$"""
        namespace {{projectName}}.Application.{{entityName}}.Queries.{{queryName}};

        /// <summary>
        /// Result of <see cref="{{queryName}}Query"/>.
        /// </summary>
        public sealed record {{queryName}}QueryResult(Guid Id);
        """;
    }
}
