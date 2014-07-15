/****************************************************************************
*  Author: Jiri Cincura (jiri@cincura.net)
*  Based on SqlCeMigrationSqlGenerator
****************************************************************************/

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Migrations.Sql;
using System.Globalization;
using System.Linq;
using System.Text;
using NuoDb.Data.Client.EntityFramework6.SqlGen;
using NuoDb.Data.Client.Util;

namespace NuoDb.Data.Client.EntityFramework6
{
	public class NuoDbMigrationSqlGenerator : MigrationSqlGenerator
	{
		public NuoDbMigrationSqlGenerator()
		{ }

		public override IEnumerable<MigrationStatement> Generate(IEnumerable<MigrationOperation> migrationOperations, string providerManifestToken)
		{
			InitializeProviderServices(providerManifestToken);
			return GenerateStatements(migrationOperations).ToArray();
		}

		void InitializeProviderServices(string providerManifestToken)
		{
			using (var connection = CreateConnection())
			{
				ProviderManifest = DbProviderServices.GetProviderServices(connection).GetProviderManifest(providerManifestToken);
			}
		}

		#region Operations

		protected IEnumerable<MigrationStatement> Generate(MigrationOperation operation)
		{
			throw new NotSupportedException(string.Format("Unknown operation '{0}'.", operation.GetType().FullName));
		}

		protected virtual IEnumerable<MigrationStatement> Generate(UpdateDatabaseOperation operation)
		{
			return GenerateStatements(operation.Migrations.SelectMany(x => x.Operations));
		}

		protected virtual IEnumerable<MigrationStatement> Generate(SqlOperation operation)
		{
			yield return Statement(operation.Sql, operation.SuppressTransaction);
		}

		protected virtual IEnumerable<MigrationStatement> Generate(AddColumnOperation operation)
		{
			using (var writer = SqlWriter())
			{
				writer.Write("ALTER TABLE ");
				writer.Write(Name(operation.Table));
				writer.Write(" ADD ");
				var column = operation.Column;
				writer.Write(Generate(column));
				if (column.IsNullable != null
					&& !column.IsNullable.Value
					&& column.DefaultValue == null
					&& string.IsNullOrWhiteSpace(column.DefaultValueSql)
					&& !column.IsIdentity
					&& !column.IsTimestamp)
				{
					writer.Write(" DEFAULT ");

					if (column.Type == PrimitiveTypeKind.DateTime)
					{
						writer.Write(Generate(DateTime.Parse("1970-01-01 00:00:00", CultureInfo.InvariantCulture)));
					}
					else
					{
						writer.Write(Generate((dynamic)column.ClrDefaultValue));
					}
				}
				yield return Statement(writer);
			}
		}

		protected virtual IEnumerable<MigrationStatement> Generate(AddForeignKeyOperation operation)
		{
			using (var writer = SqlWriter())
			{
				writer.Write("ALTER TABLE ");
				writer.Write(Name(operation.DependentTable));
				writer.Write(" ADD CONSTRAINT ");
				writer.Write(Quote(operation.Name));
				writer.Write(" FOREIGN KEY (");
				WriteColumns(writer, operation.DependentColumns.Select(Quote));
				writer.Write(") REFERENCES ");
				writer.Write(Name(operation.PrincipalTable));
				writer.Write(" (");
				WriteColumns(writer, operation.PrincipalColumns.Select(Quote));
				writer.Write(")");
				if (operation.CascadeDelete)
				{
					writer.Write(" ON DELETE CASCADE");
				}
				yield return Statement(writer);
			}
		}

		protected virtual IEnumerable<MigrationStatement> Generate(AddPrimaryKeyOperation operation)
		{
			using (var writer = SqlWriter())
			{
				writer.Write("ALTER TABLE ");
				writer.Write(Name(operation.Table));
				writer.Write(" ADD CONSTRAINT ");
				writer.Write(Quote(operation.Name));
				writer.Write(" PRIMARY KEY ");
				writer.Write("(");
				WriteColumns(writer, operation.Columns.Select(Quote));
				writer.Write(")");
				yield return Statement(writer);
			}
		}

		protected virtual IEnumerable<MigrationStatement> Generate(AlterColumnOperation operation)
		{
			var column = operation.Column;
			using (var writer = SqlWriter())
			{
				writer.Write("ALTER TABLE ");
				writer.Write(Name(operation.Table));
				writer.Write(" ALTER COLUMN ");
				writer.Write(Quote(column.Name));
				writer.Write(" ");
				writer.Write(BuildColumnType(column));
				if (column.IsNullable != null && !column.IsNullable.Value)
				{
					writer.Write(" NOT");
				}
				writer.Write(" NULL");
				yield return Statement(writer);
			}

			if (column.DefaultValue != null || !string.IsNullOrWhiteSpace(column.DefaultValueSql))
			{
				using (var writer = SqlWriter())
				{
					writer.Write("ALTER TABLE ");
					writer.Write(Name(operation.Table));
					writer.Write(" ALTER COLUMN ");
					writer.Write(Quote(column.Name));
					writer.Write(" DROP DEFAULT");
					yield return Statement(writer);
				}

				using (var writer = SqlWriter())
				{
					writer.Write("ALTER TABLE ");
					writer.Write(Name(operation.Table));
					writer.Write(" ALTER COLUMN ");
					writer.Write(Quote(column.Name));
					writer.Write(" SET DEFAULT ");
					writer.Write(column.DefaultValue != null ? Generate((dynamic)column.DefaultValue) : column.DefaultValueSql);
					yield return Statement(writer);
				}
			}
		}

		protected virtual IEnumerable<MigrationStatement> Generate(AlterProcedureOperation operation)
		{
			throw new NotImplementedException();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(AlterTableOperation operation)
		{
			// Nothing to do since there is no inherent semantics associated with annotations
			yield break;
		}

		protected virtual IEnumerable<MigrationStatement> Generate(CreateIndexOperation operation)
		{
			using (var writer = SqlWriter())
			{
				writer.Write("CREATE ");
				if (operation.IsUnique)
				{
					writer.Write("UNIQUE ");
				}
				writer.Write("INDEX ");
				writer.Write(Quote(operation.Name));
				writer.Write(" ON ");
				writer.Write(Name(operation.Table));
				writer.Write("(");
				WriteColumns(writer, operation.Columns);
				writer.Write(")");
				yield return Statement(writer);
			}
		}

		protected virtual IEnumerable<MigrationStatement> Generate(CreateProcedureOperation operation)
		{
			throw new NotImplementedException();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(CreateTableOperation operation)
		{
			using (var writer = SqlWriter())
			{
				writer.Write("CREATE TABLE ");
				writer.Write(Name(operation.Name));
				writer.Write(" (");
				writer.WriteLine();
				writer.Indent++;
				WriteColumns(writer, operation.Columns.Select(Generate), true);
				writer.Indent--;
				writer.WriteLine();
				writer.Write(")");
				yield return Statement(writer);
			}
			if (operation.PrimaryKey != null)
			{
				foreach (var item in Generate(operation.PrimaryKey))
					yield return item;
			}
		}

		protected virtual IEnumerable<MigrationStatement> Generate(DropColumnOperation operation)
		{
			using (var writer = SqlWriter())
			{
				writer.Write("ALTER TABLE ");
				writer.Write(Name(operation.Table));
				writer.Write(" DROP COLUMN ");
				writer.Write(Quote(operation.Name));
				yield return Statement(writer);
			}
		}

		protected virtual IEnumerable<MigrationStatement> Generate(DropForeignKeyOperation operation)
		{
			using (var writer = SqlWriter())
			{
				writer.Write("ALTER TABLE ");
				writer.Write(Name(operation.DependentTable));
				writer.Write(" DROP CONSTRAINT ");
				writer.Write(Quote(operation.Name));
				yield return Statement(writer);
			}
		}

		protected virtual IEnumerable<MigrationStatement> Generate(DropIndexOperation operation)
		{
			using (var writer = SqlWriter())
			{
				writer.Write("DROP INDEX ");
				writer.Write(Quote(operation.Name));
				writer.Write(" ON ");
				writer.Write(Name(operation.Table));
				yield return Statement(writer);
			}
		}

		protected virtual IEnumerable<MigrationStatement> Generate(DropPrimaryKeyOperation operation)
		{
#warning Does not work in NuoDB, yet
			using (var writer = SqlWriter())
			{
				writer.Write("ALTER TABLE ");
				writer.Write(Name(operation.Table));
				writer.Write(" DROP PRIMARY KEY");
				yield return Statement(writer);
			}
		}

		protected virtual IEnumerable<MigrationStatement> Generate(DropProcedureOperation operation)
		{
			throw new NotImplementedException();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(DropTableOperation operation)
		{
			using (var writer = SqlWriter())
			{
				writer.Write("DROP TABLE ");
				writer.Write(Name(operation.Name));
				yield return Statement(writer);
			}
		}

		protected virtual IEnumerable<MigrationStatement> Generate(HistoryOperation operation)
		{
#warning Finish
			return Enumerable.Empty<MigrationStatement>();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(MoveProcedureOperation operation)
		{
			throw new NotImplementedException();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(MoveTableOperation operation)
		{
			throw new NotSupportedException("'MoveTableOperation' is not supported.");
		}

		protected virtual IEnumerable<MigrationStatement> Generate(RenameColumnOperation operation)
		{
			using (var writer = SqlWriter())
			{
				writer.Write("ALTER TABLE ");
				writer.Write(Quote(operation.Table));
				writer.Write(" RENAME COLUMN ");
				writer.Write(Quote(operation.Name));
				writer.Write(" TO ");
				writer.Write(Quote(operation.NewName));
				yield return Statement(writer);
			}
		}

		protected virtual IEnumerable<MigrationStatement> Generate(RenameIndexOperation operation)
		{
			throw new NotSupportedException("'RenameIndexOperation' is not supported.");
		}

		protected virtual IEnumerable<MigrationStatement> Generate(RenameProcedureOperation operation)
		{
			throw new NotImplementedException();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(RenameTableOperation operation)
		{
			throw new NotSupportedException("'RenameTableOperation' is not supported.");
		}

		#endregion

		#region Models

		protected virtual string Generate(ColumnModel column)
		{
			var result = new StringBuilder();
			result.Append(Quote(column.Name));
			result.Append(" ");
			result.Append(BuildColumnType(column));

			if ((column.IsNullable != null)
				&& !column.IsNullable.Value)
			{
				result.Append(" NOT NULL");
			}

			if (column.DefaultValue != null)
			{
				result.Append(" DEFAULT ");
				result.Append(Generate((dynamic)column.DefaultValue));
			}
			else if (!string.IsNullOrWhiteSpace(column.DefaultValueSql))
			{
				result.Append(" DEFAULT ");
				result.Append(column.DefaultValueSql);
			}
			else if (column.IsIdentity)
			{
				if (column.Type == PrimitiveTypeKind.Guid && column.DefaultValue == null)
				{
					result.Append(" DEFAULT " + Guid.NewGuid().ToNuoDbString());
				}
				else
				{
					result.Append(" GENERATED BY DEFAULT AS IDENTITY");
				}
			}

			return result.ToString();
		}

		protected virtual string Generate(object defaultValue)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}", defaultValue);
		}

		protected virtual string Generate(DateTime defaultValue)
		{
			return SqlGenerator.FormatDateTime(defaultValue);
		}

		protected virtual string Generate(byte[] defaultValue)
		{
			return SqlGenerator.FormatBinary(defaultValue);
		}

		protected virtual string Generate(bool defaultValue)
		{
			return SqlGenerator.FormatBoolean(defaultValue);
		}

		protected virtual string Generate(Guid defaultValue)
		{
			return SqlGenerator.FormatGuid(defaultValue);
		}

		protected virtual string Generate(string defaultValue)
		{
			return SqlGenerator.FormatString(defaultValue);
		}

		protected virtual string Generate(TimeSpan defaultValue)
		{
			return SqlGenerator.FormatTime(defaultValue);
		}

		#endregion

		#region Helpers

		protected MigrationStatement Statement(string sql, bool suppressTransaction = false)
		{
			return new MigrationStatement
			{
				Sql = sql,
				SuppressTransaction = suppressTransaction,
				BatchTerminator = string.Empty
			};
		}

		protected static string Quote(string identifier)
		{
			return SqlGenerator.QuoteIdentifier(identifier);
		}

		protected static string Name(string name)
		{
			return name;
		}

		MigrationStatement Statement(SqlWriter sqlWriter, bool suppressTransaction = false)
		{
			return Statement(sqlWriter.ToString(), suppressTransaction);
		}

		SqlWriter SqlWriter()
		{
			var result = new SqlWriter(new StringBuilder());
			result.Indent++;
			return result;
		}

		string BuildColumnType(ColumnModel columnModel)
		{
			return BuildPropertyType(columnModel);
		}

		string BuildPropertyType(PropertyModel propertyModel)
		{
			var storeTypeName = propertyModel.StoreType;
			var typeUsage = ProviderManifest.GetStoreType(propertyModel.TypeUsage);
			if (!string.IsNullOrWhiteSpace(storeTypeName))
			{
				typeUsage = BuildStoreTypeUsage(storeTypeName, propertyModel) ?? typeUsage;
			}
			return SqlGenerator.GetSqlPrimitiveType(typeUsage);
		}

		IEnumerable<MigrationStatement> GenerateStatements(IEnumerable<MigrationOperation> operations)
		{
			return operations.Select<dynamic, IEnumerable<MigrationStatement>>(x => Generate(x)).SelectMany(x => x);
		}

		static DbConnection CreateConnection()
		{
			return DbConfiguration.DependencyResolver.GetService<DbProviderFactory>(NuoDbProviderServices.ProviderInvariantName).CreateConnection();
		}

		static void WriteColumns(SqlWriter writer, IEnumerable<string> columns, bool separateLines = false)
		{
			var separator = (string)null;
			foreach (var column in columns)
			{
				if (separator != null)
				{
					writer.Write(separator);
					if (separateLines)
						writer.WriteLine();
				}
				writer.Write(column);
				separator = ", ";
			}
		}

		#endregion
	}
}
