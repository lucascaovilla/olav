// <copyright file="PersistenceExtensionsTemplate.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Templates;

/// <summary>
/// Provides the fallback DI extension method template for a persistence sub-project
/// (e.g. <c>{Name}.Infrastructure.Persistence.Postgres</c>).
/// Used when a repository is generated before the plugin's DI file has been created.
/// </summary>
public static class PersistenceExtensionsTemplate
{
    /// <summary>
    /// Returns the initial content of a persistence sub-project DI extension file,
    /// including the first repository registration.
    /// </summary>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <param name="suffix">
    /// Persistence project suffix (e.g. <c>Persistence.Postgres</c>).
    /// Used to form the full namespace <c>{projectName}.Infrastructure.{suffix}</c>.
    /// </param>
    /// <param name="entityName">Entity name for the first repository (e.g. <c>Order</c>).</param>
    /// <returns>DependencyInjection.cs file content for the persistence sub-project.</returns>
    public static string Generate(string projectName, string suffix, string entityName)
    {
        string implProject = $"{projectName}.Infrastructure.{suffix}";
        string registration =
            $"services.AddScoped<{projectName}.Domain.{entityName}.Repositories.I{entityName}Repository, {implProject}.Repositories.{entityName}Repository>();";

        return $$"""
        namespace {{implProject}};

        using Microsoft.Extensions.DependencyInjection;

        /// <summary>
        /// Registers persistence services into the DI container.
        /// </summary>
        public static class PersistenceExtensions
        {
            /// <summary>
            /// Adds persistence services to <paramref name="services"/>.
            /// </summary>
            /// <param name="services">The service collection.</param>
            /// <returns>The same service collection for chaining.</returns>
            public static IServiceCollection AddPersistence(this IServiceCollection services)
            {
                {{registration}}
                return services;
            }
        }
        """;
    }
}
