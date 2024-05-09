// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;
using NuoDb.EntityFrameworkCore.NuoDb.Diagnostics.Internal;
using NuoDb.EntityFrameworkCore.NuoDb.Infrastructure.Internal;
using NuoDb.EntityFrameworkCore.NuoDb.Metadata.Internal;
using NuoDb.EntityFrameworkCore.NuoDb.Migrations.Internal;
using NuoDb.EntityFrameworkCore.NuoDb.Query.Internal;
using NuoDb.EntityFrameworkCore.NuoDb.Storage.Internal;
using NuoDb.EntityFrameworkCore.NuoDb.Update.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    ///     NuoDb specific extension methods for <see cref="IServiceCollection" />.
    /// </summary>
    public static class NuoDbServiceCollectionExtensions
    {
        /// <summary>
        ///     <para>
        ///         Registers the given Entity Framework <see cref="DbContext" /> as a service in the <see cref="IServiceCollection" />
        ///         and configures it to connect to a NuoDb database.
        ///     </para>
        ///     <para>
        ///         This method is a shortcut for configuring a <see cref="DbContext" /> to use NuoDb. It does not support all options.
        ///         Use <see cref="O:EntityFrameworkServiceCollectionExtensions.AddDbContext" /> and related methods for full control of
        ///         this process.
        ///     </para>
        ///     <para>
        ///         Use this method when using dependency injection in your application, such as with ASP.NET Core.
        ///         For applications that don't use dependency injection, consider creating <see cref="DbContext" />
        ///         instances directly with its constructor. The <see cref="DbContext.OnConfiguring" /> method can then be
        ///         overridden to configure the NuoDb provider and connection string.
        ///     </para>
        ///     <para>
        ///         To configure the <see cref="DbContextOptions{TContext}" /> for the context, either override the
        ///         <see cref="DbContext.OnConfiguring" /> method in your derived context, or supply
        ///         an optional action to configure the <see cref="DbContextOptions" /> for the context.
        ///     </para>
        ///     <para>
        ///         See <see href="https://aka.ms/efcore-docs-di">Using DbContext with dependency injection</see> for more information.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
        /// </remarks>
        /// <typeparam name="TContext">The type of context to be registered.</typeparam>
        /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="connectionString">The connection string of the database to connect to.</param>
        /// <param name="NuoDbOptionsAction">An optional action to allow additional NuoDb specific configuration.</param>
        /// <param name="optionsAction">An optional action to configure the <see cref="DbContextOptions" /> for the context.</param>
        /// <returns>The same service collection so that multiple calls can be chained.</returns>
        public static IServiceCollection AddNuoDb<TContext>(
            this IServiceCollection serviceCollection,
            string connectionString,
            Action<NuoDbDbContextOptionsBuilder>? NuoDbOptionsAction = null,
            Action<DbContextOptionsBuilder>? optionsAction = null)
            where TContext : DbContext
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));
            Check.NotEmpty(connectionString, nameof(connectionString));

            return serviceCollection.AddDbContext<TContext>(
                (serviceProvider, options) =>
                    {
                        optionsAction?.Invoke(options);
                        options.UseNuoDb(connectionString, NuoDbOptionsAction);
                    });
        }

        /// <summary>
        ///     <para>
        ///         Adds the services required by the NuoDb database provider for Entity Framework
        ///         to an <see cref="IServiceCollection" />.
        ///     </para>
        ///     <para>
        ///         Warning: Do not call this method accidentally. It is much more likely you need
        ///         to call <see cref="AddNuoDb{TContext}" />.
        ///     </para>
        ///     <para>
        ///         Calling this method is no longer necessary when building most applications, including those that
        ///         use dependency injection in ASP.NET or elsewhere.
        ///         It is only needed when building the internal service provider for use with
        ///         the <see cref="DbContextOptionsBuilder.UseInternalServiceProvider" /> method.
        ///         This is not recommend other than for some advanced scenarios.
        ///     </para>
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <returns>
        ///     The same service collection so that multiple calls can be chained.
        /// </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IServiceCollection AddEntityFrameworkNuoDb(this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            var builder = new EntityFrameworkRelationalServicesBuilder(serviceCollection)
                .TryAdd<LoggingDefinitions, NuoDbLoggingDefinitions>()
                .TryAdd<IDatabaseProvider, DatabaseProvider<NuoDbOptionsExtension>>()
                .TryAdd<IRelationalTypeMappingSource, NuoDbTypeMappingSource>()
                .TryAdd<ISqlGenerationHelper, NuoDbSqlGenerationHelper>()
                .TryAdd<IRelationalAnnotationProvider, NuoDbAnnotationProvider>()
                .TryAdd<IModelValidator, NuoDbModelValidator>()
                .TryAdd<IProviderConventionSetBuilder, NuoDbConventionSetBuilder>()
                .TryAdd<IUpdateSqlGenerator, NuoDbUpdateSqlGenerator>()
                .TryAdd<IModificationCommandBatchFactory, NuoDbModificationCommandBatchFactory>()
                .TryAdd<IRelationalConnection>(p => p.GetRequiredService<INuoDbRelationalConnection>())
                .TryAdd<IMigrationsSqlGenerator, NuoDbMigrationsSqlGenerator>()
                .TryAdd<IRelationalDatabaseCreator, NuoDbDatabaseCreator>()
                .TryAdd<IHistoryRepository, NuoDbHistoryRepository>()
                .TryAdd<IRelationalQueryStringFactory, NuoDbQueryStringFactory>()

                // New Query Pipeline
                .TryAdd<IMethodCallTranslatorProvider, NuoDbMethodCallTranslatorProvider>()
                .TryAdd<IMemberTranslatorProvider, NuoDbMemberTranslatorProvider>()
                .TryAdd<IQuerySqlGeneratorFactory, NuoDbQuerySqlGeneratorFactory>()
                .TryAdd<IQueryableMethodTranslatingExpressionVisitorFactory, NuoDbQueryableMethodTranslatingExpressionVisitorFactory>()
                .TryAdd<IRelationalSqlTranslatingExpressionVisitorFactory, NuoDbSqlTranslatingExpressionVisitorFactory>()
                .TryAdd<IQueryTranslationPostprocessorFactory, NuoDbQueryTranslationPostprocessorFactory>()
                .TryAddProviderSpecificServices(
                    b => b.TryAddScoped<INuoDbRelationalConnection, NuoDbRelationalConnection>());

            builder.TryAddCoreServices();

            return serviceCollection;
        }
    }
}
