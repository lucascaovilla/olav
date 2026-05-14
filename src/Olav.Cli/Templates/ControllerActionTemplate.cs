// <copyright file="ControllerActionTemplate.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Templates;

/// <summary>
/// Provides controller action snippets for injection into existing controllers.
/// </summary>
public static class ControllerActionTemplate
{
    /// <summary>
    /// Returns a POST action method snippet for injection into an existing controller.
    /// Indented for placement inside a class body.
    /// </summary>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <param name="entityName">Entity name (e.g. <c>User</c>).</param>
    /// <param name="commandName">Command name without suffix (e.g. <c>Login</c>).</param>
    /// <param name="handlerFieldName">camelCase field/parameter name (e.g. <c>loginHandler</c>).</param>
    /// <returns>Action method snippet (indented, ready for class-body injection).</returns>
    public static string GenerateCommandAction(string projectName, string entityName, string commandName, string handlerFieldName)
    {
        string routeSegment = commandName.ToLowerInvariant();

        return $$"""

            /// <summary>Handles the {{commandName}} command.</summary>
            [HttpPost("{{routeSegment}}")]
            public async Task<IActionResult> {{commandName}}([FromBody] {{commandName}}Command command, CancellationToken cancellationToken)
            {
                {{commandName}}CommandResult result = await {{handlerFieldName}}.HandleAsync(command, cancellationToken);
                return Ok(result);
            }
        """;
    }

    /// <summary>
    /// Returns a GET action method snippet for injection into an existing controller.
    /// Indented for placement inside a class body.
    /// </summary>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <param name="entityName">Entity name (e.g. <c>User</c>).</param>
    /// <param name="queryName">Query name without suffix (e.g. <c>GetUser</c>).</param>
    /// <param name="handlerFieldName">camelCase field/parameter name (e.g. <c>getUserHandler</c>).</param>
    /// <returns>Action method snippet (indented, ready for class-body injection).</returns>
    public static string GenerateQueryAction(string projectName, string entityName, string queryName, string handlerFieldName)
    {
        return $$"""

            /// <summary>Handles the {{queryName}} query.</summary>
            [HttpGet("{id:guid}")]
            public async Task<IActionResult> {{queryName}}(Guid id, CancellationToken cancellationToken)
            {
                {{queryName}}QueryResult? result = await {{handlerFieldName}}.HandleAsync(new {{queryName}}Query(id), cancellationToken);
                return result is null ? NotFound() : Ok(result);
            }
        """;
    }

    /// <summary>
    /// Returns a POST action stub for injection into an existing controller.
    /// Uses <c>throw new NotImplementedException()</c> — wire up the handler manually.
    /// </summary>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <param name="entityName">Entity name (e.g. <c>User</c>).</param>
    /// <param name="commandName">Command name without suffix (e.g. <c>Login</c>).</param>
    /// <returns>Action method stub snippet (indented, ready for class-body injection).</returns>
    public static string GenerateCommandActionStub(string projectName, string entityName, string commandName)
    {
        string routeSegment = commandName.ToLowerInvariant();

        return $$"""

            /// <summary>Handles the {{commandName}} command.</summary>
            [HttpPost("{{routeSegment}}")]
            public async Task<IActionResult> {{commandName}}([FromBody] {{commandName}}Command command, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        """;
    }

    /// <summary>
    /// Returns a GET action stub for injection into an existing controller.
    /// Uses <c>throw new NotImplementedException()</c> — wire up the handler manually.
    /// </summary>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <param name="entityName">Entity name (e.g. <c>User</c>).</param>
    /// <param name="queryName">Query name without suffix (e.g. <c>GetUser</c>).</param>
    /// <returns>Action method stub snippet (indented, ready for class-body injection).</returns>
    public static string GenerateQueryActionStub(string projectName, string entityName, string queryName)
    {
        return $$"""

            /// <summary>Handles the {{queryName}} query.</summary>
            [HttpGet("{id:guid}")]
            public async Task<IActionResult> {{queryName}}(Guid id, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        """;
    }

    /// <summary>
    /// Returns the using directive line for a command namespace.
    /// </summary>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <param name="entityName">Entity name (e.g. <c>User</c>).</param>
    /// <param name="commandName">Command name without suffix (e.g. <c>Login</c>).</param>
    /// <returns>Full using directive string ready to insert into a file.</returns>
    public static string CommandUsingLine(string projectName, string entityName, string commandName)
    {
        return $"using {projectName}.Application.{entityName}.Commands.{commandName};";
    }

    /// <summary>
    /// Returns the using directive line for a query namespace.
    /// </summary>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <param name="entityName">Entity name (e.g. <c>User</c>).</param>
    /// <param name="queryName">Query name without suffix (e.g. <c>GetUser</c>).</param>
    /// <returns>Full using directive string ready to insert into a file.</returns>
    public static string QueryUsingLine(string projectName, string entityName, string queryName)
    {
        return $"using {projectName}.Application.{entityName}.Queries.{queryName};";
    }
}
