// <copyright file="MakeCommand.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Commands;

/// <summary>
/// Routes <c>olav make &lt;sub-command&gt;</c> to the appropriate handler.
/// Currently supports: <c>migration</c>.
/// </summary>
public static class MakeCommand
{
    /// <summary>
    /// Executes a <c>make</c> sub-command.
    /// </summary>
    /// <param name="args">Full argument list (including the leading <c>make</c> token).</param>
    public static void Execute(string[] args)
    {
        string sub = args.Length > 1 ? args[1].ToLowerInvariant() : string.Empty;

        switch (sub)
        {
            case "migration":
                MakeMigrationCommand.Execute(args);
                break;

            default:
                Console.WriteLine("Usage:");
                Console.WriteLine("  olav make migration <plugin> <MigrationName>");
                break;
        }
    }
}
