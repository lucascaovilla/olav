// <copyright file="IQueryHandlerTemplate.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Templates;

/// <summary>
/// Provides the Application query handler interface template.
/// </summary>
public static class IQueryHandlerTemplate
{
    /// <summary>
    /// Returns the content of a CQRS query handler interface.
    /// </summary>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <param name="entityName">Entity folder name (e.g. <c>User</c>).</param>
    /// <param name="queryName">Query name without suffix (e.g. <c>GetUser</c>).</param>
    /// <returns>Query handler interface file content.</returns>
    public static string Generate(string projectName, string entityName, string queryName)
    {
        return $$"""
        namespace {{projectName}}.Application.{{entityName}}.Queries.{{queryName}};

        /// <summary>
        /// Handles <see cref="{{queryName}}Query"/>.
        /// </summary>
        public interface I{{queryName}}QueryHandler
        {
            /// <summary>
            /// Executes the query and returns its result, or <see langword="null"/> when not found.
            /// </summary>
            /// <param name="query">The query payload.</param>
            /// <param name="cancellationToken">Cancellation token.</param>
            public Task<{{queryName}}QueryResult?> HandleAsync({{queryName}}Query query, CancellationToken cancellationToken = default);
        }
        """;
    }
}
