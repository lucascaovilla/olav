// <copyright file="ObservabilityExtensionsTemplate.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Templates;

/// <summary>
/// Provides ObservabilityExtensions file template.
/// </summary>
public static class ObservabilityExtensionsTemplate
{
    /// <summary>
    /// Returns content of generated observability extensions.
    /// </summary>
    /// <param name="name">Repository name.</param>
    /// <param name="owner">Repository owner.</param>
    /// <param name="license">Repository license.</param>
    /// <returns>ObservabilityExtensions file content.</returns>
    public static string Generate(string name, string owner, string license)
    {
        return FileHeaderTemplate.Generate("ObservabilityExtensions.cs", owner, license) + $$"""
        namespace {{name}}.Api.Observability;

        using System;
        using Microsoft.AspNetCore.Builder;
        using Microsoft.Extensions.DependencyInjection;
        using OpenTelemetry.Trace;
        using Serilog;

        /// <summary>
        /// Provides observability configuration extensions.
        /// </summary>
        public static class ObservabilityExtensions
        {
            private static bool configured;
            private static bool middlewareApplied;

            /// <summary>
            /// Adds mandatory Olav observability configuration.
            /// </summary>
            /// <param name="services">Service collection.</param>
            /// <returns>Updated service collection.</returns>
            public static IServiceCollection AddOlavObservability(this IServiceCollection services)
            {
                Log.Logger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .CreateLogger();

                services.AddLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.AddSerilog();
                });

                services.AddOpenTelemetry()
                    .WithTracing(builder =>
                    {
                        builder
                            .AddAspNetCoreInstrumentation()
                            .AddConsoleExporter();
                    });

                configured = true;

                return services;
            }

            /// <summary>
            /// Enables mandatory middleware pipeline.
            /// </summary>
            /// <param name="app">Application builder.</param>
            /// <returns>Updated application builder.</returns>
            public static IApplicationBuilder UseOlavObservability(this IApplicationBuilder app)
            {
                if (!configured)
                {
                    throw new InvalidOperationException(
                        "Olav Observability not configured. Call AddOlavObservability().");
                }

                app.UseMiddleware<CorrelationMiddleware>();

                middlewareApplied = true;

                return app;
            }

            /// <summary>
            /// Verifies that Olav observability was fully configured.
            /// </summary>
            public static void EnsureOlavCompliance()
            {
                if (!configured || !middlewareApplied)
                {
                    throw new InvalidOperationException(
                        "Olav observability not fully configured. " +
                        "Ensure AddOlavObservability() and UseOlavObservability() are called.");
                }
            }
        }
        """;
    }
}
