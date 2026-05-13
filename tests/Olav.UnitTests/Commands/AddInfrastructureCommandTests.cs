// <copyright file="AddInfrastructureCommandTests.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.UnitTests.Commands;

using System;
using System.Collections.Generic;
using System.IO;
using Olav.Commands;
using Olav.Generation;
using Xunit;

/// <summary>
/// Unit tests for <see cref="AddInfrastructureCommand.PromptMissingParameters"/>.
/// </summary>
public class AddInfrastructureCommandTests
{
    [Fact]
    public void PromptMissingParameters_PromptsForRequiredParams()
    {
        PluginInfoResult info = new PluginInfoResult
        {
            Id = "test",
            Version = "1.0.0",
            DisplayName = "Test",
            Category = "infrastructure",
            Delivery = "package",
            Parameters = new List<PluginParameterDefinition>
            {
                new PluginParameterDefinition { Name = "appName", Type = "string", Default = null },
            },
        };
        Dictionary<string, string> parameters = new Dictionary<string, string>();

        TextReader original = Console.In;
        try
        {
            Console.SetIn(new StringReader("my-app\n"));
            AddInfrastructureCommand.PromptMissingParameters(info, parameters);
        }
        finally
        {
            Console.SetIn(original);
        }

        Assert.Equal("my-app", parameters["appName"]);
    }

    [Fact]
    public void PromptMissingParameters_SkipsParamsWithDefaults()
    {
        PluginInfoResult info = new PluginInfoResult
        {
            Id = "test",
            Version = "1.0.0",
            DisplayName = "Test",
            Category = "infrastructure",
            Delivery = "package",
            Parameters = new List<PluginParameterDefinition>
            {
                new PluginParameterDefinition { Name = "connStr", Type = "string", Default = "DefaultConnection" },
            },
        };
        Dictionary<string, string> parameters = new Dictionary<string, string>();

        AddInfrastructureCommand.PromptMissingParameters(info, parameters);

        Assert.False(parameters.ContainsKey("connStr"));
    }

    [Fact]
    public void PromptMissingParameters_SkipsParamsAlreadyProvided()
    {
        PluginInfoResult info = new PluginInfoResult
        {
            Id = "test",
            Version = "1.0.0",
            DisplayName = "Test",
            Category = "infrastructure",
            Delivery = "package",
            Parameters = new List<PluginParameterDefinition>
            {
                new PluginParameterDefinition { Name = "appName", Type = "string", Default = null },
            },
        };
        Dictionary<string, string> parameters = new Dictionary<string, string> { { "appName", "pre-set" } };

        AddInfrastructureCommand.PromptMissingParameters(info, parameters);

        Assert.Equal("pre-set", parameters["appName"]);
    }
}
