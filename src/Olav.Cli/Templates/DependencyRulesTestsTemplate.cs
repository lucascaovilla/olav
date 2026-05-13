// <copyright file="DependencyRulesTestsTemplate.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Templates;

/// <summary>
/// Provides DependencyRulesTests.cs file template.
/// </summary>
public static class DependencyRulesTestsTemplate
{
    /// <summary>
    /// Returns content of generated dependency rules test.
    /// </summary>
    /// <param name="name">Repository name.</param>
    /// <param name="owner">Repository owner.</param>
    /// <param name="license">Repository license.</param>
    /// <returns>DependencyRulesTests.cs file content.</returns>
    public static string Generate(string name, string owner, string license)
    {
        return FileHeaderTemplate.Generate("DependencyRulesTests.cs", owner, license) + $$"""
        namespace {{name}}.ArchitectureTests;

        using System.Reflection;
        using NetArchTest.Rules;
        using Xunit;

        /// <summary>
        /// Enforces Clean Architecture dependency rules.
        /// </summary>
        public class DependencyRulesTests
        {
            private static readonly Assembly DomainAssembly =
                Assembly.Load("{{name}}.Domain");

            private static readonly Assembly ApplicationAssembly =
                Assembly.Load("{{name}}.Application");

            private static readonly Assembly InfrastructureAssembly =
                Assembly.Load("{{name}}.Infrastructure");

            /// <summary>
            /// Domain must not depend on outer layers.
            /// </summary>
            [Fact]
            public void Domain_Should_Not_Depend_On_Other_Layers()
            {
                bool result = Types.InAssembly(DomainAssembly)
                    .Should()
                    .NotHaveDependencyOn("{{name}}.Application")
                    .And()
                    .NotHaveDependencyOn("{{name}}.Infrastructure")
                    .And()
                    .NotHaveDependencyOn("{{name}}.Api")
                    .GetResult()
                    .IsSuccessful;

                Assert.True(result);
            }

            /// <summary>
            /// Application must not depend on Api.
            /// </summary>
            [Fact]
            public void Application_Should_Not_Depend_On_Api()
            {
                bool result = Types.InAssembly(ApplicationAssembly)
                    .Should()
                    .NotHaveDependencyOn("{{name}}.Api")
                    .GetResult()
                    .IsSuccessful;

                Assert.True(result);
            }

            /// <summary>
            /// Infrastructure must not depend on Api.
            /// </summary>
            [Fact]
            public void Infrastructure_Should_Not_Depend_On_Api()
            {
                bool result = Types.InAssembly(InfrastructureAssembly)
                    .Should()
                    .NotHaveDependencyOn("{{name}}.Api")
                    .GetResult()
                    .IsSuccessful;

                Assert.True(result);
            }
        }
        """;
    }
}
