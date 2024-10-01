// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using NuoDb.Data.Client;
using NuoDb.EntityFrameworkCore.NuoDb.Extensions;
using NuoDb.EntityFrameworkCore.NuoDb.Extensions.Internal;
using NuoDb.EntityFrameworkCore.NuoDb.Internal;

namespace NuoDb.EntityFrameworkCore.NuoDb.Scaffolding.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class NuoDbDatabaseModelFactory : DatabaseModelFactory
    {
        private readonly IDiagnosticsLogger<DbLoggerCategory.Scaffolding> _logger;
        private readonly IRelationalTypeMappingSource _typeMappingSource;

        private static readonly ISet<string> _dateTimePrecisionTypes = new HashSet<string>
        {
            "datetimeoffset",
            "time"
        };

        private static readonly ISet<string> _maxLengthRequiredTypes
            = new HashSet<string>
            {
                "binary",
                "varbinary",
                "char",
                "varchar",
                "nchar",
                "nvarchar"
            };

        private const string _namePartRegex
            = @"(?:(?:\[(?<part{0}>(?:(?:\]\])|[^\]])+)\])|(?<part{0}>[^\.\[\]]+))";

        private static readonly Regex _partExtractor
            = new(
                string.Format(
                    CultureInfo.InvariantCulture,
                    @"^{0}(?:\.{1})?$",
                    string.Format(CultureInfo.InvariantCulture, _namePartRegex, 1),
                    string.Format(CultureInfo.InvariantCulture, _namePartRegex, 2)),
                RegexOptions.Compiled,
                TimeSpan.FromMilliseconds(1000));

        private static (string? Schema, string Table) Parse(string table)
        {
            var match = _partExtractor.Match(table.Trim());

            if (!match.Success)
            {
                throw new InvalidOperationException("Invalid Table");
            }

            var part1 = match.Groups["part1"].Value.Replace("]]", "]");
            var part2 = match.Groups["part2"].Value.Replace("]]", "]");

            return string.IsNullOrEmpty(part2) ? (null, part1) : (part1, part2);
        }
        private static string EscapeLiteral(string s)
            => $"'{s.Replace("'", "''")}'";

        private static Func<string, string>? GenerateSchemaFilter(IReadOnlyList<string> schemas)
        {
            return schemas.Count > 0
                ? (s =>
                {
                    var schemaFilterBuilder = new StringBuilder();
                    schemaFilterBuilder.Append(s);
                    schemaFilterBuilder.Append(" IN (");
                    schemaFilterBuilder.AppendJoin(", ", schemas.Select(EscapeLiteral));
                    schemaFilterBuilder.Append(')');
                    return schemaFilterBuilder.ToString();
                })
                : (s => $"{s} != 'SYSTEM'");
        }

         private static Func<string, string, string>? GenerateTableFilter(
            IReadOnlyList<(string? Schema, string Table)> tables,
            Func<string, string>? schemaFilter)
            => schemaFilter != null
                || tables.Count > 0
                    ? ((s, t) =>
                        {
                            var tableFilterBuilder = new StringBuilder();

                            var openBracket = false;
                            if (schemaFilter != null)
                            {
                                tableFilterBuilder
                                    .Append('(')
                                    .Append(schemaFilter(s));
                                openBracket = true;
                            }

                            if (tables.Count > 0)
                            {
                                if (openBracket)
                                {
                                    tableFilterBuilder
                                        .AppendLine()
                                        .Append("OR ");
                                }
                                else
                                {
                                    tableFilterBuilder.Append('(');
                                    openBracket = true;
                                }

                                var tablesWithoutSchema = tables.Where(e => string.IsNullOrEmpty(e.Schema)).ToList();
                                if (tablesWithoutSchema.Count > 0)
                                {
                                    tableFilterBuilder.Append(t);
                                    tableFilterBuilder.Append(" IN (");
                                    tableFilterBuilder.AppendJoin(", ", tablesWithoutSchema.Select(e => EscapeLiteral(e.Table)));
                                    tableFilterBuilder.Append(')');
                                }

                                var tablesWithSchema = tables.Where(e => !string.IsNullOrEmpty(e.Schema)).ToList();
                                if (tablesWithSchema.Count > 0)
                                {
                                    if (tablesWithoutSchema.Count > 0)
                                    {
                                        tableFilterBuilder.Append(" OR ");
                                    }

                                    tableFilterBuilder.Append(t);
                                    tableFilterBuilder.Append(" IN (");
                                    tableFilterBuilder.AppendJoin(", ", tablesWithSchema.Select(e => EscapeLiteral(e.Table)));
                                    tableFilterBuilder.Append(") AND (");
                                    tableFilterBuilder.Append(s);
                                    tableFilterBuilder.Append(" + N'.' + ");
                                    tableFilterBuilder.Append(t);
                                    tableFilterBuilder.Append(") IN (");
                                    tableFilterBuilder.AppendJoin(
                                        ", ", tablesWithSchema.Select(e => EscapeLiteral($"{e.Schema}.{e.Table}")));
                                    tableFilterBuilder.Append(')');
                                }
                            }

                            if (openBracket)
                            {
                                tableFilterBuilder.Append(')');
                            }

                            return tableFilterBuilder.ToString();
                        })
                    : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public NuoDbDatabaseModelFactory(
            IDiagnosticsLogger<DbLoggerCategory.Scaffolding> logger,
            IRelationalTypeMappingSource typeMappingSource)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(typeMappingSource, nameof(typeMappingSource));

            _logger = logger;
            _typeMappingSource = typeMappingSource;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override DatabaseModel Create(string connectionString, DatabaseModelFactoryOptions options)
        {
            Check.NotNull(connectionString, nameof(connectionString));
            Check.NotNull(options, nameof(options));

            using var connection = new NuoDbConnection(connectionString);
            return Create(connection, options);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override DatabaseModel Create(DbConnection connection, DatabaseModelFactoryOptions options)
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(options, nameof(options));
            var connStrBuilder = new NuoDbConnectionStringBuilder(connection.ConnectionString);
            var connSchema = connStrBuilder.Schema;

            var databaseModel = new DatabaseModel();
            var schemaList = options.Schemas.ToList();
            if (schemaList.Count < 1 && !string.IsNullOrEmpty(connSchema))
            {
                schemaList.Add(connSchema);
            }
            
            var schemaFilter = GenerateSchemaFilter(schemaList);
            var tableList = options.Tables.ToList();
            var tableFilter = GenerateTableFilter(tableList.Select(Parse).ToList(), schemaFilter);

            var connectionStartedOpen = connection.State == ConnectionState.Open;
            if (!connectionStartedOpen)
            {
                connection.Open();
            }

            try
            {
                
                databaseModel.DatabaseName = GetDatabaseName(connection);
                databaseModel.DefaultSchema = connStrBuilder.Schema;

                GetTables(connection, databaseModel, tableFilter);
                GetSequences(connection, databaseModel, schemaFilter);

                // foreach (var table in databaseModel.Tables)
                // {
                //     GetForeignKeys(connection, table, databaseModel.Tables, databaseModel.DefaultSchema);
                // }

                var nullableKeyColumns = databaseModel.Tables
                    .SelectMany(t => t.PrimaryKey?.Columns ?? Array.Empty<DatabaseColumn>())
                    .Concat(databaseModel.Tables.SelectMany(t => t.ForeignKeys).SelectMany(fk => fk.PrincipalColumns))
                    .Where(c => c.IsNullable)
                    .Distinct();
                foreach (var column in nullableKeyColumns)
                {
                    // TODO: Consider warning
                    column.IsNullable = false;
                }
            }
            catch (Exception e)
            {

            }
            finally
            {
                if (!connectionStartedOpen)
                {
                    connection.Close();
                }
            }

            return databaseModel;
        }

        private void GetSequences(
            DbConnection connection,
            DatabaseModel databaseModel,
            Func<string, string>? schemaFilter)
        {
            using var command = connection.CreateCommand();
            command.CommandText =
                @"select schema, sequencename from system.SEQUENCES s";
            if (schemaFilter != null)
            {
                command.CommandText += @"
WHERE " + schemaFilter("schema");
            }

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var schema = reader.GetValueOrDefault<string>("schema");
                var sequenceName = reader.GetValueOrDefault<string>("sequencename");
                _logger.SequenceFound(DisplayName(schema, sequenceName));

                var sequence = new DatabaseSequence
                {
                    Database = databaseModel,
                    Name = sequenceName,
                    Schema = schema,
                };
                databaseModel.Sequences.Add(sequence);
            }

        }

        private static string GetDatabaseName(DbConnection connection)
        {
            var name = Path.GetFileNameWithoutExtension(connection.DataSource);
            if (string.IsNullOrEmpty(name))
            {
                name = "Main";
            }

            return name;
        }

        private void GetTables(DbConnection connection, DatabaseModel databaseModel, Func<string, string, string>? tableFilter)
        {
            // var tablesToSelect = new HashSet<string>(tables.ToList(), StringComparer.OrdinalIgnoreCase);
            // var selectedTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var tables = new List<DatabaseTable>();


            using (var command = connection.CreateCommand())
            {
                var commandText = new StringBuilder("SELECT \"t\".\"SCHEMA\", \"t\".\"TABLENAME\", \"t\".\"TYPE\"")
                    .AppendLine(" FROM SYSTEM.TABLES \"t\"")
                    .Append("where \"Type\" in ('VIEW', 'TABLE')\r\n");
                var filter = "";
                if (tableFilter != null)
                {
                    filter = tableFilter("\"t\".\"SCHEMA\"", "\"t\".\"SCHEMA\"");
                    commandText.Append($"and {filter}");
                }

                // if (tablesToSelect.Any())
                // {
                //     commandText.Append($" and \"t\".\"TABLENAME\" in ({string.Join(",", tablesToSelect)})");
                // }
                // else
                // {
                //     commandText.Append(" and \"t\".\"TABLENAME\" != '").Append(HistoryRepository.DefaultTableName).Append("'");
                // }
                    
                command.CommandText = commandText.ToString();

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var schema = reader.GetValueOrDefault<string>("SCHEMA");
                    var name = reader.GetValueOrDefault<string>("TABLENAME");
                    // if (!AllowsTable(tablesToSelect, selectedTables, name))
                    // {
                    //     continue;
                    // }

                    _logger.TableFound(name);

                    var type = reader.GetValueOrDefault<string>("TYPE");
                    var table = type == "TABLE"
                        ? new DatabaseTable { Database = databaseModel, Name = name }
                        : new DatabaseView { Database = databaseModel, Name = name };
                    table.Schema = schema;

                    
                    // GetPrimaryKey(connection, table, schema);
                    // GetUniqueConstraints(connection, table, schema);
                    // GetIndexes(connection, table, schema);

                    tables.Add(table);
                }
                GetColumns(connection, tables, filter);
                GetIndexes(connection, tables, filter);
                GetForeignKeys(connection, tables, filter);
            }

            foreach (var table in tables)
            {
                databaseModel.Tables.Add(table);
            }
        }

        private static bool AllowsTable(HashSet<string> tables, HashSet<string> selectedTables, string name)
        {
            if (tables.Count == 0)
            {
                return true;
            }

            if (tables.Contains(name))
            {
                selectedTables.Add(name);
                return true;
            }

            return false;
        }

        // TODO: Implement
        private void GetColumns(
            DbConnection connection,
            IReadOnlyList<DatabaseTable> tables,
            string tableFilter)
        {


            using var command = connection.CreateCommand();
            var sql = @"SELECT 
         f.SCHEMA, f.TABLENAME,
          F.field AS Name,
          f.COMPUTEDEXPRESSION,
          F.fieldId AS Ordinal,
          (MOD(F.flags,2) = 0) AS IsNullable,
          case
            when F.domainname is not null and F.domainname <> '' then F.schema || '.' || F.domainname
            else (SELECT name FROM SYSTEM.DATATYPES WHERE Id = F.datatype)
          end AS TypeName,
          F.length AS MaxLength,
          F.precision AS Precision,
          0 AS DateTimePrecision,
          F.scale,
          '' AS CollationCatalog,
          '' AS CollationSchema,
          '' AS CollationName,
          '' AS CharacterSetCatalog,
          '' AS CharacterSetSchema,
          '' AS CharacterSetName,
          false AS IsMultiSet,
          case
            when F.generator_sequence is not null and F.generator_sequence like '%IDENTITY_SEQUENCE' then true
            else false
          end AS IsIdentity,
          (F.flags >= 8) AS IsStoreGenerated,
          F.defaultvalue
        FROM SYSTEM.FIELDS as F
        INNER JOIN SYSTEM.TABLES as T ON (T.schema != 'SYSTEM' AND T.type = 'TABLE' AND F.schema = T.schema AND F.tableName = T.tableName)
        WHERE ";
            sql += tableFilter;
            sql += @" ORDER BY F.SCHEMA, F.TABLENAME, F.FIELDID";

            command.CommandText = sql;

            using var reader = command.ExecuteReader();
            //var groups = reader.Cast<DbDataRecord>().ToList();
            var tableColumnGroups = reader.Cast<DbDataRecord>()
                .GroupBy(
                    ddr => (tableSchema: ddr.GetValueOrDefault<string>("schema"),
                        tableName: ddr.GetFieldValue<string>("tablename"))).ToList();

            foreach (var tableColumnGroup in tableColumnGroups)
            {
                var tableSchema = tableColumnGroup.Key.tableSchema;
                var tableName = tableColumnGroup.Key.tableName;

                var table = tables.Single(t => t.Schema == tableSchema && t.Name == tableName);

                foreach (var dataRecord in tableColumnGroup)
                {
                    var columnName = dataRecord.GetFieldValue<string>("NAME");
                    var ordinal = dataRecord.GetFieldValue<int>("ORDINAL");
                    var dataTypeName = dataRecord.GetValueOrDefault<string>("TYPENAME");
                    var maxLength = dataRecord.GetValueOrDefault<int>("MAXLENGTH");
                    var precision = dataRecord.GetValueOrDefault<int>("PRECISION");
                    var scale = dataRecord.GetValueOrDefault<int>("SCALE");
                    var nullable = dataRecord.GetValueOrDefault<bool>("ISNULLABLE");
                    var isIdentity = dataRecord.GetValueOrDefault<bool>("ISIDENTITY");
                    var defaultValue = dataRecord.GetValueOrDefault<string>("DEFAULTVALUE");
                    var computedValue = dataRecord.GetValueOrDefault<string>("COMPUTEDEXPRESSION");
                    var isStoreGenerated = dataRecord.GetValueOrDefault<bool>("ISSTOREGENERATED");



                    _logger.ColumnFound(
                        table.Name,
                        columnName,
                        dataTypeName, !nullable, defaultValue);
                
                    table.Columns.Add(
                        new DatabaseColumn
                        {
                            Table = table,
                            Name = columnName,
                            StoreType = GetStoreType(dataTypeName, maxLength, precision, scale),
                            IsNullable = nullable,
                            DefaultValueSql = defaultValue,
                            ValueGenerated = isStoreGenerated
                                ? ValueGenerated.OnAdd
                                : default(ValueGenerated?),
                            ComputedColumnSql = computedValue
                        });
                }
            }
        }

        private static string GetStoreType(string dataTypeName, int maxLength, int precision, int scale)
        {
            if (dataTypeName == "timestamp without timezone")
            {
                return "rowversion";
            }

            if (dataTypeName == "decimal"
                || dataTypeName == "numeric")
            {
                return $"{dataTypeName}({precision}, {scale})";
            }

            if (_dateTimePrecisionTypes.Contains(dataTypeName)
                && scale != 7)
            {
                return $"{dataTypeName}({scale})";
            }

            if (_maxLengthRequiredTypes.Contains(dataTypeName))
            {
                if (maxLength == -1)
                {
                    return $"{dataTypeName}(max)";
                }

                if (dataTypeName == "nvarchar"
                    || dataTypeName == "nchar")
                {
                    maxLength /= 2;
                }

                return $"{dataTypeName}({maxLength})";
            }

            return dataTypeName;
        }

        private string? FilterClrDefaults(string dataType, bool notNull, string defaultValue)
        {
            if (string.Equals(defaultValue, "null", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (notNull
                && defaultValue == "0"
                && _typeMappingSource.FindMapping(dataType)?.ClrType.IsNumeric() == true)
            {
                return null;
            }

            return defaultValue;
        }

        // private void GetPrimaryKeys(DbConnection connection, IReadOnlyList<DatabaseTable> tables, string tableFilter)
        // {
        //     using var command = connection.CreateCommand();
        //     var sql = @" SELECT 
        //   schema || '.' || tablename || '.' || indexname AS Id,
        //   schema || '.' || tablename AS ParentId,
        //   indexId,
        //   indexname AS Name,
        //   false AS IsDeferrable,
        //   false AS IsInitiallyDeferred,
        //   case indextype
        //     when 0 then 'PRIMARY KEY'
        //     when 1 then 'UNIQUE'
        //   end AS ConstraintType
        // FROM SYSTEM.INDEXES
        // WHERE
        //   schema != 'SYSTEM' AND (indextype = 0) 
        //   and ";
        //     sql += tableFilter;
        //
        //     
        //     using var reader = command.ExecuteReader();
        //     if (reader.Read())
        //     {
        //         var name = reader.GetValueOrDefault<string>("NAME");
        //         _logger.PrimaryKeyFound(name, table.Name);
        //         var primaryKey = new DatabasePrimaryKey
        //         {
        //             Table = table,
        //             Name = name
        //         };
        //
        //         using var command2 = connection.CreateCommand();
        //         command2.CommandText =
        //             "select field from system.indexfields where indexname = @indexname and schema = @schema order by position";
        //         var indexParam = command2.CreateParameter();
        //         indexParam.ParameterName = "@indexname";
        //         indexParam.Value = name;
        //         command2.Parameters.Add(indexParam);
        //
        //         command2.Parameters.Add(schemaParam);
        //
        //         using var colreader = command2.ExecuteReader();
        //         while (colreader.Read())
        //         {
        //             var columnName = colreader.GetValueOrDefault<string>("field");
        //             var column = table.Columns.FirstOrDefault(c => c.Name == columnName)
        //                          ?? table.Columns.FirstOrDefault(c => c.Name!.Equals(columnName, StringComparison.OrdinalIgnoreCase));
        //             Check.DebugAssert(column != null, "column is null.");
        //             primaryKey.Columns.Add(column);
        //         }
        //
        //
        //
        //         table.PrimaryKey = primaryKey;
        //     }
        // }

        private static void GetRowidPrimaryKey(
            DbConnection connection,
            DatabaseTable table)
        {
            using var command = connection.CreateCommand();
            command.CommandText = new StringBuilder()
                .AppendLine("SELECT \"name\"")
                .AppendLine("FROM pragma_table_info(@table)")
                .AppendLine("WHERE \"pk\" = 1;")
                .ToString();

            var parameter = command.CreateParameter();
            parameter.ParameterName = "@table";
            parameter.Value = table.Name;
            command.Parameters.Add(parameter);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                return;
            }

            var columnName = reader.GetString(0);
            var column = table.Columns.FirstOrDefault(c => c.Name == columnName)
                ?? table.Columns.FirstOrDefault(c => c.Name!.Equals(columnName, StringComparison.OrdinalIgnoreCase));
            Check.DebugAssert(column != null, "column is null.");

            Check.DebugAssert(!reader.Read(), "Unexpected composite primary key.");

            table.PrimaryKey = new DatabasePrimaryKey
            {
                Table = table,
                Name = string.Empty,
                Columns = { column }
            };
        }

        private void GetUniqueConstraints(DbConnection connection, DatabaseTable table, string schema)
        {
            using var command = connection.CreateCommand();
            command.CommandText =
                @" SELECT 
          schema || '.' || tablename || '.' || indexname AS Id,
          schema || '.' || tablename AS ParentId,
          indexId,
          indexname AS Name,
          false AS IsDeferrable,
          false AS IsInitiallyDeferred,
          case indextype
            when 0 then 'PRIMARY KEY'
            when 1 then 'UNIQUE'
          end AS ConstraintType
        FROM SYSTEM.INDEXES
        WHERE
          schema != 'SYSTEM' AND (indextype = 4) 
          and tablename = @table
          and schema = @schema";

            var parameter = command.CreateParameter();
            parameter.ParameterName = "@table";
            parameter.Value = table.Name;
            command.Parameters.Add(parameter);

            var schemaParam = new NuoDbParameter();
            schemaParam.ParameterName = "@schema";
            schemaParam.Value= schema;
            schemaParam.DbType = DbType.String;
            command.Parameters.Add(schemaParam);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var name = reader.GetValueOrDefault<string>("NAME");
                _logger.UniqueConstraintFound(name, table.Name);
                var constraint = new DatabaseUniqueConstraint()
                {
                    Table = table,
                    Name = name
                };

                using var command2 = connection.CreateCommand();
                command2.CommandText =
                    "select field from system.indexfields where indexname = @indexname order by position";
                var indexParam = command2.CreateParameter();
                indexParam.ParameterName = "@indexname";
                indexParam.Value = name;
                command2.Parameters.Add(indexParam);

                using var colreader = command2.ExecuteReader();
                while (colreader.Read())
                {
                    var columnName = colreader.GetValueOrDefault<string>("field");
                    var column = table.Columns.FirstOrDefault(c => c.Name == columnName)
                                 ?? table.Columns.FirstOrDefault(c =>
                                     c.Name!.Equals(columnName, StringComparison.OrdinalIgnoreCase));
                    Check.DebugAssert(column != null, "column is null.");
                    constraint.Columns.Add(column);
                }



                table.UniqueConstraints.Add(constraint);
            }
        }

        private void GetIndexes(DbConnection connection, IReadOnlyList<DatabaseTable> tables, string tableFilter)
        {
            using var command = connection.CreateCommand();
            var sql  = 
                @"SELECT 
         i.TABLENAME,
         i.SCHEMA,
         i.indexId,
          i.indexname AS Name,
          false AS IsDeferrable,
          false AS IsInitiallyDeferred,
          (i.INDEXTYPE = 0) as is_primarykey,
          (i.INDEXTYPE = 4) as is_unique_constraint,
          (i.INDEXTYPE = 1) as is_unique,
          idxf.field as column_name,
          case i.indextype
            when 0 then 'PRIMARY KEY'
            when 1 then 'UNIQUE INDEX'
            when 2 then 'INDEX'
            when 4 then 'UNIQUE CONSTRAINT'
          end AS ConstraintType
        FROM SYSTEM.INDEXES i
        inner join system.tables ""t"" on i.tablename = t.tablename and i.schema = t.schema
        inner join system.indexfields idxf on idxf.INDEXNAME = i.INDEXNAME and idxf.SCHEMA = t.schema
        WHERE
          i.schema != 'SYSTEM' AND i.indextype in (0, 1,2, 4)
          and ";
            sql += tableFilter;

            command.CommandText = sql;
            
      

            using var reader = command.ExecuteReader();
            var tableIndexGroups = reader.Cast<DbDataRecord>()
                .GroupBy(
                    ddr => (tableSchema: ddr.GetValueOrDefault<string>("SCHEMA"),
                        tableName: ddr.GetFieldValue<string>("TABLENAME"))).ToList();
            foreach (var tableIndexGroup in tableIndexGroups)
            {
                var tableSchema = tableIndexGroup.Key.tableSchema;
                var tableName = tableIndexGroup.Key.tableName;

                var table = tables.Single(t => t.Schema == tableSchema && t.Name == tableName);

                var primaryKeyGroups = tableIndexGroup
                    .Where(ddr => ddr.GetValueOrDefault<bool>("is_primarykey") )
                    .GroupBy(
                        ddr =>
                            (Name: ddr.GetFieldValue<string>("name"),
                                TypeDesc: ddr.GetValueOrDefault<string>("constrainttype")))
                    .ToArray();

                Check.DebugAssert(primaryKeyGroups.Length == 0 || primaryKeyGroups.Length == 1, "Multiple primary keys found");

                if (primaryKeyGroups.Length == 1)
                {
                    if (TryGetPrimaryKey(primaryKeyGroups[0], out var primaryKey))
                    {
                        _logger.PrimaryKeyFound(primaryKey.Name!, DisplayName(tableSchema, tableName));
                        table.PrimaryKey = primaryKey;
                    }
                }

                var uniqueConstraintGroups = tableIndexGroup
                    .Where(ddr => ddr.GetValueOrDefault<bool>("is_unique_constraint"))
                    .GroupBy(
                        ddr =>
                            (Name: ddr.GetValueOrDefault<string>("name"),
                                TypeDesc: ddr.GetValueOrDefault<string>("constrainttype")))
                    .ToArray();

                foreach (var uniqueConstraintGroup in uniqueConstraintGroups)
                {
                    if (TryGetUniqueConstraint(uniqueConstraintGroup, out var uniqueConstraint))
                    {
                        _logger.UniqueConstraintFound(uniqueConstraintGroup.Key.Name!, DisplayName(tableSchema, tableName));
                        table.UniqueConstraints.Add(uniqueConstraint);
                    }
                }

                var indexGroups = tableIndexGroup
                    .Where(
                        ddr => !ddr.GetValueOrDefault<bool>("is_primarykey")
                               && !ddr.GetValueOrDefault<bool>("is_unique_constraint"))
                    .GroupBy(
                        ddr =>
                            (Name: ddr.GetValueOrDefault<string>("name"),
                                TypeDesc: ddr.GetValueOrDefault<string>("constrainttype"),
                                IsUnique: ddr.GetValueOrDefault<bool>("is_unique")))
                    .ToArray();

                foreach (var indexGroup in indexGroups)
                {
                    if (TryGetIndex(indexGroup, out var index))
                    {
                        _logger.IndexFound(indexGroup.Key.Name!, DisplayName(tableSchema, tableName), indexGroup.Key.IsUnique);
                        table.Indexes.Add(index);
                    }
                }
                bool TryGetIndex(
                    IGrouping<(string? Name, string? TypeDesc, bool IsUnique),
                        DbDataRecord> indexGroup,
                    [NotNullWhen(true)] out DatabaseIndex? index)
                {
                    index = new DatabaseIndex
                    {
                        Table = table,
                        Name = indexGroup.Key.Name,
                        IsUnique = indexGroup.Key.IsUnique
                    };


                    foreach (var dataRecord in indexGroup)
                    {
                        var columnName = dataRecord.GetValueOrDefault<string>("column_name");
                        var column = table.Columns.FirstOrDefault(c => c.Name == columnName)
                                     ?? table.Columns.FirstOrDefault(
                                         c => c.Name!.Equals(columnName, StringComparison.OrdinalIgnoreCase));

                        if (column is null)
                        {
                            return false;
                        }

                        index.Columns.Add(column);
                    }

                    return index.Columns.Count > 0;
                }

                bool TryGetUniqueConstraint(
                    IGrouping<(string? Name, string? TypeDesc), DbDataRecord> uniqueConstraintGroup,
                    [NotNullWhen(true)] out DatabaseUniqueConstraint? uniqueConstraint)
                {
                    uniqueConstraint = new DatabaseUniqueConstraint { Table = table, Name = uniqueConstraintGroup.Key.Name };

                    foreach (var dataRecord in uniqueConstraintGroup)
                    {
                        var columnName = dataRecord.GetValueOrDefault<string>("column_name");
                        var column = table.Columns.FirstOrDefault(c => c.Name == columnName)
                                     ?? table.Columns.FirstOrDefault(
                                         c => c.Name!.Equals(columnName, StringComparison.OrdinalIgnoreCase));

                        if (column is null)
                        {
                            return false;
                        }

                        uniqueConstraint.Columns.Add(column);
                    }

                    return true;
                }
                bool TryGetPrimaryKey(
                    IGrouping<(string Name, string? TypeDesc), DbDataRecord> primaryKeyGroup,
                    [NotNullWhen(true)] out DatabasePrimaryKey? primaryKey)
                {
                    primaryKey = new DatabasePrimaryKey { Table = table, Name = primaryKeyGroup.Key.Name };

                   

                    foreach (var dataRecord in primaryKeyGroup)
                    {
                        var columnName = dataRecord.GetValueOrDefault<string>("column_name");
                        var column = table.Columns.FirstOrDefault(c => c.Name == columnName)
                                     ?? table.Columns.FirstOrDefault(
                                         c => c.Name!.Equals(columnName, StringComparison.OrdinalIgnoreCase));

                        if (column is null)
                        {
                            return false;
                        }

                        primaryKey.Columns.Add(column);
                    }

                    return true;
                }
            }
            // while (reader.Read())
            // {
            //     var name = reader.GetValueOrDefault<string>("NAME");
            //     var isUnique = reader.GetValueOrDefault<string>("CONSTRAINTTYPE") == "UNIQUE";
            //     _logger.IndexFound(name, table.Name, isUnique);
            //     var index = new DatabaseIndex()
            //     {
            //         Table = table,
            //         Name = name,
            //         IsUnique = isUnique
            //     };
            //
            //     using var command2 = connection.CreateCommand();
            //     command2.CommandText =
            //         "select field from system.indexfields where indexname = @indexname and schema=@schema order by position";
            //     var indexParam = command2.CreateParameter();
            //     indexParam.ParameterName = "@indexname";
            //     indexParam.Value = name;
            //     command2.Parameters.Add(indexParam);
            //     command2.Parameters.Add(schemaParam);
            //
            //     using var colreader = command2.ExecuteReader();
            //     while (colreader.Read())
            //     {
            //         var columnName = colreader.GetValueOrDefault<string>("field");
            //         var column = table.Columns.FirstOrDefault(c => c.Name == columnName)
            //                      ?? table.Columns.FirstOrDefault(c =>
            //                          c.Name!.Equals(columnName, StringComparison.OrdinalIgnoreCase));
            //         Check.DebugAssert(column != null, "column is null.");
            //         index.Columns.Add(column);
            //     }
            //
            //     table.Indexes.Add(index);
            // }
        }
        private static string DisplayName(string? schema, string name)
            => (!string.IsNullOrEmpty(schema) ? schema + "." : "") + name;
        private void GetForeignKeys(DbConnection connection, IReadOnlyList<DatabaseTable> tables, string tableFilter)
        {
            using var command = connection.CreateCommand();
            var sql  =
                @"select 
    fk.FOREIGNKEYNAME as name,
    t.TABLENAME as local_table_name,
    (select f.field from SYSTEM.FIELDS f where fk.PRIMARYFIELDID = f.FIELDID and f.TABLENAME = frg.TABLENAME and f.SCHEMA = frg.SCHEMA) as principal_field_name,
    frg.TABLENAME as principal_table_name,
    (select f.field from SYSTEM.FIELDS f where fk.FOREIGNFIELDID = f.FIELDID and f.tablename = t.TABLENAME and f.SCHEMA = t.SCHEMA) as local_field_name,
    frg.SCHEMA as principal_table_schema,
    t.schema as local_table_schema,
fk.deleterule
from system.FOREIGNKEYS fk
inner join system.tables frg on fk.PRIMARYTABLEID = frg.TABLEID
inner join system.tables t on fk.FOREIGNTABLEID = t.TABLEID
where ";
            sql += tableFilter;

            command.CommandText = sql;
               

            using var reader = command.ExecuteReader();
             var tableForeignKeyGroups = reader.Cast<DbDataRecord>()
                .GroupBy(
                    ddr => (tableSchema: ddr.GetValueOrDefault<string>("local_table_schema"),
                        tableName: ddr.GetFieldValue<string>("local_table_name"))).ToList();

            foreach (var tableForeignKeyGroup in tableForeignKeyGroups)
            {
                var tableSchema = tableForeignKeyGroup.Key.tableSchema;
                var tableName = tableForeignKeyGroup.Key.tableName;

                var table = tables.Single(t => t.Schema == tableSchema && t.Name == tableName);

                var foreignKeyGroups = tableForeignKeyGroup
                    .GroupBy(
                        c => (Name: c.GetValueOrDefault<string>("name"),
                            PrincipalTableSchema: c.GetValueOrDefault<string>("principal_table_schema"),
                            PrincipalTableName: c.GetValueOrDefault<string>("principal_table_name"),
                            OnDeleteAction: c.GetValueOrDefault<int>("deleterule")));

                foreach (var foreignKeyGroup in foreignKeyGroups)
                {
                    var fkName = foreignKeyGroup.Key.Name;
                    var principalTableSchema = foreignKeyGroup.Key.PrincipalTableSchema;
                    var principalTableName = foreignKeyGroup.Key.PrincipalTableName;
                    var onDeleteAction = foreignKeyGroup.Key.OnDeleteAction;

                    if (principalTableName == null)
                    {
                        // _logger.ForeignKeyReferencesUnknownPrincipalTableWarning(
                        //     fkName,
                        //     DisplayName(table.Schema, table.Name));

                        continue;
                    }

                    _logger.ForeignKeyFound(
                        fkName!,
                        DisplayName(table.Schema, table.Name),
                        DisplayName(principalTableSchema, principalTableName),
                        onDeleteAction!.ToString());

                    var principalTable = tables.FirstOrDefault(
                            t => t.Schema == principalTableSchema
                                && t.Name == principalTableName)
                        ?? tables.FirstOrDefault(
                            t => t.Schema?.Equals(principalTableSchema, StringComparison.OrdinalIgnoreCase) == true
                                && t.Name.Equals(principalTableName, StringComparison.OrdinalIgnoreCase));

                    if (principalTable == null)
                    {
                        // _logger.ForeignKeyReferencesMissingPrincipalTableWarning(
                        //     fkName,
                        //     DisplayName(table.Schema, table.Name),
                        //     DisplayName(principalTableSchema, principalTableName));

                        continue;
                    }

                    var foreignKey = new DatabaseForeignKey
                    {
                        Table = table,
                        Name = fkName,
                        PrincipalTable = principalTable,
                        OnDelete = ConvertToReferentialAction(onDeleteAction)
                    };

                    var invalid = false;

                    foreach (var dataRecord in foreignKeyGroup)
                    {
                        var columnName = dataRecord.GetValueOrDefault<string>("local_field_name");
                        var column = table.Columns.FirstOrDefault(c => c.Name == columnName)
                            ?? table.Columns.FirstOrDefault(
                                c => c.Name!.Equals(columnName, StringComparison.OrdinalIgnoreCase));
                        Check.DebugAssert(column != null, "column is null.");

                        var principalColumnName = dataRecord.GetValueOrDefault<string>("principal_field_name");
                        var principalColumn = foreignKey.PrincipalTable.Columns.FirstOrDefault(c => c.Name == principalColumnName)
                            ?? foreignKey.PrincipalTable.Columns.FirstOrDefault(
                                c => c.Name!.Equals(principalColumnName, StringComparison.OrdinalIgnoreCase));
                        if (principalColumn == null)
                        {
                            invalid = true;
                            _logger.ForeignKeyPrincipalColumnMissingWarning(
                                fkName!,
                                DisplayName(table.Schema, table.Name!),
                                principalColumnName!,
                                DisplayName(principalTableSchema, principalTableName));
                            break;
                        }

                        foreignKey.Columns.Add(column);
                        foreignKey.PrincipalColumns.Add(principalColumn);
                    }

                    if (!invalid)
                    {
                        if (foreignKey.Columns.SequenceEqual(foreignKey.PrincipalColumns))
                        {
                            // _logger.ReflexiveConstraintIgnored(
                            //     foreignKey.Name!,
                            //     DisplayName(table.Schema, table.Name!));
                        }
                        else
                        {
                            var duplicated = table.ForeignKeys
                                .FirstOrDefault(
                                    k => k.Columns.SequenceEqual(foreignKey.Columns)
                                        && k.PrincipalTable.Equals(foreignKey.PrincipalTable));
                            if (duplicated != null)
                            {
                                // _logger.DuplicateForeignKeyConstraintIgnored(
                                //     foreignKey.Name!,
                                //     DisplayName(table.Schema, table.Name!),
                                //     duplicated.Name!);
                                continue;
                            }

                            table.ForeignKeys.Add(foreignKey);
                        }
                    }
                }
           
            }
        }

        private static ReferentialAction? ConvertToReferentialAction(int value)
        {
            switch (value)
            {
                case 0:
                    return ReferentialAction.Cascade;

                case 1:
                    return ReferentialAction.Restrict;

                case 2:
                    return ReferentialAction.SetNull;

                case 3:
                    return ReferentialAction.NoAction;

                case 4:
                    return ReferentialAction.SetDefault;

                default:
                    Check.DebugAssert(value > 4, "Unexpected value: " + value);
                    return null;
            }
        }
    }
}
