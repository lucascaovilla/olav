// <copyright file="DockerfileTemplate.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Templates;

/// <summary>
/// Provides Dockerfile file template.
/// </summary>
public static class DockerfileTemplate
{
    /// <summary>
    /// Returns content of generated Dockerfile.
    /// </summary>
    /// <param name="name">Repository name.</param>
    /// <returns>Dockerfile file content.</returns>
    public static string Generate(string name)
    {
        return $"""
        FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
        WORKDIR /src

        COPY . .
        RUN dotnet restore src/{name}.Api/{name}.Api.csproj
        RUN dotnet publish src/{name}.Api/{name}.Api.csproj -c Release -o /app/publish

        FROM mcr.microsoft.com/dotnet/aspnet:10.0
        WORKDIR /app
        COPY --from=build /app/publish .
        ENV ASPNETCORE_URLS=http://+:8080
        EXPOSE 8080
        ENTRYPOINT ["dotnet", "{name}.Api.dll"]
        """;
    }
}
