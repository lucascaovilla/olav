// <copyright file="ServiceTemplate.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Templates;

/// <summary>
/// Provides Application service implementation templates.
/// </summary>
public static class ServiceTemplate
{
    /// <summary>
    /// Returns the content of an Application service implementation scoped to an entity folder.
    /// The constructor always receives <c>I{entityName}Repository</c> as a dependency.
    /// </summary>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <param name="entityName">Entity folder name and repository owner (e.g. <c>Order</c>).</param>
    /// <param name="serviceName">Service class name (e.g. <c>OrderService</c>).</param>
    /// <param name="plugin">
    /// Optional installed infrastructure plugin id (e.g. <c>postgres</c>).
    /// Reserved for future template variants; currently unused.
    /// </param>
    /// <returns>Service implementation file content.</returns>
    public static string Generate(string projectName, string entityName, string serviceName, string? plugin = null)
    {
        return $$"""
        namespace {{projectName}}.Application.Services.{{entityName}};

        using {{projectName}}.Domain.{{entityName}}.Repositories;

        /// <summary>
        /// Implementation of <see cref="I{{serviceName}}"/>.
        /// </summary>
        public class {{serviceName}} : I{{serviceName}}
        {
            private readonly I{{entityName}}Repository repository;

            /// <summary>
            /// Initializes a new instance of the <see cref="{{serviceName}}"/> class.
            /// </summary>
            /// <param name="repository">The <see cref="{{entityName}}"/> repository.</param>
            public {{serviceName}}(I{{entityName}}Repository repository) => this.repository = repository;
        }
        """;
    }
}
