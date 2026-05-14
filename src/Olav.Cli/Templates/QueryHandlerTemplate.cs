// <copyright file="QueryHandlerTemplate.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Templates;

/// <summary>
/// Provides the Application query handler implementation template.
/// </summary>
public static class QueryHandlerTemplate
{
    /// <summary>
    /// Returns the content of a CQRS query handler implementation stub.
    /// </summary>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <param name="entityName">Entity folder name (e.g. <c>User</c>).</param>
    /// <param name="queryName">Query name without suffix (e.g. <c>GetUser</c>).</param>
    /// <returns>Query handler implementation file content.</returns>
    public static string Generate(string projectName, string entityName, string queryName)
    {
        return $$"""
        namespace {{projectName}}.Application.{{entityName}}.Queries.{{queryName}};

        /// <summary>
        /// Implementation of <see cref="I{{queryName}}QueryHandler"/>.
        /// </summary>
        public sealed class {{queryName}}QueryHandler : I{{queryName}}QueryHandler
        {
            /// <inheritdoc/>
            public Task<{{queryName}}QueryResult?> HandleAsync({{queryName}}Query query, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }
        }
        """;
    }
}
