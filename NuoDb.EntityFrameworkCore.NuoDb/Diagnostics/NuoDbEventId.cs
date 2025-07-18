// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using NuoDb.EntityFrameworkCore.NuoDb.Diagnostics.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     <para>
    ///         Event IDs for NuoDb events that correspond to messages logged to an <see cref="ILogger" />
    ///         and events sent to a <see cref="DiagnosticSource" />.
    ///     </para>
    ///     <para>
    ///         These IDs are also used with <see cref="WarningsConfigurationBuilder" /> to configure the
    ///         behavior of warnings.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see>, and
    /// </remarks>
    public static class NuoDbEventId
    {
        // Warning: These values must not change between releases.
        // Only add new values to the end of sections, never in the middle.
        // Try to use <Noun><Verb> naming and be consistent with existing names.
        private enum Id
        {
            // Model validation events
            SchemaConfiguredWarning = CoreEventId.ProviderBaseId,
            SequenceConfiguredWarning,

            // Infrastructure events
            UnexpectedConnectionTypeWarning = CoreEventId.ProviderBaseId + 100,

            // Migrations events
            TableRebuildPendingWarning = CoreEventId.ProviderBaseId + 200,

            // Scaffolding events
            ColumnFound = CoreEventId.ProviderDesignBaseId,
            SequenceFound,
            ForeignKeyFound,
            ForeignKeyPrincipalColumnMissingWarning,
            ForeignKeyReferencesMissingTableWarning,
            IndexFound,
            MissingTableWarning,
            PrimaryKeyFound,
            SchemasNotSupportedWarning,
            TableFound,
            UniqueConstraintFound
        }

        private static readonly string _validationPrefix = DbLoggerCategory.Model.Validation.Name + ".";

        private static EventId MakeValidationId(Id id)
            => new((int)id, _validationPrefix + id);



        public static readonly EventId SequenceConfiguredWarning = MakeValidationId(Id.SequenceConfiguredWarning);

        private static readonly string _infraPrefix = DbLoggerCategory.Infrastructure.Name + ".";

        private static EventId MakeInfraId(Id id)
            => new((int)id, _infraPrefix + id);


        private static readonly string _migrationsPrefix = DbLoggerCategory.Migrations.Name + ".";

        private static EventId MakeMigrationsId(Id id)
            => new((int)id, _migrationsPrefix + id);

        /// <summary>
        ///     An operation may fail due to a pending rebuild of the table.
        ///     This event is in the <see cref="DbLoggerCategory.Migrations" /> category.
        /// </summary>
        public static readonly EventId TableRebuildPendingWarning = MakeMigrationsId(Id.TableRebuildPendingWarning);

        private static readonly string _scaffoldingPrefix = DbLoggerCategory.Scaffolding.Name + ".";

        private static EventId MakeScaffoldingId(Id id)
            => new((int)id, _scaffoldingPrefix + id);

        /// <summary>
        ///     A column was found.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId ColumnFound = MakeScaffoldingId(Id.ColumnFound);

        public static readonly EventId SchemasNotSupportedWarning = MakeScaffoldingId(Id.SchemasNotSupportedWarning);

        /// <summary>
        ///     A foreign key references a missing table.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId ForeignKeyReferencesMissingTableWarning =
            MakeScaffoldingId(Id.ForeignKeyReferencesMissingTableWarning);

        /// <summary>
        ///     A table was found.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId TableFound = MakeScaffoldingId(Id.TableFound);

        /// <summary>
        ///     The database is missing a table.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId MissingTableWarning = MakeScaffoldingId(Id.MissingTableWarning);

        /// <summary>
        ///     A column referenced by a foreign key constraint was not found.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId ForeignKeyPrincipalColumnMissingWarning =
            MakeScaffoldingId(Id.ForeignKeyPrincipalColumnMissingWarning);

        /// <summary>
        ///     An index was found.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId IndexFound = MakeScaffoldingId(Id.IndexFound);

        /// <summary>
        ///     A foreign key was found.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId ForeignKeyFound = MakeScaffoldingId(Id.ForeignKeyFound);

        /// <summary>
        ///     A primary key was found.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId PrimaryKeyFound = MakeScaffoldingId(Id.PrimaryKeyFound);

        /// <summary>
        ///     A sequence was found.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId SequenceFound = MakeScaffoldingId(Id.SequenceFound);

        /// <summary>
        ///     A unique constraint was found.
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId UniqueConstraintFound = MakeScaffoldingId(Id.UniqueConstraintFound);

        /// <summary>
        ///     A schema was configured
        ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
        /// </summary>
        public static readonly EventId SchemaConfigured = MakeScaffoldingId(Id.SchemaConfiguredWarning);
    }
}
