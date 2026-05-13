// <copyright file="DockerComposeTemplate.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Templates;

/// <summary>
/// Provides docker-compose files templates.
/// </summary>
public static class DockerComposeTemplate
{
    /// <summary>
    /// Returns content of generated production docker compose.
    /// </summary>
    /// <param name="name">Repository name.</param>
    /// <returns>Content of production docker compose.</returns>
    public static string GeneratePrd(string name)
    {
        return $$"""
        services:
          web:
            image: {{name.ToLowerInvariant()}}-web:${IMAGE_TAG}
            ports:
              - "8080:8080"
            env_file: ../.env
            environment:
              - ASPNETCORE_ENVIRONMENT=Production
        """;
    }

    /// <summary>
    /// Returns content of generated staging docker compose.
    /// </summary>
    /// <param name="name">Repository name.</param>
    /// <returns>Content of staging docker compose.</returns>
    public static string GenerateStaging(string name)
    {
        return $$"""
        services:
          web:
            image: {{name.ToLowerInvariant()}}-web:${IMAGE_TAG}
            ports:
              - "8081:8080"
            env_file: ../.env
            environment:
              - ASPNETCORE_ENVIRONMENT=Staging
        """;
    }

    /// <summary>
    /// Returns content of generated local docker compose.
    /// Contains only the web service — infrastructure services are added by plugins.
    /// </summary>
    /// <param name="name">Repository name.</param>
    /// <returns>Content of local docker compose.</returns>
    public static string GenerateLocal(string name)
    {
        _ = name;
        return """
        services:
          web:
            build:
              context: ..
              dockerfile: docker/Dockerfile
            ports:
              - "8080:8080"
            env_file: ../.env
            environment:
              - ASPNETCORE_ENVIRONMENT=Development
              - ASPNETCORE_URLS=http://+:8080
        """;
    }
}
