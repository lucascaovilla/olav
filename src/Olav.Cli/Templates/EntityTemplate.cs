// <copyright file="EntityTemplate.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Templates;

/// <summary>
/// Provides the Domain entity class template.
/// </summary>
public static class EntityTemplate
{
    /// <summary>
    /// Returns the content of a Domain entity class.
    /// </summary>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <param name="entityName">Entity class name (e.g. <c>Order</c>).</param>
    /// <returns>Entity file content.</returns>
    public static string Generate(string projectName, string entityName)
    {
        return $$"""
        namespace {{projectName}}.Domain.{{entityName}}.Entities;

        /// <summary>
        /// Represents the {{entityName}} domain entity.
        /// </summary>
        public class {{entityName}}
        {
            /// <summary>
            /// Gets the unique identifier for this entity.
            /// </summary>
            public Guid Id { get; private set; } = Guid.NewGuid();
        }
        """;
    }
}
