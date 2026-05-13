// <copyright file="CorrelationMiddlewareTemplate.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Templates;

/// <summary>
/// Provides CorrelationMiddleware.cs file template.
/// </summary>
public static class CorrelationMiddlewareTemplate
{
    /// <summary>
    /// Returns content of generated correlation middleware.
    /// </summary>
    /// <param name="name">Repository name.</param>
    /// <param name="owner">Repository owner.</param>
    /// <param name="license">Repository license.</param>
    /// <returns>CorrelationMiddleware.cs file content.</returns>
    public static string Generate(string name, string owner, string license)
    {
        return FileHeaderTemplate.Generate("CorrelationMiddleware.cs", owner, license) + $$"""
        namespace {{name}}.Api.Observability;

        using System;
        using System.Diagnostics;
        using System.Threading.Tasks;
        using Microsoft.AspNetCore.Http;

        /// <summary>
        /// Middleware responsible for injecting correlation id into requests.
        /// </summary>
        public class CorrelationMiddleware
        {
            private const string HeaderName = "X-Correlation-Id";

            private readonly RequestDelegate next;

            /// <summary>
            /// Initializes a new instance of the <see cref="CorrelationMiddleware"/> class.
            /// </summary>
            /// <param name="next">Next delegate.</param>
            public CorrelationMiddleware(RequestDelegate next)
            {
                this.next = next;
            }

            /// <summary>
            /// Executes middleware logic.
            /// </summary>
            /// <param name="context">HTTP context.</param>
            /// <returns>Task.</returns>
            public async Task InvokeAsync(HttpContext context)
            {
                string correlationId;

                if (!context.Request.Headers.TryGetValue(HeaderName, out Microsoft.Extensions.Primitives.StringValues headerValue))
                {
                    correlationId = Guid.NewGuid().ToString();
                    context.Request.Headers[HeaderName] = correlationId;
                }
                else
                {
                    correlationId = headerValue.ToString();
                }

                context.Response.Headers[HeaderName] = correlationId;

                using Activity activity = new Activity("Olav.Request");
                activity.SetIdFormat(ActivityIdFormat.W3C);
                activity.Start();
                activity.SetTag("correlation.id", correlationId);

                await this.next(context);
            }
        }
        """;
    }
}
