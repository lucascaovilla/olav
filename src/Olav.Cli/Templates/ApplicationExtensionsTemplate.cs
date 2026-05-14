// <copyright file="ApplicationExtensionsTemplate.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Templates;

/// <summary>
/// Provides the Application DI extension method template.
/// </summary>
public static class ApplicationExtensionsTemplate
{
    /// <summary>
    /// Returns the initial content of the Application DI extension file,
    /// including the first service registration line.
    /// </summary>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <param name="serviceName">Service class name for the first registration (e.g. <c>OrderService</c>).</param>
    /// <returns>ApplicationExtensions.cs file content.</returns>
    public static string Generate(string projectName, string serviceName)
    {
        return $$"""
        namespace {{projectName}}.Application;

        using Microsoft.Extensions.DependencyInjection;

        /// <summary>
        /// Registers Application services into the DI container.
        /// </summary>
        public static class ApplicationExtensions
        {
            /// <summary>
            /// Adds Application services to <paramref name="services"/>.
            /// </summary>
            /// <param name="services">The service collection.</param>
            /// <returns>The same service collection for chaining.</returns>
            public static IServiceCollection AddApplication(this IServiceCollection services)
            {
                services.AddScoped<{{projectName}}.Application.Services.I{{serviceName}}, {{projectName}}.Application.Services.{{serviceName}}>();
                return services;
            }
        }
        """;
    }

    /// <summary>
    /// Returns an empty Application DI extension file shell with no pre-filled registrations.
    /// Use <c>DiRegistrationInjector.Inject()</c> to add lines after calling this.
    /// </summary>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <returns>ApplicationExtensions.cs file content with an empty <c>AddApplication</c> method.</returns>
    public static string GenerateEmpty(string projectName)
    {
        return $$"""
        namespace {{projectName}}.Application;

        using Microsoft.Extensions.DependencyInjection;

        /// <summary>
        /// Registers Application services into the DI container.
        /// </summary>
        public static class ApplicationExtensions
        {
            /// <summary>
            /// Adds Application services to <paramref name="services"/>.
            /// </summary>
            /// <param name="services">The service collection.</param>
            /// <returns>The same service collection for chaining.</returns>
            public static IServiceCollection AddApplication(this IServiceCollection services)
            {
                return services;
            }
        }
        """;
    }
}
