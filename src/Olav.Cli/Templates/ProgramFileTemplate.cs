// <copyright file="ProgramFileTemplate.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Templates;

/// <summary>
/// Provides Program.cs file template.
/// </summary>
public static class ProgramFileTemplate
{
    /// <summary>
    /// Returns content of generated Program.cs.
    /// </summary>
    /// <param name="name">Repository name.</param>
    /// <param name="owner">Repository owner.</param>
    /// <param name="license">Repository license.</param>
    /// <returns>Program.cs file content.</returns>
    public static string Generate(string name, string owner, string license)
    {
        return FileHeaderTemplate.Generate("Program.cs", owner, license) + $$"""
        namespace {{name}}.Api;

        using Microsoft.AspNetCore.Builder;
        using Microsoft.Extensions.DependencyInjection;
        using Microsoft.Extensions.Hosting;
        using {{name}}.Api.Observability;

        /// <summary>
        /// Entry point.
        /// </summary>
        public static class Program
        {
            /// <summary>
            /// Main method.
            /// </summary>
            /// <param name="args">Arguments.</param>
            public static void Main(string[] args)
            {
                WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

                builder.Services.AddControllers();
                builder.Services.AddOlavObservability();

                WebApplication app = builder.Build();

                app.UseOlavObservability();
                app.UseAuthorization();
                app.MapControllers();
                ObservabilityExtensions.EnsureOlavCompliance();

                app.Run();
            }
        }
        """;
    }
}
