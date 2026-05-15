// <copyright file="RepositoryTemplate.cs" company="Olav">
// Copyright (c) Olav.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
namespace Olav.Templates;

/// <summary>
/// Provides Infrastructure repository implementation templates, varying by installed plugin.
/// </summary>
public static class RepositoryTemplate
{
    /// <summary>
    /// Returns the content of an Infrastructure repository implementation.
    /// When <paramref name="plugin"/> is <c>postgres</c> or <c>sqlserver</c> an EF Core body is
    /// emitted. When it is <c>redis</c> a StackExchange.Redis body is emitted. Otherwise a stub
    /// body with <see cref="NotImplementedException"/> is emitted.
    /// </summary>
    /// <param name="projectName">Project namespace (e.g. <c>MyApp</c>).</param>
    /// <param name="entityName">Entity name the repository is for (e.g. <c>Order</c>).</param>
    /// <param name="plugin">
    /// Installed plugin id driving the implementation style, or <see langword="null"/> for a stub.
    /// </param>
    /// <param name="implProjectOverride">
    /// Full project namespace to use for the implementation (e.g. <c>MyApp.Infrastructure.Persistence.Postgres</c>).
    /// When <see langword="null"/>, defaults to <c>{projectName}.Infrastructure</c>.
    /// </param>
    /// <returns>Repository implementation file content.</returns>
    public static string Generate(string projectName, string entityName, string? plugin, string? implProjectOverride = null)
    {
        string implProject = implProjectOverride ?? $"{projectName}.Infrastructure";
        return plugin?.ToLowerInvariant() switch
        {
            "postgres" or "sqlserver" => GenerateEfCore(projectName, entityName, implProject),
            "redis" => GenerateRedis(projectName, entityName, implProject),
            _ => GenerateStub(projectName, entityName),
        };
    }

    private static string GenerateEfCore(string projectName, string entityName, string implProject)
    {
        return $$"""
        namespace {{implProject}}.Repositories;

        using Microsoft.EntityFrameworkCore;
        using {{projectName}}.Domain.{{entityName}}.Entities;
        using {{projectName}}.Domain.{{entityName}}.Repositories;

        /// <summary>
        /// EF Core implementation of <see cref="I{{entityName}}Repository"/>.
        /// </summary>
        public class {{entityName}}Repository : I{{entityName}}Repository
        {
            private readonly {{projectName}}DbContext context;

            /// <summary>
            /// Initializes a new instance of the <see cref="{{entityName}}Repository"/> class.
            /// </summary>
            /// <param name="context">The EF Core database context.</param>
            public {{entityName}}Repository({{projectName}}DbContext context)
            {
                this.context = context;
            }

            /// <inheritdoc/>
            public async Task<{{entityName}}?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            {
                return await this.context.Set<{{entityName}}>().FindAsync([id], cancellationToken);
            }

            /// <inheritdoc/>
            public async Task<IEnumerable<{{entityName}}>> GetAllAsync(CancellationToken cancellationToken = default)
            {
                return await this.context.Set<{{entityName}}>().ToListAsync(cancellationToken);
            }

            /// <inheritdoc/>
            public async Task AddAsync({{entityName}} entity, CancellationToken cancellationToken = default)
            {
                this.context.Set<{{entityName}}>().Add(entity);
                await this.context.SaveChangesAsync(cancellationToken);
            }

            /// <inheritdoc/>
            public async Task UpdateAsync({{entityName}} entity, CancellationToken cancellationToken = default)
            {
                this.context.Set<{{entityName}}>().Update(entity);
                await this.context.SaveChangesAsync(cancellationToken);
            }

            /// <inheritdoc/>
            public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
            {
                {{entityName}}? entity = await this.GetByIdAsync(id, cancellationToken);
                if (entity is not null)
                {
                    this.context.Set<{{entityName}}>().Remove(entity);
                    await this.context.SaveChangesAsync(cancellationToken);
                }
            }
        }
        """;
    }

    private static string GenerateRedis(string projectName, string entityName, string implProject)
    {
        return $$"""
        namespace {{implProject}}.Repositories;

        using StackExchange.Redis;
        using {{projectName}}.Domain.{{entityName}}.Entities;
        using {{projectName}}.Domain.{{entityName}}.Repositories;

        /// <summary>
        /// Redis implementation of <see cref="I{{entityName}}Repository"/>.
        /// </summary>
        public class {{entityName}}Repository : I{{entityName}}Repository
        {
            private readonly IConnectionMultiplexer redis;

            /// <summary>
            /// Initializes a new instance of the <see cref="{{entityName}}Repository"/> class.
            /// </summary>
            /// <param name="redis">The Redis connection multiplexer.</param>
            public {{entityName}}Repository(IConnectionMultiplexer redis)
            {
                this.redis = redis;
            }

            /// <inheritdoc/>
            public Task<{{entityName}}?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public Task<IEnumerable<{{entityName}}>> GetAllAsync(CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public Task AddAsync({{entityName}} entity, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public Task UpdateAsync({{entityName}} entity, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }
        }
        """;
    }

    private static string GenerateStub(string projectName, string entityName)
    {
        return $$"""
        namespace {{projectName}}.Infrastructure.Repositories;

        using {{projectName}}.Domain.{{entityName}}.Entities;
        using {{projectName}}.Domain.{{entityName}}.Repositories;

        /// <summary>
        /// Stub implementation of <see cref="I{{entityName}}Repository"/>.
        /// Replace with a real implementation backed by your persistence layer.
        /// </summary>
        public class {{entityName}}Repository : I{{entityName}}Repository
        {
            /// <inheritdoc/>
            public Task<{{entityName}}?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public Task<IEnumerable<{{entityName}}>> GetAllAsync(CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public Task AddAsync({{entityName}} entity, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public Task UpdateAsync({{entityName}} entity, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }
        }
        """;
    }
}
