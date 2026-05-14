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
    /// Returns the content of a Domain enum.
    /// </summary>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <param name="enumName">Enum type name (e.g. <c>OrderStatus</c>).</param>
    /// <returns>Enum file content.</returns>
    public static string Generate(string projectName, string enumName)
    {
        return $$"""
        namespace {{projectName}}.Domain.Enums;

        /// <summary>
        /// Represents the {{enumName}} domain enum.
        /// </summary>
        public enum {{enumName}}
        {
        }
        """;
    }
}
