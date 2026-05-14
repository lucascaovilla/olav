// <copyright file="AddQueryCommand.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Commands;

using Olav.Generation;
using Olav.Helpers;

/// <summary>
/// Handles <c>olav add query &lt;EntityName&gt; &lt;QueryName&gt; [--with-handler] [--with-endpoint]</c>.
/// Generates a CQRS query record and optional handler + API endpoint.
/// </summary>
public static class AddQueryCommand
{
    /// <summary>
    /// Executes the <c>add query</c> command.
    /// </summary>
    /// <param name="args">Full argument list (including the leading <c>add query</c> tokens).</param>
    public static void Execute(string[] args)
    {
        if (args.Length < 4)
        {
            Console.WriteLine("Usage: olav add query <EntityName> <QueryName> [--with-handler] [--with-endpoint]");
            return;
        }

        string entityName = args[2];
        string queryName = args[3];
        bool withHandler = HasFlag(args, "--with-handler") || HasFlag(args, "--with-endpoint");
        bool withEndpoint = HasFlag(args, "--with-endpoint");

        if (withEndpoint && !HasFlag(args, "--with-handler"))
        {
            Console.WriteLine("Note: --with-endpoint implies --with-handler. Handler will also be generated.");
        }

        string root = ProjectRootHelper.FindProjectRoot(Directory.GetCurrentDirectory());

        try
        {
            string projectName = ProjectNameHelper.DiscoverProjectName(root);
            new CqrsQueryGenerator(entityName, queryName, projectName, root, withHandler, withEndpoint).Generate();
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private static bool HasFlag(string[] args, string flag)
    {
        return args.Any(a => string.Equals(a, flag, StringComparison.OrdinalIgnoreCase));
    }
}
