// <copyright file="InfrastructureExtensionsTemplate.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Templates;

/// <summary>
/// Provides the Infrastructure DI extension method template.
/// </summary>
public static class InfrastructureExtensionsTemplate
{
    /// <summary>
    /// Returns the initial content of the Infrastructure DI extension file,
    /// including the first repository registration line.
    /// </summary>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <param name="entityName">Entity name for the first repository (e.g. <c>Order</c>).</param>
    /// <returns>InfrastructureExtensions.cs file content.</returns>
    public static string Generate(string projectName, string entityName)
    {
        string registration =
            $"services.AddScoped<{projectName}.Domain.{entityName}.Repositories.I{entityName}Repository, {projectName}.Infrastructure.Repositories.{entityName}Repository>();";

        return GenerateFile(projectName, registration);
    }

    /// <summary>
    /// Returns the initial content of the Infrastructure DI extension file,
    /// including the first infrastructure service registration line.
    /// </summary>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <param name="serviceName">Service class name (e.g. <c>EmailService</c>).</param>
    /// <returns>InfrastructureExtensions.cs file content.</returns>
    public static string GenerateForService(string projectName, string serviceName)
    {
        string registration =
            $"services.AddScoped<{projectName}.Application.Services.I{serviceName}, {projectName}.Infrastructure.Services.{serviceName}>();";

        return GenerateFile(projectName, registration);
    }

    private static string GenerateFile(string projectName, string firstRegistration)
    {
        return $$"""
        namespace {{projectName}}.Infrastructure;

        using Microsoft.Extensions.DependencyInjection;

        /// <summary>
        /// Registers Infrastructure services into the DI container.
        /// </summary>
        public static class InfrastructureExtensions
        {
            /// <summary>
            /// Adds Infrastructure services to <paramref name="services"/>.
            /// </summary>
            /// <param name="services">The service collection.</param>
            /// <returns>The same service collection for chaining.</returns>
            public static IServiceCollection AddInfrastructure(this IServiceCollection services)
            {
                {{firstRegistration}}
                return services;
            }
        }
        """;
    }
}
