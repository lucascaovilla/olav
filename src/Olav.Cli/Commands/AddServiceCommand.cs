// <copyright file="AddServiceCommand.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Commands;

using Olav.Generation;
using Olav.Helpers;

/// <summary>
/// Handles <c>olav add service &lt;EntityName&gt; &lt;ServiceName&gt; [application|infrastructure]</c>.
/// Interface always goes to Application. Implementation goes to the specified layer (default: application).
/// </summary>
public static class AddServiceCommand
{
    /// <summary>
    /// Executes the <c>add service</c> command.
    /// </summary>
    /// <param name="args">Full argument list (including the leading <c>add service</c> tokens).</param>
    public static void Execute(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: olav add service <ServiceName> [--entity <EntityName>] [application|infrastructure]");
            return;
        }

        string serviceName = args[2];
        string? entityName = ParseFlag(args, "--entity");
        string? layerArg = null;
        for (int i = 3; i < args.Length; i++)
        {
            if (!args[i].StartsWith("--", StringComparison.Ordinal) && args[i - 1] != "--entity")
            {
                layerArg = args[i];
                break;
            }
        }

        ServiceGenerator.ServiceLayer layer = ParseLayer(layerArg);
        string root = ProjectRootHelper.FindProjectRoot(Directory.GetCurrentDirectory());
        string? plugin = InstalledPluginHelper.ResolveInfrastructurePlugin(root);

        try
        {
            string projectName = ProjectNameHelper.DiscoverProjectName(root);
            new ServiceGenerator(entityName, serviceName, projectName, root, layer, plugin).Generate();
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private static string? ParseFlag(string[] args, string flag)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == flag)
            {
                return args[i + 1];
            }
        }

        return null;
    }

    private static ServiceGenerator.ServiceLayer ParseLayer(string? value)
    {
        if (string.Equals(value, "infrastructure", StringComparison.OrdinalIgnoreCase))
        {
            return ServiceGenerator.ServiceLayer.Infrastructure;
        }

        return ServiceGenerator.ServiceLayer.Application;
    }
}
