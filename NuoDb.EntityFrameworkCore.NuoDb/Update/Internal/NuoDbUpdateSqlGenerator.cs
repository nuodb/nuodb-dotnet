// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using NuoDb.EntityFrameworkCore.NuoDb.Extensions.Internal;
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
    public class NuoDbUpdateSqlGenerator : UpdateAndSelectSqlGenerator
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
                .Append("SCOPE_IDENTITY()");
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>

        protected override ResultSetMapping AppendSelectAffectedCountCommand(
            StringBuilder commandStringBuilder,
            string name,
            string? schema,
            int commandPosition)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotEmpty(name, nameof(name));

            commandStringBuilder
                .Append("SELECT GETUPDATECOUNT() FROM DUAL")
                .AppendLine(SqlGenerationHelper.StatementTerminator);
                //.AppendLine();

            return ResultSetMapping.LastInResultSet | ResultSetMapping.ResultSetWithRowsAffectedOnly;
        }

        protected override ResultSetMapping AppendInsertAndSelectOperation(StringBuilder commandStringBuilder,
            IReadOnlyModificationCommand command, int commandPosition, out bool requiresTransaction)
        {
            var name = command.TableName;
            var schema = command.Schema;
            var operations = command.ColumnModifications;

            var writeOperations = operations.Where(o => o.IsWrite).ToList();
            var readOperations = operations.Where(o => o.IsRead).ToList();

            AppendInsertCommand(commandStringBuilder, name, schema, writeOperations, readOperations: Array.Empty<IColumnModification>());

            if (readOperations.Count > 0)
            {
                var keyOperations = operations.Where(o => o.IsKey).ToList();

                requiresTransaction = true;

                return AppendSelectAffectedCommand(commandStringBuilder, name, schema, readOperations, keyOperations, commandPosition);
            }

            requiresTransaction = false;

            return ResultSetMapping.NoResults;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override ResultSetMapping AppendInsertOperation(StringBuilder commandStringBuilder,
            IReadOnlyModificationCommand command,
            int commandPosition, out bool requiresTransaction)
            => command.IsIdentitySelectOnly()
                ? AppendInsertSelectingInsertedId(commandStringBuilder, command, commandPosition, out requiresTransaction) 
                :AppendInsertAndSelectOperation(commandStringBuilder, command, commandPosition,out requiresTransaction);



        private ResultSetMapping AppendInsertSelectingInsertedId(StringBuilder commandStringBuilder,
            IReadOnlyModificationCommand command,
            int commandPosition, out bool requiresTransaction)
        {
            var name = command.TableName;
            var schema = command.Schema;
            var operations = command.ColumnModifications;

            var writeOperations = operations.Where(o => o.IsWrite).ToList();
            var readKeyOperation = operations.FirstOrDefault(o => o is { IsKey: true, IsRead: true });


            AppendInsertCommandHeader(commandStringBuilder, name, schema, writeOperations);
            AppendValuesHeader(commandStringBuilder, writeOperations);
            AppendValues(commandStringBuilder, name, schema, writeOperations);
            commandStringBuilder.AppendLine(SqlGenerationHelper.StatementTerminator);
            commandStringBuilder
                .AppendLine($"SELECT SCOPE_IDENTITY() as {readKeyOperation.ColumnName} FROM DUAL{SqlGenerationHelper.StatementTerminator}");
            requiresTransaction = false;
            return ResultSetMapping.LastInResultSet;
        }


        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override ResultSetMapping AppendUpdateOperation(StringBuilder commandStringBuilder,
            IReadOnlyModificationCommand command,
            int commandPosition, out bool requiresTransaction)
            => AppendUpdateAndSelectOperation(commandStringBuilder, command, commandPosition, out requiresTransaction);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override ResultSetMapping AppendDeleteOperation(StringBuilder commandStringBuilder,
            IReadOnlyModificationCommand command,
            int commandPosition, out bool requiresTransaction)
            => AppendDeleteAndSelectOperation(commandStringBuilder, command, commandPosition, out requiresTransaction);



        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override void AppendRowsAffectedWhereCondition(StringBuilder commandStringBuilder, int expectedRowsAffected)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
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
