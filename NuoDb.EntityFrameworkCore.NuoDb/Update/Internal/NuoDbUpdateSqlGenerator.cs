// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using NuoDb.EntityFrameworkCore.NuoDb.Internal;

namespace NuoDb.EntityFrameworkCore.NuoDb.Update.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class NuoDbUpdateSqlGenerator : UpdateSqlGenerator
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public NuoDbUpdateSqlGenerator(UpdateSqlGeneratorDependencies dependencies)
            : base(dependencies)
        {
        }
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override void AppendIdentityWhereCondition(StringBuilder commandStringBuilder, IColumnModification columnModification)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(columnModification, nameof(columnModification));

            SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, columnModification.ColumnName);
            commandStringBuilder.Append(" = ")
                .Append("LAST_INSERT_ID()");
        }

        // /// <summary>
        // ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        // ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        // ///     any release. You should only use it directly in your code with extreme caution and knowing that
        // ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        // /// </summary>
        // protected override ResultSetMapping AppendSelectAffectedCountCommand(
        //     StringBuilder commandStringBuilder,
        //     string name,
        //     string? schema,
        //     int commandPosition)
        // {
        //     Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
        //     Check.NotEmpty(name, nameof(name));
        //
        //     commandStringBuilder
        //         .Append("SELECT changes()")
        //         .AppendLine(SqlGenerationHelper.StatementTerminator)
        //         .AppendLine();
        //
        //     return ResultSetMapping.LastInResultSet;
        // }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override void AppendRowsAffectedWhereCondition(StringBuilder commandStringBuilder, int expectedRowsAffected)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
        
            commandStringBuilder.Append("GETUPDATECOUNT() = ").Append(expectedRowsAffected);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override string GenerateNextSequenceValueOperation(string name, string? schema)
            => throw new NotSupportedException(NuoDbStrings.SequencesNotSupported);
    }
}
