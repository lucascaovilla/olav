// <copyright file="IRepositoryTemplate.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Templates;

/// <summary>
/// Provides the Domain repository interface template.
/// </summary>
public static class IRepositoryTemplate
{
    /// <summary>
    /// Returns the content of a Domain repository interface.
    /// </summary>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <param name="entityName">Entity name the repository is for (e.g. <c>Order</c>).</param>
    /// <returns>Repository interface file content.</returns>
    public static string Generate(string projectName, string entityName)
    {
        return $$"""
        namespace {{projectName}}.Domain.{{entityName}}.Repositories;

        using {{projectName}}.Domain.{{entityName}}.Entities;

        /// <summary>
        /// Repository contract for <see cref="{{entityName}}"/>.
        /// </summary>
        public interface I{{entityName}}Repository
        {
            /// <summary>Gets a <see cref="{{entityName}}"/> by its identifier.</summary>
            public Task<{{entityName}}?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

            /// <summary>Returns all <see cref="{{entityName}}"/> records.</summary>
            public Task<IEnumerable<{{entityName}}>> GetAllAsync(CancellationToken cancellationToken = default);

            /// <summary>Persists a new <see cref="{{entityName}}"/>.</summary>
            public Task AddAsync({{entityName}} entity, CancellationToken cancellationToken = default);

            /// <summary>Persists changes to an existing <see cref="{{entityName}}"/>.</summary>
            public Task UpdateAsync({{entityName}} entity, CancellationToken cancellationToken = default);

            /// <summary>Removes a <see cref="{{entityName}}"/> by its identifier.</summary>
            public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        }
        """;
    }
}
