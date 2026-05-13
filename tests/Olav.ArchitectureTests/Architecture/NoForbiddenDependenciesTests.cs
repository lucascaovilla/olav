using NetArchTest.Rules;
using Xunit;
using System.Linq;
using Olav.Testing.Extensions;

namespace Olav.ArchitectureTests.Architecture;

public class NoForbiddenDependenciesTests
{
    private const string BaseNamespace = "Olav";

    [Fact]
    public void Only_Generation_Should_Use_System_IO()
    {
        Types.InAssembly(typeof(Program).Assembly)
            .That()
            .DoNotResideInNamespace("Olav.Generation")
            .And()
            .DoNotResideInNamespace("Olav.Infrastructure")
            .And()
            .DoNotResideInNamespace("Olav.Commands")
            .And()
            .DoNotResideInNamespace("Olav.Verifiers")
            .And()
            .DoNotResideInNamespace("Olav.Helpers")
            .ShouldNot()
            .HaveDependencyOn("System.IO")
            .GetResult()
            .AssertSuccessful("Types using System.IO");
    }

    [Fact]
    public void Only_Infrastructure_Should_Use_System_Diagnostics()
    {
        // Olav.Generation is also exempt: C# records in that namespace emit
        // compiler-generated DebuggerBrowsableAttribute references implicitly.
        Types.InAssembly(typeof(Olav.Program).Assembly)
            .That()
            .DoNotResideInNamespace($"{BaseNamespace}.Infrastructure")
            .And()
            .DoNotResideInNamespace($"{BaseNamespace}.Generation")
            .ShouldNot()
            .HaveDependencyOn("System.Diagnostics")
            .GetResult()
            .AssertSuccessful("Infrastructures that don't use System.Diagnostics");
    }
}
