// <copyright file="DiRegistrationInjector.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Infrastructure;

/// <summary>
/// Injects <c>services.AddScoped&lt;,&gt;()</c> lines into an existing DI extension file.
/// </summary>
public static class DiRegistrationInjector
{
    private const string ReturnMarker = "return services;";

    /// <summary>
    /// Inserts <paramref name="registrationLine"/> before <c>return services;</c> in
    /// <paramref name="filePath"/>. No-ops if the line is already present (idempotent).
    /// </summary>
    /// <param name="filePath">Absolute path to the DI extension file.</param>
    /// <param name="registrationLine">
    /// The full registration statement to inject (e.g.
    /// <c>services.AddScoped&lt;IFoo, Foo&gt;();</c>).
    /// </param>
    public static void Inject(string filePath, string registrationLine)
    {
        string content = File.ReadAllText(filePath);

        if (content.Contains(registrationLine.Trim()))
        {
            return;
        }

        int markerIndex = content.LastIndexOf(ReturnMarker, StringComparison.Ordinal);
        if (markerIndex < 0)
        {
            return;
        }

        int lineStart = content.LastIndexOf('\n', markerIndex) + 1;
        string lineIndent = content[lineStart..markerIndex];
        string injected = $"{lineIndent}{registrationLine.Trim()}\n";
        File.WriteAllText(filePath, content.Insert(lineStart, injected));
    }
}
