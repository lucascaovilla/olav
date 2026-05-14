// <copyright file="ProgramDiInjector.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Infrastructure;

/// <summary>
/// Injects DI registration calls into <c>Program.cs</c> before <c>WebApplication app = builder.Build();</c>.
/// </summary>
public static class ProgramDiInjector
{
    private const string BuildMarker = "WebApplication app = builder.Build();";

    /// <summary>
    /// Inserts <paramref name="diCall"/> before <c>WebApplication app = builder.Build();</c>
    /// in <paramref name="filePath"/>. No-ops when the line is already present or the file does not exist (idempotent).
    /// </summary>
    /// <param name="filePath">Absolute path to <c>Program.cs</c>.</param>
    /// <param name="diCall">
    /// The DI call statement to inject (e.g. <c>builder.Services.AddApplication();</c>).
    /// </param>
    public static void Inject(string filePath, string diCall)
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        string content = File.ReadAllText(filePath);

        if (content.Contains(diCall.Trim()))
        {
            return;
        }

        int markerIndex = content.IndexOf(BuildMarker, StringComparison.Ordinal);
        if (markerIndex < 0)
        {
            return;
        }

        string toInsert = $"{diCall.Trim()}\n        ";
        File.WriteAllText(filePath, content.Insert(markerIndex, toInsert));
    }

    /// <summary>
    /// Inserts <c>using <paramref name="usingNamespace"/>;</c> after the last existing using directive
    /// in <paramref name="filePath"/>. No-ops when the using is already present or the file does not exist (idempotent).
    /// </summary>
    /// <param name="filePath">Absolute path to <c>Program.cs</c>.</param>
    /// <param name="usingNamespace">Namespace to add (e.g. <c>MyApp.Application</c>).</param>
    public static void InjectUsing(string filePath, string usingNamespace)
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        string usingLine = $"using {usingNamespace};";
        string content = File.ReadAllText(filePath);

        if (content.Contains(usingLine))
        {
            return;
        }

        int lastUsingStart = content.LastIndexOf("\nusing ", StringComparison.Ordinal);
        if (lastUsingStart < 0)
        {
            return;
        }

        int endOfLastUsing = content.IndexOf('\n', lastUsingStart + 1);
        if (endOfLastUsing < 0)
        {
            return;
        }

        File.WriteAllText(filePath, content.Insert(endOfLastUsing, $"\n{usingLine}"));
    }
}
