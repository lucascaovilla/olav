using System;
using System.IO;
using Olav.Infrastructure;
using Xunit;

namespace Olav.UnitTests.Infrastructure;

public class DiRegistrationInjectorTests
{
    private static string BuildDiFile(string registration = "") =>
        $$"""
        namespace MyApp.Infrastructure;

        using Microsoft.Extensions.DependencyInjection;

        public static class InfrastructureExtensions
        {
            public static IServiceCollection AddInfrastructure(this IServiceCollection services)
            {
                {{registration}}
                return services;
            }
        }
        """;

    [Fact]
    public void Inject_InsertsRegistration_WithCorrectIndent()
    {
        string path = Path.GetTempFileName();
        File.WriteAllText(path, BuildDiFile());

        string registration = "services.AddScoped<IFoo, Foo>();";
        DiRegistrationInjector.Inject(path, registration);

        string result = File.ReadAllText(path);
        int lineIndex = result.IndexOf(registration, StringComparison.Ordinal);
        int lineStart = result.LastIndexOf('\n', lineIndex) + 1;
        int indentCount = lineIndex - lineStart;

        int returnIndex = result.IndexOf("return services;", StringComparison.Ordinal);
        int returnLineStart = result.LastIndexOf('\n', returnIndex) + 1;
        int returnIndentCount = returnIndex - returnLineStart;

        Assert.Equal(returnIndentCount, indentCount);
    }

    [Fact]
    public void Inject_IsIdempotent_WhenCalledTwice()
    {
        string path = Path.GetTempFileName();
        File.WriteAllText(path, BuildDiFile());

        string registration = "services.AddScoped<IFoo, Foo>();";
        DiRegistrationInjector.Inject(path, registration);
        DiRegistrationInjector.Inject(path, registration);

        string result = File.ReadAllText(path);
        int count = 0;
        int idx = 0;
        while ((idx = result.IndexOf(registration, idx, StringComparison.Ordinal)) >= 0)
        {
            count++;
            idx++;
        }

        Assert.Equal(1, count);
    }

    [Fact]
    public void Inject_AppendsMultipleRegistrations_InOrder()
    {
        string path = Path.GetTempFileName();
        File.WriteAllText(path, BuildDiFile());

        DiRegistrationInjector.Inject(path, "services.AddScoped<IFoo, Foo>();");
        DiRegistrationInjector.Inject(path, "services.AddScoped<IBar, Bar>();");

        string result = File.ReadAllText(path);
        Assert.Contains("IFoo", result);
        Assert.Contains("IBar", result);
        Assert.True(result.IndexOf("return services;", StringComparison.Ordinal) > result.IndexOf("IBar", StringComparison.Ordinal));
    }

    [Fact]
    public void Inject_NoOp_WhenMarkerNotFound()
    {
        string path = Path.GetTempFileName();
        string original = "namespace Foo; public class Bar { }";
        File.WriteAllText(path, original);

        DiRegistrationInjector.Inject(path, "services.AddScoped<IFoo, Foo>();");

        Assert.Equal(original, File.ReadAllText(path));
    }
}
