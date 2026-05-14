// <copyright file="MakeMigrationCommand.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Commands;

using Olav.Generation;
using Olav.Helpers;

/// <summary>
/// Handles <c>olav make migration &lt;plugin&gt; &lt;MigrationName&gt;</c>.
/// Runs <c>dotnet ef migrations add</c> targeting the plugin's persistence project.
/// </summary>
public static class MakeMigrationCommand
{
    /// <summary>
    /// Executes the <c>make migration</c> command.
    /// </summary>
    /// <param name="args">Full argument list (including the leading <c>make migration</c> tokens).</param>
    public static void Execute(string[] args)
    {
        if (args.Length < 4)
        {
            Console.WriteLine("Usage: olav make migration <plugin> <MigrationName>");
            Console.WriteLine("       Example: olav make migration postgres InitialCreate");
            return;
        }

        string plugin = args[2];
        string migrationName = args[3];
        string root = ProjectRootHelper.FindProjectRoot(Directory.GetCurrentDirectory());

        try
        {
            new EfMigrationGenerator(plugin, migrationName, root).Generate();
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
