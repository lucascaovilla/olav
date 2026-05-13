// <copyright file="IServiceTemplate.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Templates;

/// <summary>
/// Provides the Application service interface template.
/// </summary>
public static class IServiceTemplate
{
    /// <summary>
    /// Returns the content of an Application service interface.
    /// </summary>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <param name="entityName">Entity folder name (e.g. <c>Order</c>).</param>
    /// <param name="serviceName">Service class name (e.g. <c>OrderService</c>).</param>
    /// <returns>Service interface file content.</returns>
    public static string Generate(string projectName, string entityName, string serviceName)
    {
        return $$"""
        namespace {{projectName}}.Application.Services.{{entityName}};

        /// <summary>
        /// Contract for <see cref="{{serviceName}}"/>.
        /// </summary>
        public interface I{{serviceName}}
        {
        }
        """;
    }
}
