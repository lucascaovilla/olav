// <copyright file="ControllerTemplate.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Templates;

/// <summary>
/// Provides API controller templates (new controller with a single action).
/// </summary>
public static class ControllerTemplate
{
    /// <summary>
    /// Returns a new controller file with a POST action for the given command.
    /// Uses C# primary constructor syntax.
    /// </summary>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <param name="entityName">Entity name (e.g. <c>User</c>).</param>
    /// <param name="commandName">Command name without suffix (e.g. <c>Login</c>).</param>
    /// <param name="handlerFieldName">camelCase parameter name (e.g. <c>loginHandler</c>).</param>
    /// <returns>Controller file content.</returns>
    public static string GenerateForCommand(string projectName, string entityName, string commandName, string handlerFieldName)
    {
        string routeSegment = commandName.ToLowerInvariant();

        return $$"""
        namespace {{projectName}}.Api.Controllers;

        using Microsoft.AspNetCore.Mvc;
        using {{projectName}}.Application.{{entityName}}.Commands.{{commandName}};

        /// <summary>
        /// API controller for <c>{{entityName}}</c> operations.
        /// </summary>
        [ApiController]
        [Route("api/[controller]")]
        public sealed class {{entityName}}Controller(I{{commandName}}CommandHandler {{handlerFieldName}}) : ControllerBase
        {
            /// <summary>Handles the {{commandName}} command.</summary>
            [HttpPost("{{routeSegment}}")]
            public async Task<IActionResult> {{commandName}}([FromBody] {{commandName}}Command command, CancellationToken cancellationToken)
            {
                {{commandName}}CommandResult result = await {{handlerFieldName}}.HandleAsync(command, cancellationToken);
                return Ok(result);
            }
        }
        """;
    }

    /// <summary>
    /// Returns a new controller file with a GET action for the given query.
    /// Uses C# primary constructor syntax.
    /// </summary>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <param name="entityName">Entity name (e.g. <c>User</c>).</param>
    /// <param name="queryName">Query name without suffix (e.g. <c>GetUser</c>).</param>
    /// <param name="handlerFieldName">camelCase parameter name (e.g. <c>getUserHandler</c>).</param>
    /// <returns>Controller file content.</returns>
    public static string GenerateForQuery(string projectName, string entityName, string queryName, string handlerFieldName)
    {
        return $$"""
        namespace {{projectName}}.Api.Controllers;

        using Microsoft.AspNetCore.Mvc;
        using {{projectName}}.Application.{{entityName}}.Queries.{{queryName}};

        /// <summary>
        /// API controller for <c>{{entityName}}</c> operations.
        /// </summary>
        [ApiController]
        [Route("api/[controller]")]
        public sealed class {{entityName}}Controller(I{{queryName}}QueryHandler {{handlerFieldName}}) : ControllerBase
        {
            /// <summary>Handles the {{queryName}} query.</summary>
            [HttpGet("{id:guid}")]
            public async Task<IActionResult> {{queryName}}(Guid id, CancellationToken cancellationToken)
            {
                {{queryName}}QueryResult? result = await {{handlerFieldName}}.HandleAsync(new {{queryName}}Query(id), cancellationToken);
                return result is null ? NotFound() : Ok(result);
            }
        }
        """;
    }
}
