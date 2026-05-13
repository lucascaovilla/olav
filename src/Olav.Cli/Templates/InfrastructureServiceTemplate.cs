// <copyright file="InfrastructureServiceTemplate.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Templates;

/// <summary>
/// Provides the Infrastructure service implementation template.
/// Used when the service is an external-integration adapter whose interface
/// is defined in Application but whose implementation lives in Infrastructure.
/// </summary>
public static class InfrastructureServiceTemplate
{
    /// <summary>
    /// Returns the content of an Infrastructure service implementation.
    /// The class implements the Application-layer interface.
    /// </summary>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <param name="entityName">Entity folder name (e.g. <c>Email</c>).</param>
    /// <param name="serviceName">Service class name (e.g. <c>EmailService</c>).</param>
    /// <returns>Infrastructure service implementation file content.</returns>
    public static string Generate(string projectName, string entityName, string serviceName)
    {
        return $$"""
        namespace {{projectName}}.Infrastructure.Services.{{entityName}};

        using {{projectName}}.Application.Services.{{entityName}};

        /// <summary>
        /// Infrastructure implementation of <see cref="I{{serviceName}}"/>.
        /// </summary>
        public class {{serviceName}} : I{{serviceName}}
        {
        }
        """;
    }
}
