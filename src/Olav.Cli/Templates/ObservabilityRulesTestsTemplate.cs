// <copyright file="ObservabilityRulesTestsTemplate.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Templates;

/// <summary>
/// Provides ObservabilityRulesTests.cs file template.
/// </summary>
public static class ObservabilityRulesTestsTemplate
{
    /// <summary>
    /// Returns content of generated observability rules test.
    /// </summary>
    /// <param name="name">Repository name.</param>
    /// <param name="owner">Repository owner.</param>
    /// <param name="license">Repository license.</param>
    /// <returns>ObservabilityRulesTests.cs file content.</returns>
    public static string Generate(string name, string owner, string license)
    {
        return FileHeaderTemplate.Generate("ObservabilityRulesTests.cs", owner, license) + $$"""
        namespace {{name}}.ArchitectureTests;

        using System;
        using System.IO;
        using System.Linq;
        using System.Reflection;
        using Xunit;

        /// <summary>
        /// Enforces mandatory Olav observability rules.
        /// </summary>
        public class ObservabilityRulesTests
        {
            /// <summary>
            /// Ensures Observability namespace exists.
            /// </summary>
            [Fact]
            public void Observability_Namespace_Must_Exist()
            {
                Assembly webAssembly = Assembly.Load("{{name}}.Api");

                bool exists = webAssembly
                    .GetTypes()
                    .Any(t => t.Namespace != null &&
                            t.Namespace.Contains(".Api.Observability"));

                Assert.True(exists);
            }

            /// <summary>
            /// Ensures Program.cs configures observability.
            /// </summary>
            [Fact]
            public void Program_Must_Configure_Observability()
            {
                string baseDirectory = AppContext.BaseDirectory;

                DirectoryInfo? directory = new DirectoryInfo(baseDirectory);

                while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, "src")))
                {
                    directory = directory.Parent;
                }

                if (directory is null)
                {
                    throw new InvalidOperationException("Could not locate solution root.");
                }

                string programPath = Path.Combine(
                    directory.FullName,
                    "src",
                    "{{name}}.Api",
                    "Program.cs");

                Assert.True(File.Exists(programPath));

                string content = File.ReadAllText(programPath);

                Assert.Contains("AddOlavObservability", content);
                Assert.Contains("UseOlavObservability", content);
                Assert.Contains("EnsureOlavCompliance", content);
            }
        }
        """;
    }
}
