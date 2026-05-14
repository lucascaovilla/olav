// <copyright file="Program.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Runtime.CompilerServices;
using Olav.Commands;

[assembly: InternalsVisibleTo("Olav.IntegrationTests")]
[assembly: InternalsVisibleTo("Olav.ArchitectureTests")]
[assembly: InternalsVisibleTo("Olav.UnitTests")]

namespace Olav;

/// <summary>
/// Entry point for the Olav CLI.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Application entry point.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    internal static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintHelp();
            return;
        }

        string command = args[0].ToLowerInvariant();

        switch (command)
        {
            case "new":
                NewCommand.Execute(args);
                break;

            case "lint":
                LintCommand.Execute();
                break;

            case "verify":
                VerifyCommand.Execute();
                break;

            case "migrate":
                MigrateCommand.Execute(args);
                break;

            case "add":
                ExecuteAdd(args);
                break;

            case "make":
                MakeCommand.Execute(args);
                break;

            case "plugin":
                PluginCommand.Execute(args);
                break;

            case "source":
                SourceCommand.Execute(args);
                break;

            default:
                PrintHelp();
                break;
        }
    }

    private static void ExecuteAdd(string[] args)
    {
        string sub = args.Length > 1 ? args[1].ToLowerInvariant() : string.Empty;
        switch (sub)
        {
            case "infrastructure":
                AddInfrastructureCommand.Execute(args);
                break;
            case "deployment":
                AddDeploymentCommand.Execute(args);
                break;
            case "entity":
                AddEntityCommand.Execute(args);
                break;
            case "enum":
                AddEnumCommand.Execute(args);
                break;
            case "repository":
                AddRepositoryCommand.Execute(args);
                break;
            case "service":
                AddServiceCommand.Execute(args);
                break;
            case "command":
                AddCommandCommand.Execute(args);
                break;
            case "query":
                AddQueryCommand.Execute(args);
                break;
            default:
                Console.WriteLine("Usage: olav add infrastructure|deployment|entity|enum|repository|service|command|query <name>");
                break;
        }
    }

    private static void PrintHelp()
    {
        Console.WriteLine(
        """
        Olav CLI

        Usage:
          olav new <ProjectName>
          olav new plugin <id> [--category <value>] [--delivery <value>] [--author <value>]
          olav lint
          olav verify
          olav migrate [--apply]
          olav add infrastructure <source> [--param value …]
          olav add deployment <source> [--param value …]
          olav add entity <EntityName>
          olav add enum <EntityName> <EnumName>
          olav add repository <EntityName> [<plugin>]
          olav add service <ServiceName> [--entity <EntityName>] [application|infrastructure]
          olav add command <EntityName> <CommandName> [--with-handler] [--with-endpoint]
          olav add query <EntityName> <QueryName> [--with-handler] [--with-endpoint]
          olav make migration <plugin> <MigrationName>
          olav plugin list
          olav plugin remove <id>
          olav source add <alias> <url>
          olav source remove <alias>
          olav source list
        """);
    }
}
