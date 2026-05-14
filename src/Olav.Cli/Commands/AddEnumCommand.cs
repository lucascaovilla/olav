// <copyright file="AddEnumCommand.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Commands;

using Olav.Generation;
using Olav.Helpers;

/// <summary>
/// Handles <c>olav add enum &lt;EnumName&gt;</c>.
/// Generates a Domain enum.
/// </summary>
public static class AddEnumCommand
{
    /// <summary>
    /// Executes the <c>add enum</c> command.
    /// </summary>
    /// <param name="args">Full argument list (including the leading <c>add enum</c> tokens).</param>
    public static void Execute(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: olav add enum <EnumName>");
            return;
        }

        string enumName = args[2];
        string root = ProjectRootHelper.FindProjectRoot(Directory.GetCurrentDirectory());

        try
        {
            string projectName = ProjectNameHelper.DiscoverProjectName(root);
            new DomainEnumGenerator(enumName, projectName, root).Generate();
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
