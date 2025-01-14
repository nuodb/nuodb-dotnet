// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using NuoDb.EntityFrameworkCore.NuoDb.Internal;
using NuoDb.EntityFrameworkCore.NuoDb.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    /// <summary>
    ///     <para>
    ///         NuoDb-specific implementation of <see cref="MigrationsSqlGenerator" />.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see>, and
    /// </remarks>
    public class NuoDbMigrationsSqlGenerator : MigrationsSqlGenerator
    {
        private IReadOnlyList<MigrationOperation> _operations = null!;
        /// <summary>
        ///     Creates a new <see cref="NuoDbMigrationsSqlGenerator" /> instance.
        /// </summary>
        /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
        /// <param name="migrationsAnnotations">Provider-specific Migrations annotations to use.</param>
        public NuoDbMigrationsSqlGenerator(
            MigrationsSqlGeneratorDependencies dependencies,
            IRelationalAnnotationProvider migrationsAnnotations)
            : base(dependencies)
        {
        }

  
        /// <summary>
        ///     Generates commands from a list of operations.
        /// </summary>
        /// <param name="operations">The operations.</param>
        /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
        /// <param name="options">The options to use when generating commands.</param>
        /// <returns>The list of commands to be executed or scripted.</returns>
        public override IReadOnlyList<MigrationCommand> Generate(
            IReadOnlyList<MigrationOperation> operations,
            IModel? model = null,
            MigrationsSqlGenerationOptions options = MigrationsSqlGenerationOptions.Default)
        {
            _operations = operations;
            try
            {
                return base.Generate(RewriteOperations(operations, model), model, options);
            }
            finally
            {
                _operations = null!;
            }
        }

        // TODO: Implement switch 
        private IReadOnlyList<MigrationOperation> RewriteOperations(
            IReadOnlyList<MigrationOperation> migrationOperations,
            IModel? model)
        {
            var operations = new List<MigrationOperation>();
            foreach (var operation in migrationOperations)
            {
                switch (operation)
                {
                   
                    case AddForeignKeyOperation foreignKeyOperation:
                    {
                        var table = operations
                            .OfType<CreateTableOperation>()
                            .FirstOrDefault(o => o.Name == foreignKeyOperation.Table);

                        if (table != null)
                        {
                            table.ForeignKeys.Add(foreignKeyOperation);
                        }
                        else
                        {

                            operations.Add(foreignKeyOperation);
                        }

                        break;
                    }
                    case AlterColumnOperation _:
                    case DropUniqueConstraintOperation _:
                    case DropColumnOperation dropColumnOperation:
                    case CreateIndexOperation createIndexOperation:
                    case RenameIndexOperation renameIndexOperation:
                    case AddColumnOperation addColumnOperation:
                    case RenameColumnOperation renameColumnOperation:
                    case RenameTableOperation renameTableOperation:
                    case AlterSequenceOperation _:
                    case CreateSequenceOperation _:
                    case CreateTableOperation _:
                    case DropIndexOperation _:
                    case DropSchemaOperation _:
                    case DropSequenceOperation _:
                    case DropTableOperation _:
                    case EnsureSchemaOperation _:
                    case RenameSequenceOperation _:
                    case RestartSequenceOperation _:
                        {
                            operations.Add(operation);

                            break;
                        }

                    case DeleteDataOperation _:
                    case InsertDataOperation _:
                    case UpdateDataOperation _:

                    default:
                        {
                            // foreach (var rebuild in rebuilds.Values)
                            // {
                            //     rebuild.OperationsToWarnFor.Add(operation);
                            // }

                            operations.Add(operation);

                            break;
                        }
                }
            }


            // in the future, may need to support rebuilds, and the rebuilds should happen here
            

            return operations;
        }

        /// <summary>
        ///     Builds commands for the given <see cref="AlterDatabaseOperation" /> by making calls on the given
        ///     <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
        /// <param name="builder">The command builder to use to build the commands.</param>
        protected override void Generate(AlterDatabaseOperation operation, IModel? model, MigrationCommandListBuilder builder)
        {
            if (operation[NuoDbAnnotationNames.InitSpatialMetaData] as bool? != true
                || operation.OldDatabase[NuoDbAnnotationNames.InitSpatialMetaData] as bool? == true)
            {
                return;
            }

            builder
                .Append("SELECT InitSpatialMetaData()")
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            EndStatement(builder);
        }

        /// <summary>
        ///     Builds commands for the given <see cref="DropIndexOperation" />
        ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
        /// <param name="builder">The command builder to use to build the commands.</param>
        /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
        protected override void Generate(
            DropIndexOperation operation,
            IModel? model,
            MigrationCommandListBuilder builder,
            bool terminate)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("DROP INDEX ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));

            if (terminate)
            {
                builder
                    .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                    .EndCommand();
            }
        }

        /// <summary>
        ///     Builds commands for the given <see cref="RenameIndexOperation" />
        ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
        /// <param name="builder">The command builder to use to build the commands.</param>
        protected override void Generate(RenameIndexOperation operation, IModel? model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (operation.NewName != null
                && operation.NewName != operation.Name)
            {
                builder
                    .Append("ALTER TABLE ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table))
                    
                    .Append(" RENAME INDEX ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                    .Append(" TO ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.NewName))
                    .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                    .EndCommand();
            }
        }

        /// <summary>
        ///     Builds commands for the given <see cref="RenameTableOperation" />
        ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
        /// <param name="builder">The command builder to use to build the commands.</param>
        protected override void Generate(RenameTableOperation operation, IModel? model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (operation.NewName != null
                && operation.NewName != operation.Name)
            {
                builder
                    .Append("ALTER TABLE ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                    .Append(" RENAME TO ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.NewName))
                    .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                    .EndCommand();
            }
        }

        /// <summary>
        ///     Builds commands for the given <see cref="RenameTableOperation" />
        ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
        /// <param name="builder">The command builder to use to build the commands.</param>
        protected override void Generate(RenameColumnOperation operation, IModel? model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table))
                .Append(" RENAME COLUMN ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .Append(" TO ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.NewName))
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                .EndCommand();
        }

        /// <summary>
        ///     Builds commands for the given <see cref="CreateTableOperation" /> by making calls on the given
        ///     <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
        /// <param name="builder">The command builder to use to build the commands.</param>
        /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
        protected override void Generate(
            CreateTableOperation operation,
            IModel? model,
            MigrationCommandListBuilder builder,
            bool terminate = true)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            

            // Lifts a primary key definition into the typename.
            // This handles the quirks of creating integer primary keys using autoincrement, not default rowid behavior.
            if (operation.PrimaryKey?.Columns.Length == 1)
            {
                var columnOp = operation.Columns.FirstOrDefault(o => o.Name == operation.PrimaryKey.Columns[0]);
                if (columnOp != null)
                //if (columnOp != null  && string.IsNullOrEmpty(operation.PrimaryKey.Name))
                {
                    //columnOp.AddAnnotation(NuoDbAnnotationNames.InlinePrimaryKey, true);
                    //columnOp.AddAnnotation(NuoDbAnnotationNames.Autoincrement, true);
                    if (!string.IsNullOrEmpty(operation.PrimaryKey.Name))
                    {
                        columnOp.AddAnnotation(NuoDbAnnotationNames.InlinePrimaryKeyName, operation.PrimaryKey.Name);
                    }
            
                    //operation.PrimaryKey = null;
                }
            }

            builder
                .Append("CREATE TABLE IF NOT EXISTS ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
                .AppendLine(" (");

            using (builder.Indent())
            {
                if (!string.IsNullOrEmpty(operation.Comment))
                {
                    builder
                        .AppendLines(Dependencies.SqlGenerationHelper.GenerateComment(operation.Comment))
                        .AppendLine();
                }

                CreateTableColumns(operation, model, builder);
                CreateTableConstraints(operation, model, builder);
                builder.AppendLine();
            }

            builder.Append(")");

            if (terminate)
            {
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                EndStatement(builder);
            }
        }



        
        /// <summary>
        ///     Generates a SQL fragment for the column definitions in a <see cref="CreateTableOperation" />.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
        /// <param name="builder">The command builder to use to add the SQL fragment.</param>
        protected override void CreateTableColumns(
            CreateTableOperation operation,
            IModel? model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));
            var sortedCols = operation.Columns.OrderBy(x => string.IsNullOrEmpty(x.ComputedColumnSql)).ToList();

            if (!operation.Columns.Any(c => !string.IsNullOrEmpty(c.Comment)))
            {
                //base.CreateTableColumns(sortedCols, model, builder);
                var columns = operation.Columns.OrderBy(x => !String.IsNullOrEmpty(x.ComputedColumnSql)).ToList();

                for (var i = 0; i < columns.Count; i++)
                {
                    ColumnDefinition(columns[i], model, builder);

                    if (i != columns.Count - 1)
                    {
                        builder.AppendLine(",");
                    }
                }
            }
            else
            {
                CreateTableColumnsWithComments(operation, model, builder);
            }
        }

        private void CreateTableColumnsWithComments(
            CreateTableOperation operation,
            IModel? model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));
            var columns = operation.Columns.OrderBy(x => !String.IsNullOrEmpty(x.ComputedColumnSql)).ToList();
            for (var i = 0; i < columns.Count; i++)
            {
                var column = columns[i];

                if (i > 0)
                {
                    builder.AppendLine();
                }

                if (!string.IsNullOrEmpty(column.Comment))
                {
                    builder.AppendLines(Dependencies.SqlGenerationHelper.GenerateComment(column.Comment));
                }

                ColumnDefinition(column, model, builder);

                if (i != columns.Count - 1)
                {
                    builder.AppendLine(",");
                }
            }
        }

        /// <summary>
        ///     Generates a SQL fragment for a column definition for the given column metadata.
        /// </summary>
        /// <param name="schema">The schema that contains the table, or <see langword="null" /> to use the default schema.</param>
        /// <param name="table">The table that contains the column.</param>
        /// <param name="name">The column name.</param>
        /// <param name="operation">The column metadata.</param>
        /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
        /// <param name="builder">The command builder to use to add the SQL fragment.</param>
        protected override void ColumnDefinition(
            string? schema,
            string table,
            string name,
            ColumnOperation operation,
            IModel? model,
            MigrationCommandListBuilder builder)
        {
            base.ColumnDefinition(schema, table, name, operation, model, builder);

            var inlinePk = operation[NuoDbAnnotationNames.InlinePrimaryKey] as bool?;
            if (inlinePk == true)
            {
                var inlinePkName = operation[
                    NuoDbAnnotationNames.InlinePrimaryKeyName] as string;
                builder.Append(" PRIMARY KEY");
            }

            var autoincrement = operation[NuoDbAnnotationNames.Autoincrement] as bool?
                                ?? operation[NuoDbAnnotationNames.LegacyAutoincrement] as bool?;
            if (autoincrement == true)
            {
                builder.Append(" GENERATED BY DEFAULT");
                builder.Append(" AS IDENTITY");
            }
        }

        #region Invalid migration operations



        /// <summary>
        ///     Builds commands for the given <see cref="AddForeignKeyOperation" /> by making calls on the given
        ///     <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
        /// <param name="builder">The command builder to use to build the commands.</param>
        /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
        protected override void Generate(
            AddForeignKeyOperation operation,
            IModel? model,
            MigrationCommandListBuilder builder,
            bool terminate = true)
        {
            base.Generate(operation, model, builder, terminate: false);

            if (terminate)
            {
                builder
                    .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                    .EndCommand(suppressTransaction: false);
            }
        }

        /// <summary>
        ///     Throws <see cref="NotSupportedException" /> since this operation requires table rebuilds, which
        ///     are not yet supported.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
        /// <param name="builder">The command builder to use to build the commands.</param>
        /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
        protected override void Generate(
            DropForeignKeyOperation operation,
            IModel? model,
            MigrationCommandListBuilder builder,
            bool terminate = true)
        {
            base.Generate(operation, model, builder, terminate: false);
        }
            
        protected override void Generate(InsertDataOperation operation, IModel model, MigrationCommandListBuilder builder, bool terminate = true)
        {
            base.Generate(operation, model, builder, terminate);
        }


        protected override void DefaultValue(object defaultValue, string defaultValueSql, string columnType,
            MigrationCommandListBuilder builder)
        {
            //base.DefaultValue(defaultValue, defaultValueSql, columnType, builder);
            Check.NotNull(builder, nameof(builder));

            if (defaultValueSql != null)
            {
                builder
                    .Append(" DEFAULT (")
                    .Append(defaultValueSql)
                    .Append(")");
            }
            else if (defaultValue != null)
            {
                var typeMapping = columnType != null
                    ? Dependencies.TypeMappingSource.FindMapping(defaultValue.GetType(), columnType)
                    : null;
                if (typeMapping == null)
                {
                    typeMapping = Dependencies.TypeMappingSource.GetMappingForValue(defaultValue);
                }

                builder
                    .Append(" DEFAULT ")
                    .Append(typeMapping.GenerateSqlLiteral(defaultValue));
            }
        }

        /// <summary>
        ///     Builds commands for the given <see cref="AlterColumnOperation" />
        ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
        /// <param name="builder">The command builder to use to build the commands.</param>
        protected override void Generate(
            AlterColumnOperation operation,
            IModel? model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (operation[RelationalAnnotationNames.ColumnOrder] != operation.OldColumn[RelationalAnnotationNames.ColumnOrder])
            {
                Dependencies.MigrationsLogger.ColumnOrderIgnoredWarning(operation);
            }

            IEnumerable<ITableIndex>? indexesToRebuild = null;
            var column = model?.GetRelationalModel().FindTable(operation.Table, operation.Schema)
                ?.Columns.FirstOrDefault(c => c.Name == operation.Name);

            if (operation.ComputedColumnSql != operation.OldColumn.ComputedColumnSql
                || operation.IsStored != operation.OldColumn.IsStored)
            {
                var dropColumnOperation = new DropColumnOperation
                {
                    Schema = operation.Schema,
                    Table = operation.Table,
                    Name = operation.Name
                };
                if (column != null)
                {
                    dropColumnOperation.AddAnnotations(column.GetAnnotations());
                }

                var addColumnOperation = new AddColumnOperation
                {
                    Schema = operation.Schema,
                    Table = operation.Table,
                    Name = operation.Name,
                    ClrType = operation.ClrType,
                    ColumnType = operation.ColumnType,
                    IsUnicode = operation.IsUnicode,
                    IsFixedLength = operation.IsFixedLength,
                    MaxLength = operation.MaxLength,
                    Precision = operation.Precision,
                    Scale = operation.Scale,
                    IsRowVersion = operation.IsRowVersion,
                    IsNullable = operation.IsNullable,
                    DefaultValue = operation.DefaultValue,
                    DefaultValueSql = operation.DefaultValueSql,
                    ComputedColumnSql = operation.ComputedColumnSql,
                    IsStored = operation.IsStored,
                    Comment = operation.Comment,
                    Collation = operation.Collation
                };
                addColumnOperation.AddAnnotations(operation.GetAnnotations());

                // TODO: Use a column rebuild instead
                indexesToRebuild = GetIndexesToRebuild(column, operation).ToList();
                DropIndexes(indexesToRebuild, builder);
                Generate(dropColumnOperation, model, builder, terminate: false);
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                Generate(addColumnOperation, model, builder);
                CreateIndexes(indexesToRebuild, builder);
                builder.EndCommand(suppressTransaction: false);

                return;
            }

            var columnType = operation.ColumnType
                ?? GetColumnType(
                    operation.Schema,
                    operation.Table,
                    operation.Name,
                    operation,
                    model);

            var narrowed = false;
            var oldColumnSupported = IsOldColumnSupported(model);
            if (oldColumnSupported)
            {
                if (IsIdentity(operation) != IsIdentity(operation.OldColumn))
                {
                    throw new InvalidOperationException(NuoDbStrings.AlterIdentityColumn);
                }

                var oldType = operation.OldColumn.ColumnType
                    ?? GetColumnType(
                        operation.Schema,
                        operation.Table,
                        operation.Name,
                        operation.OldColumn,
                        model);
                narrowed = columnType != oldType
                    || operation.Collation != operation.OldColumn.Collation
                    || !operation.IsNullable && operation.OldColumn.IsNullable;
            }

            if (narrowed)
            {
                indexesToRebuild = GetIndexesToRebuild(column, operation).ToList();
                DropIndexes(indexesToRebuild, builder);
            }

            var alterStatementNeeded = narrowed
                || !oldColumnSupported
                || operation.ClrType != operation.OldColumn.ClrType
                || columnType != operation.OldColumn.ColumnType
                || operation.IsUnicode != operation.OldColumn.IsUnicode
                || operation.IsFixedLength != operation.OldColumn.IsFixedLength
                || operation.MaxLength != operation.OldColumn.MaxLength
                || operation.Precision != operation.OldColumn.Precision
                || operation.Scale != operation.OldColumn.Scale
                || operation.IsRowVersion != operation.OldColumn.IsRowVersion
                || operation.IsNullable != operation.OldColumn.IsNullable
                || operation.Collation != operation.OldColumn.Collation
                || HasDifferences(operation.GetAnnotations(), operation.OldColumn.GetAnnotations());

            var (oldDefaultValue, oldDefaultValueSql) = (operation.OldColumn.DefaultValue, operation.OldColumn.DefaultValueSql);

            if (alterStatementNeeded
                || !Equals(operation.DefaultValue, oldDefaultValue)
                || operation.DefaultValueSql != oldDefaultValueSql)
            {
                //DropDefaultConstraint(operation.Schema, operation.Table, operation.Name, builder);
                (oldDefaultValue, oldDefaultValueSql) = (null, null);
            }

            if (alterStatementNeeded)
            {
                builder
                    .Append("ALTER TABLE ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                    .Append(" MODIFY COLUMN ");

                // NB: ComputedColumnSql, IsStored, DefaultValue, DefaultValueSql, Comment, ValueGenerationStrategy, and Identity are
                //     handled elsewhere. Don't copy them here.
                var definitionOperation = new AlterColumnOperation
                {
                    Schema = operation.Schema,
                    Table = operation.Table,
                    Name = operation.Name,
                    ClrType = operation.ClrType,
                    ColumnType = operation.ColumnType,
                    IsUnicode = operation.IsUnicode,
                    IsFixedLength = operation.IsFixedLength,
                    MaxLength = operation.MaxLength,
                    Precision = operation.Precision,
                    Scale = operation.Scale,
                    IsRowVersion = operation.IsRowVersion,
                    IsNullable = operation.IsNullable,
                    Collation = operation.Collation,
                    OldColumn = operation.OldColumn
                };
                definitionOperation.AddAnnotations(
                    operation.GetAnnotations().Where(
                        a => a.Name != NuoDbAnnotationNames.Identity));

                ColumnDefinition(
                    operation.Schema,
                    operation.Table,
                    operation.Name,
                    definitionOperation,
                    model,
                    builder);

                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            }

            if (!Equals(operation.DefaultValue, oldDefaultValue) || operation.DefaultValueSql != oldDefaultValueSql)
            {
                builder
                    .Append("ALTER TABLE ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                    .Append(" ALTER ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                    .Append(" SET ");
                DefaultValue(operation.DefaultValue, operation.DefaultValueSql, operation.ColumnType, builder);
                builder
                    .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            }



            if (narrowed)
            {
                CreateIndexes(indexesToRebuild!, builder);
            }

            builder.EndCommand(suppressTransaction: false);
        }

        protected override void Generate(AlterTableOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            base.Generate(operation, model, builder);
        }

        private static bool HasDifferences(IEnumerable<IAnnotation> source, IEnumerable<IAnnotation> target)
        {
            var targetAnnotations = target.ToDictionary(a => a.Name);

            var count = 0;
            foreach (var sourceAnnotation in source)
            {
                if (!targetAnnotations.TryGetValue(sourceAnnotation.Name, out var targetAnnotation)
                    || !Equals(sourceAnnotation.Value, targetAnnotation.Value))
                {
                    return true;
                }

                count++;
            }

            return count != targetAnnotations.Count;
        }
        /// <summary>
        ///     Generates SQL to create the given indexes.
        /// </summary>
        /// <param name="indexes">The indexes to create.</param>
        /// <param name="builder">The command builder to use to build the commands.</param>
        protected virtual void CreateIndexes(
            IEnumerable<ITableIndex> indexes,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(indexes, nameof(indexes));
            Check.NotNull(builder, nameof(builder));

            foreach (var index in indexes)
            {
                Generate(CreateIndexOperation.CreateFrom(index), index.Table.Model.Model, builder, terminate: false);
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            }
        }

        protected override void Generate(CreateIndexOperation operation, IModel model, MigrationCommandListBuilder builder,
            bool terminate = true)
        {
            if (!string.IsNullOrEmpty(operation.Filter))
            {
                throw new InvalidOperationException("NuoDb does not support indexes with filters");
            }

            base.Generate(operation, model, builder, terminate);
        }


        private static bool IsIdentity(ColumnOperation operation)
            => operation[NuoDbAnnotationNames.Identity] != null;
      

        /// <summary>
        ///     Generates SQL to drop the given indexes.
        /// </summary>
        /// <param name="indexes">The indexes to drop.</param>
        /// <param name="builder">The command builder to use to build the commands.</param>
        protected virtual void DropIndexes(
            IEnumerable<ITableIndex> indexes,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(indexes, nameof(indexes));
            Check.NotNull(builder, nameof(builder));

            foreach (var index in indexes)
            {
                var table = index.Table;
                var operation = new DropIndexOperation
                {
                    Schema = table.Schema,
                    Table = table.Name,
                    Name = index.Name
                };
                operation.AddAnnotations(index.GetAnnotations());

                Generate(operation, table.Model.Model, builder, terminate: false);
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            }
        }


        /// <summary>
        ///     Gets the list of indexes that need to be rebuilt when the given column is changing.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="currentOperation">The operation which may require a rebuild.</param>
        /// <returns>The list of indexes affected.</returns>
        protected virtual IEnumerable<ITableIndex> GetIndexesToRebuild(
            IColumn? column,
            MigrationOperation currentOperation)
        {
            Check.NotNull(currentOperation, nameof(currentOperation));

            if (column == null)
            {
                yield break;
            }

            var table = column.Table;
            var createIndexOperations = _operations.SkipWhile(o => o != currentOperation).Skip(1)
                .OfType<CreateIndexOperation>().Where(o => o.Table == table.Name && o.Schema == table.Schema).ToList();
            foreach (var index in table.Indexes)
            {
                var indexName = index.Name;
                if (createIndexOperations.Any(o => o.Name == indexName))
                {
                    continue;
                }

                if (index.Columns.Any(c => c == column))
                {
                    yield return index;
                }
                else if (index[NuoDbAnnotationNames.Include] is IReadOnlyList<string> includeColumns
                         && includeColumns.Contains(column.Name))
                {
                    yield return index;
                }
            }
        }

        /// <summary>
        ///     Generates a SQL fragment for a computed column definition for the given column metadata.
        /// </summary>
        /// <param name="schema">The schema that contains the table, or <see langword="null" /> to use the default schema.</param>
        /// <param name="table">The table that contains the column.</param>
        /// <param name="name">The column name.</param>
        /// <param name="operation">The column metadata.</param>
        /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
        /// <param name="builder">The command builder to use to add the SQL fragment.</param>
        protected override void ComputedColumnDefinition(
            string? schema,
            string table,
            string name,
            ColumnOperation operation,
            IModel? model,
            MigrationCommandListBuilder builder)
        {
            builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(name));
            var columnType = operation.ColumnType ?? GetColumnType(schema, table, name, operation, model)!;

            builder
                .Append(" ")
                .Append(columnType)
                .Append(" GENERATED ALWAYS AS (")
                .Append(operation.ComputedColumnSql!)
                .Append(") persisted");


            if (operation.Collation != null)
            {
                builder
                    .Append(" COLLATE ")
                    .Append(operation.Collation);
            }
        }
        
        protected override void Generate(
            CreateSequenceOperation operation,
            IModel? model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("CREATE SEQUENCE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema));

            builder
                .Append(" START WITH ")
                .Append(IntegerConstant(operation.StartValue));

            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
        }

        /// <summary>
        ///     Builds commands for the given <see cref="RestartSequenceOperation" /> by making calls on the given
        ///     <see cref="MigrationCommandListBuilder" />, and then terminates the final command.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
        /// <param name="builder">The command builder to use to build the commands.</param>
        protected override void Generate(
            RestartSequenceOperation operation,
            IModel? model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER SEQUENCE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
                .Append(" START WITH ")
                .Append(IntegerConstant(operation.StartValue))
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
        }

        protected override void Generate(AlterSequenceOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER SEQUENCE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema));

            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
        }

        private string IntegerConstant(long value)
            => string.Format(CultureInfo.InvariantCulture, "{0}", value);
        #endregion

        #region Ignored schema operations

        /// <summary>
        ///     Ignored, since schemas are not supported by NuoDb and are silently ignored to improve testing compatibility.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
        /// <param name="builder">The command builder to use to build the commands.</param>
        protected override void Generate(EnsureSchemaOperation operation, IModel? model, MigrationCommandListBuilder builder)
        {
        }

        /// <summary>
        ///     Ignored, since schemas are not supported by NuoDb and are silently ignored to improve testing compatibility.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
        /// <param name="builder">The command builder to use to build the commands.</param>
        protected override void Generate(DropSchemaOperation operation, IModel? model, MigrationCommandListBuilder builder)
        {
        }

        #endregion

        #region Sequences not supported

        

        /// <summary>
        ///     Throws <see cref="NotSupportedException" /> since NuoDb does not support sequences.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
        /// <param name="builder">The command builder to use to build the commands.</param>
        protected override void Generate(RenameSequenceOperation operation, IModel? model, MigrationCommandListBuilder builder)
            => throw new InvalidOperationException("NuoDb Does not support renaming sequences");

        // /// <summary>
        // ///     Throws <see cref="NotSupportedException" /> since NuoDb does not support sequences.
        // /// </summary>
        // /// <param name="operation">The operation.</param>
        // /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
        // /// <param name="builder">The command builder to use to build the commands.</param>
        // protected override void Generate(AlterSequenceOperation operation, IModel? model, MigrationCommandListBuilder builder)
        //     => throw new NotSupportedException(NuoDbStrings.SequencesNotSupported);

        // /// <summary>
        // ///     Throws <see cref="NotSupportedException" /> since NuoDb does not support sequences.
        // /// </summary>
        // /// <param name="operation">The operation.</param>
        // /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
        // /// <param name="builder">The command builder to use to build the commands.</param>
        // protected override void Generate(DropSequenceOperation operation, IModel? model, MigrationCommandListBuilder builder)
        //     => throw new NotSupportedException(NuoDbStrings.SequencesNotSupported);

        #endregion

        private sealed class RebuildContext
        {
            public ICollection<MigrationOperation> OperationsToReplace { get; } = new List<MigrationOperation>();
            public IDictionary<string, AddColumnOperation> AddColumnsDeferred { get; } = new Dictionary<string, AddColumnOperation>();
            public ICollection<string> DropColumnsDeferred { get; } = new HashSet<string>();
            public readonly IDictionary<string, AlterColumnOperation> AlterColumnsDeferred = new Dictionary<string, AlterColumnOperation>();

            public readonly IDictionary<string, RenameColumnOperation> RenameColumnsDeferred =
                new Dictionary<string, RenameColumnOperation>();

            public ICollection<string> CreateIndexesDeferred { get; } = new HashSet<string>();
            public ICollection<MigrationOperation> OperationsToWarnFor { get; } = new List<MigrationOperation>();
        }
    }
}
