// <copyright file="ControllerActionInjector.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Infrastructure;

/// <summary>
/// Injects action method snippets and using directives into existing API controller files.
/// </summary>
public static class ControllerActionInjector
{
    /// <summary>
    /// Inserts <paramref name="actionSnippet"/> before the last <c>}</c> in
    /// <paramref name="filePath"/> (the class closing brace). No-ops if an action
    /// with the same method name is already present (idempotent).
    /// Also injects <paramref name="usingLine"/> after the last existing using directive
    /// if not already present.
    /// </summary>
    /// <param name="filePath">Absolute path to the controller file.</param>
    /// <param name="actionMethodName">
    /// Public method name used for idempotency check (e.g. <c>Login</c>).
    /// </param>
    /// <param name="actionSnippet">Action method text to inject (already indented).</param>
    /// <param name="usingLine">Full using directive line (e.g. <c>using MyApp.Application.User.Commands.Login;</c>).</param>
    public static void Inject(string filePath, string actionMethodName, string actionSnippet, string usingLine)
    {
        string content = File.ReadAllText(filePath);

        string idempotencyMarker = $"public async Task<IActionResult> {actionMethodName}(";
        if (content.Contains(idempotencyMarker))
        {
            return;
        }

        content = InjectUsing(content, usingLine);
        content = InjectAction(content, actionSnippet);

        File.WriteAllText(filePath, content);
    }

    private static string InjectUsing(string content, string usingLine)
    {
        if (content.Contains(usingLine.Trim()))
        {
            return content;
        }

        int lastUsingIndex = content.LastIndexOf("\nusing ", StringComparison.Ordinal);
        if (lastUsingIndex >= 0)
        {
            int endOfLine = content.IndexOf('\n', lastUsingIndex + 1);
            if (endOfLine >= 0)
            {
                return content.Insert(endOfLine + 1, usingLine.Trim() + "\n");
            }
        }

        return content;
    }

    private static string InjectAction(string content, string actionSnippet)
    {
        int lastBrace = content.LastIndexOf('}');
        if (lastBrace < 0)
        {
            return content;
        }

        return content.Insert(lastBrace, actionSnippet + "\n");
    }
}
