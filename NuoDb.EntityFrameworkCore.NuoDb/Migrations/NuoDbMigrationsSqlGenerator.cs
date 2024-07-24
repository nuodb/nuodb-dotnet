// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using NuoDb.EntityFrameworkCore.NuoDb.Extensions;
using NuoDb.EntityFrameworkCore.NuoDb.Extensions.Internal;
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
        private int _variableCounter;
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

        // private bool IsSpatialiteColumn(AddColumnOperation operation, IModel? model)
        //     => NuoDbTypeMappingSource.IsSpatialiteType(
        //         operation.ColumnType
        //         ?? GetColumnType(
        //             operation.Schema,
        //             operation.Table,
        //             operation.Name,
        //             operation,
        //             model)!);

        // TODO: Implement switch 
        private IReadOnlyList<MigrationOperation> RewriteOperations(
            IReadOnlyList<MigrationOperation> migrationOperations,
            IModel? model)
        {
            var operations = new List<MigrationOperation>();
            //var rebuilds = new Dictionary<(string Table, string? Schema), RebuildContext>();
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
                            //var rebuild = rebuilds.GetOrAddNew((foreignKeyOperation.Table, foreignKeyOperation.Schema));
                            //rebuild.OperationsToReplace.Add(foreignKeyOperation);

                            operations.Add(foreignKeyOperation);
                        }

                        break;
                    }
                    case AlterColumnOperation _:
                    case DropUniqueConstraintOperation _:
                        // {
                        //     var tableOperation = (ITableMigrationOperation)operation;
                        //     var rebuild = rebuilds.GetOrAddNew((tableOperation.Table, tableOperation.Schema));
                        //     rebuild.OperationsToReplace.Add(operation);
                        //
                        //     operations.Add(operation);
                        //
                        //     break;
                        // }

                    case DropColumnOperation dropColumnOperation:
                        // {
                        //     var rebuild = rebuilds.GetOrAddNew((dropColumnOperation.Table, dropColumnOperation.Schema));
                        //     rebuild.OperationsToReplace.Add(dropColumnOperation);
                        //     rebuild.DropColumnsDeferred.Add(dropColumnOperation.Name);
                        //
                        //     operations.Add(dropColumnOperation);
                        //
                        //     break;
                        // }

                    
                    case CreateIndexOperation createIndexOperation:
                        // {
                        //     if (rebuilds.TryGetValue((createIndexOperation.Table, createIndexOperation.Schema), out var rebuild)
                        //         && (rebuild.AddColumnsDeferred.Keys.Intersect(createIndexOperation.Columns).Any()
                        //             || rebuild.RenameColumnsDeferred.Keys.Intersect(createIndexOperation.Columns).Any()))
                        //     {
                        //         rebuild.OperationsToReplace.Add(createIndexOperation);
                        //         rebuild.CreateIndexesDeferred.Add(createIndexOperation.Name);
                        //     }
                        //
                        //     operations.Add(createIndexOperation);
                        //
                        //     break;
                        // }

                    case RenameIndexOperation renameIndexOperation:
                        // {
                        //     var index = renameIndexOperation.Table != null
                        //         ? model?.GetRelationalModel().FindTable(renameIndexOperation.Table, renameIndexOperation.Schema)
                        //             ?.Indexes.FirstOrDefault(i => i.Name == renameIndexOperation.NewName)
                        //         : null;
                        //     if (index != null)
                        //     {
                        //         operations.Add(
                        //             new DropIndexOperation
                        //             {
                        //                 Table = renameIndexOperation.Table,
                        //                 Schema = renameIndexOperation.Schema,
                        //                 Name = renameIndexOperation.Name
                        //             });
                        //
                        //         operations.Add(CreateIndexOperation.CreateFrom(index));
                        //     }
                        //     else
                        //     {
                        //         operations.Add(renameIndexOperation);
                        //     }
                        //
                        //     break;
                        // }

                    case AddColumnOperation addColumnOperation:
                        // {
                        //     if (rebuilds.TryGetValue((addColumnOperation.Table, addColumnOperation.Schema), out var rebuild)
                        //         && rebuild.DropColumnsDeferred.Contains(addColumnOperation.Name))
                        //     {
                        //         rebuild.OperationsToReplace.Add(addColumnOperation);
                        //         rebuild.AddColumnsDeferred.Add(addColumnOperation.Name, addColumnOperation);
                        //     }
                        //     else if (addColumnOperation.Comment != null)
                        //     {
                        //         rebuilds.GetOrAddNew((addColumnOperation.Table, addColumnOperation.Schema));
                        //     }
                        //
                        //     operations.Add(addColumnOperation);
                        //
                        //     break;
                        // }

                    case RenameColumnOperation renameColumnOperation:
                        // {
                        //     if (rebuilds.TryGetValue((renameColumnOperation.Table, renameColumnOperation.Schema), out var rebuild))
                        //     {
                        //         if (rebuild.DropColumnsDeferred.Contains(renameColumnOperation.NewName))
                        //         {
                        //             rebuild.OperationsToReplace.Add(renameColumnOperation);
                        //             rebuild.DropColumnsDeferred.Add(renameColumnOperation.Name);
                        //             rebuild.RenameColumnsDeferred.Add(renameColumnOperation.NewName, renameColumnOperation);
                        //         }
                        //     }
                        //
                        //     operations.Add(renameColumnOperation);
                        //
                        //     break;
                        // }

                    case RenameTableOperation renameTableOperation:
                        // {
                        //     if (rebuilds.Remove((renameTableOperation.Name, renameTableOperation.Schema), out var rebuild))
                        //     {
                        //         rebuilds.Add(
                        //             (renameTableOperation.NewName ?? renameTableOperation.Name, renameTableOperation.NewSchema), rebuild);
                        //     }
                        //
                        //     operations.Add(renameTableOperation);
                        //
                        //     break;
                        // }

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
                        // {
                        //     var tableOperation = (ITableMigrationOperation)operation;
                        //     if (rebuilds.TryGetValue((tableOperation.Table, tableOperation.Schema), out var rebuild))
                        //     {
                        //         rebuild.OperationsToWarnFor.Add(operation);
                        //     }
                        //
                        //     operations.Add(operation);
                        //
                        //     break;
                        // }

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

            // var skippedRebuilds = new List<(string Table, string? Schema)>();
            // var indexesToRebuild = new List<ITableIndex>();
            // foreach (var rebuild in rebuilds)
            // {
            //     var table = model?.GetRelationalModel().FindTable(rebuild.Key.Table, rebuild.Key.Schema);
            //     if (table == null)
            //     {
            //         skippedRebuilds.Add(rebuild.Key);
            //
            //         continue;
            //     }
            //
            //     foreach (var operationToWarnFor in rebuild.Value.OperationsToWarnFor)
            //     {
            //         // TODO: Consider warning once per table--list all operation types we're warning for
            //         // TODO: Consider listing which operations required a rebuild
            //         Dependencies.MigrationsLogger.TableRebuildPendingWarning(operationToWarnFor.GetType(), table.Name);
            //     }
            //
            //     foreach (var operationToReplace in rebuild.Value.OperationsToReplace)
            //     {
            //         operations.Remove(operationToReplace);
            //     }
            //
            //     var createTableOperation = new CreateTableOperation
            //     {
            //         Name = "ef_temp_" + table.Name,
            //         Schema = table.Schema,
            //         Comment = table.Comment
            //     };
            //
            //     var primaryKey = table.PrimaryKey;
            //     if (primaryKey != null)
            //     {
            //         createTableOperation.PrimaryKey = AddPrimaryKeyOperation.CreateFrom(primaryKey);
            //     }
            //
            //     foreach (var column in table.Columns.Where(c => c.Order.HasValue).OrderBy(c => c.Order!.Value)
            //         .Concat(table.Columns.Where(c => !c.Order.HasValue)))
            //     {
            //         if (!column.TryGetDefaultValue(out var defaultValue))
            //         {
            //             defaultValue = null;
            //         }
            //
            //         var addColumnOperation = new AddColumnOperation
            //         {
            //             Name = column.Name,
            //             ColumnType = column.StoreType,
            //             IsNullable = column.IsNullable,
            //             DefaultValue = rebuild.Value.AddColumnsDeferred.TryGetValue(column.Name, out var originalOperation)
            //                 && !originalOperation.IsNullable
            //                     ? originalOperation.DefaultValue
            //                     : defaultValue,
            //             DefaultValueSql = column.DefaultValueSql,
            //             ComputedColumnSql = column.ComputedColumnSql,
            //             IsStored = column.IsStored,
            //             Comment = column.Comment,
            //             Collation = column.Collation
            //         };
            //         addColumnOperation.AddAnnotations(column.GetAnnotations());
            //         createTableOperation.Columns.Add(addColumnOperation);
            //     }
            //
            //     foreach (var foreignKey in table.ForeignKeyConstraints)
            //     {
            //         createTableOperation.ForeignKeys.Add(AddForeignKeyOperation.CreateFrom(foreignKey));
            //     }
            //
            //     foreach (var uniqueConstraint in table.UniqueConstraints.Where(c => !c.GetIsPrimaryKey()))
            //     {
            //         createTableOperation.UniqueConstraints.Add(AddUniqueConstraintOperation.CreateFrom(uniqueConstraint));
            //     }
            //
            //     foreach (var checkConstraint in table.CheckConstraints)
            //     {
            //         createTableOperation.CheckConstraints.Add(AddCheckConstraintOperation.CreateFrom(checkConstraint));
            //     }
            //
            //     createTableOperation.AddAnnotations(table.GetAnnotations());
            //     operations.Add(createTableOperation);
            //
            //     foreach (var index in table.Indexes)
            //     {
            //         if (index.IsUnique && rebuild.Value.CreateIndexesDeferred.Contains(index.Name))
            //         {
            //             var createIndexOperation = CreateIndexOperation.CreateFrom(index);
            //             createIndexOperation.Table = createTableOperation.Name;
            //             operations.Add(createIndexOperation);
            //         }
            //         else
            //         {
            //             indexesToRebuild.Add(index);
            //         }
            //     }
            //
            //     var intoBuilder = new StringBuilder();
            //     var selectBuilder = new StringBuilder();
            //     var first = true;
            //     foreach (var column in table.Columns)
            //     {
            //         if (column.ComputedColumnSql != null
            //             || rebuild.Value.AddColumnsDeferred.ContainsKey(column.Name))
            //         {
            //             continue;
            //         }
            //
            //         if (first)
            //         {
            //             first = false;
            //         }
            //         else
            //         {
            //             intoBuilder.Append(", ");
            //             selectBuilder.Append(", ");
            //         }
            //
            //         intoBuilder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(column.Name));
            //
            //         var defaultValue = rebuild.Value.AlterColumnsDeferred.TryGetValue(column.Name, out var alterColumnOperation)
            //             && !alterColumnOperation.IsNullable
            //             && alterColumnOperation.OldColumn.IsNullable
            //                 ? alterColumnOperation.DefaultValue
            //                 : null;
            //         if (defaultValue != null)
            //         {
            //             selectBuilder.Append("IFNULL(");
            //         }
            //
            //         selectBuilder.Append(
            //             Dependencies.SqlGenerationHelper.DelimitIdentifier(
            //                 rebuild.Value.RenameColumnsDeferred.TryGetValue(column.Name, out var renameColumnOperation)
            //                     ? renameColumnOperation.Name
            //                     : column.Name));
            //
            //         if (defaultValue != null)
            //         {
            //             var defaultValueTypeMapping = (column.StoreType == null
            //                     ? null
            //                     : Dependencies.TypeMappingSource.FindMapping(defaultValue.GetType(), column.StoreType))
            //                 ?? Dependencies.TypeMappingSource.GetMappingForValue(defaultValue);
            //
            //             selectBuilder
            //                 .Append(", ")
            //                 .Append(defaultValueTypeMapping.GenerateSqlLiteral(defaultValue))
            //                 .Append(')');
            //         }
            //     }
            //
            //     operations.Add(
            //         new SqlOperation
            //         {
            //             Sql = new StringBuilder()
            //                 .Append("INSERT INTO ")
            //                 .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(createTableOperation.Name))
            //                 .Append(" (")
            //                 .Append(intoBuilder)
            //                 .AppendLine(")")
            //                 .Append("SELECT ")
            //                 .Append(selectBuilder)
            //                 .AppendLine()
            //                 .Append("FROM ")
            //                 .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(table.Name))
            //                 .Append(Dependencies.SqlGenerationHelper.StatementTerminator)
            //                 .ToString()
            //         });
            // }
            //
            // foreach (var skippedRebuild in skippedRebuilds)
            // {
            //     rebuilds.Remove(skippedRebuild);
            // }

            // if (rebuilds.Any())
            // {
            //     operations.Add(
            //         new SqlOperation { Sql = "PRAGMA foreign_keys = 0;", SuppressTransaction = true });
            // }
            //
            // foreach (var rebuild in rebuilds)
            // {
            //     operations.Add(
            //         new DropTableOperation { Name = rebuild.Key.Table, Schema = rebuild.Key.Schema });
            //     operations.Add(
            //         new RenameTableOperation
            //         {
            //             Name = "ef_temp_" + rebuild.Key.Table,
            //             Schema = rebuild.Key.Schema,
            //             NewName = rebuild.Key.Table,
            //             NewSchema = rebuild.Key.Schema
            //         });
            // }
            //
            // if (rebuilds.Any())
            // {
            //     operations.Add(
            //         new SqlOperation { Sql = "PRAGMA foreign_keys = 1;", SuppressTransaction = true });
            // }
            //
            // foreach (var index in indexesToRebuild)
            // {
            //     operations.Add(CreateIndexOperation.CreateFrom(index));
            // }

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
        ///     Builds commands for the given <see cref="AddColumnOperation" /> by making calls on the given
        ///     <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
        /// <param name="builder">The command builder to use to build the commands.</param>
        /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
        protected override void Generate(AddColumnOperation operation, IModel? model, MigrationCommandListBuilder builder, bool terminate)
        {
            base.Generate(operation, model, builder, terminate);
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
                if (columnOp != null && string.IsNullOrEmpty(operation.PrimaryKey.Name))
                {
                    columnOp.AddAnnotation(NuoDbAnnotationNames.InlinePrimaryKey, true);
                    // if (!string.IsNullOrEmpty(operation.PrimaryKey.Name))
                    // {
                    //     columnOp.AddAnnotation(NuoDbAnnotationNames.InlinePrimaryKeyName, operation.PrimaryKey.Name);
                    // }
            
                    operation.PrimaryKey = null;
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
                var autoincrement = operation[NuoDbAnnotationNames.Autoincrement] as bool?
                    ?? operation[NuoDbAnnotationNames.LegacyAutoincrement] as bool?;
                if (autoincrement == true)
                {
                    builder.Append(" GENERATED BY DEFAULT");
                    builder.Append(" AS IDENTITY");
                }


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


       

        // /// <summary>
        // ///     Throws <see cref="NotSupportedException" /> since this operation requires table rebuilds, which
        // ///     are not yet supported.
        // /// </summary>
        // /// <param name="operation">The operation.</param>
        // /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
        // /// <param name="builder">The command builder to use to build the commands.</param>
        // protected override void Generate(AddCheckConstraintOperation operation, IModel? model, MigrationCommandListBuilder builder)
        //     => throw new NotSupportedException(
        //         NuoDbStrings.InvalidMigrationOperation(operation.GetType().ShortDisplayName()));

        // /// <summary>
        // ///     Throws <see cref="NotSupportedException" /> since this operation requires table rebuilds, which
        // ///     are not yet supported.
        // /// </summary>
        // /// <param name="operation">The operation.</param>
        // /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
        // /// <param name="builder">The command builder to use to build the commands.</param>
        // /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
        // protected override void Generate(
        //     DropColumnOperation operation,
        //     IModel? model,
        //     MigrationCommandListBuilder builder,
        //     bool terminate = true)
        //     => throw new NotSupportedException(
        //         NuoDbStrings.InvalidMigrationOperation(operation.GetType().ShortDisplayName()));

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
            

        /// <summary>
        ///     Throws <see cref="NotSupportedException" /> since this operation requires table rebuilds, which
        ///     are not yet supported.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
        /// <param name="builder">The command builder to use to build the commands.</param>
        /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
        // protected override void Generate(
        //     DropPrimaryKeyOperation operation,
        //     IModel? model,
        //     MigrationCommandListBuilder builder,
        //     bool terminate = true)
        //     => throw new NotSupportedException(
        //         NuoDbStrings.InvalidMigrationOperation(operation.GetType().ShortDisplayName()));

        /// <summary>
        ///     Throws <see cref="NotSupportedException" /> since this operation requires table rebuilds, which
        ///     are not yet supported.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
        /// <param name="builder">The command builder to use to build the commands.</param>
        protected override void Generate(DropUniqueConstraintOperation operation, IModel? model, MigrationCommandListBuilder builder)
        {
            base.Generate(operation, model, builder);
        }

        // /// <summary>
        // ///     Throws <see cref="NotSupportedException" /> since this operation requires table rebuilds, which
        // ///     are not yet supported.
        // /// </summary>
        // /// <param name="operation">The operation.</param>
        // /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
        // /// <param name="builder">The command builder to use to build the commands.</param>
        // protected override void Generate(DropCheckConstraintOperation operation, IModel? model, MigrationCommandListBuilder builder)
        //     => throw new NotSupportedException(
        //         NuoDbStrings.InvalidMigrationOperation(operation.GetType().ShortDisplayName()));


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
        ///     Generates a SQL fragment to drop default constraints for a column.
        /// </summary>
        /// <param name="schema">The schema that contains the table.</param>
        /// <param name="tableName">The table that contains the column.</param>
        /// <param name="columnName">The column.</param>
        /// <param name="builder">The command builder to use to add the SQL fragment.</param>
        protected virtual void DropDefaultConstraint(
            string? schema,
            string tableName,
            string columnName,
            MigrationCommandListBuilder builder)
        {
            Check.NotEmpty(tableName, nameof(tableName));
            Check.NotEmpty(columnName, nameof(columnName));
            Check.NotNull(builder, nameof(builder));

            var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

            var variable = "@var" + _variableCounter++;

            builder
                .Append("DECLARE ")
                .Append(variable)
                .AppendLine(" sysname;")
                .Append("SELECT ")
                .Append(variable)
                .AppendLine(" = [d].[name]")
                .AppendLine("FROM [sys].[default_constraints] [d]")
                .AppendLine(
                    "INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]")
                .Append("WHERE ([d].[parent_object_id] = OBJECT_ID(")
                .Append(
                    stringTypeMapping.GenerateSqlLiteral(
                        Dependencies.SqlGenerationHelper.DelimitIdentifier(tableName, schema)))
                .Append(") AND [c].[name] = ")
                .Append(stringTypeMapping.GenerateSqlLiteral(columnName))
                .AppendLine(");")
                .Append("IF ")
                .Append(variable)
                .Append(" IS NOT NULL EXEC(")
                .Append(
                    stringTypeMapping.GenerateSqlLiteral(
                        "ALTER TABLE " + Dependencies.SqlGenerationHelper.DelimitIdentifier(tableName, schema) + " DROP CONSTRAINT ["))
                .Append(" + ")
                .Append(variable)
                .Append(" + ']")
                .Append(Dependencies.SqlGenerationHelper.StatementTerminator)
                .Append("')")
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
        }

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
