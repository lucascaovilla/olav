// <copyright file="DomainEnumTemplate.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Templates;

/// <summary>
/// Provides the Domain enum template.
/// </summary>
public static class DomainEnumTemplate
{
    /// <summary>
    /// Returns the content of a Domain enum scoped to an entity folder.
    /// </summary>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <param name="entityName">Entity folder name (e.g. <c>Order</c>).</param>
    /// <param name="enumName">Enum type name (e.g. <c>OrderStatus</c>).</param>
    /// <returns>Enum file content.</returns>
    public static string Generate(string projectName, string entityName, string enumName)
    {
        return $$"""
        namespace {{projectName}}.Domain.{{entityName}}.Enums;

        /// <summary>
        /// Represents the {{enumName}} domain enum.
        /// </summary>
        public enum {{enumName}}
        {
        }
        """;
    }
}
