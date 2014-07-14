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
			var builder = new StringBuilder();
			builder.Append("ALTER TABLE ");
			builder.Append(Name(operation.Table));
			builder.Append(" ADD ");
			var column = operation.Column;
			builder.Append(Generate(column));
			if (column.IsNullable != null
				&& !column.IsNullable.Value
				&& column.DefaultValue == null
				&& string.IsNullOrWhiteSpace(column.DefaultValueSql)
				&& !column.IsIdentity
				&& !column.IsTimestamp)
			{
				builder.Append(" DEFAULT ");

				if (column.Type == PrimitiveTypeKind.DateTime)
				{
					builder.Append(Generate(DateTime.Parse("1970-01-01 00:00:00", CultureInfo.InvariantCulture)));
				}
				else
				{
					builder.Append(Generate((dynamic)column.ClrDefaultValue));
				}
			}

			yield return Statement(builder.ToString());
		}

		protected virtual IEnumerable<MigrationStatement> Generate(AddForeignKeyOperation operation)
		{
			var builder = new StringBuilder();
			builder.Append("ALTER TABLE ");
			builder.Append(Name(operation.DependentTable));
			builder.Append(" ADD CONSTRAINT ");
			builder.Append(Quote(operation.Name));
			builder.Append(" FOREIGN KEY (");
			builder.Append(JoinColumns(operation.DependentColumns.Select(Quote)));
			builder.Append(") REFERENCES ");
			builder.Append(Name(operation.PrincipalTable));
			builder.Append(" (");
			builder.Append(JoinColumns(operation.PrincipalColumns.Select(Quote)));
			builder.Append(")");
			if (operation.CascadeDelete)
			{
				builder.Append(" ON DELETE CASCADE");
			}
			yield return Statement(builder.ToString());
		}

		protected virtual IEnumerable<MigrationStatement> Generate(AddPrimaryKeyOperation operation)
		{
			var builder = new StringBuilder();
			builder.Append("ALTER TABLE ");
			builder.Append(Name(operation.Table));
			builder.Append(" ADD CONSTRAINT ");
			builder.Append(Quote(operation.Name));
			builder.Append(" PRIMARY KEY ");
			builder.Append("(");
			builder.Append(JoinColumns(operation.Columns.Select(Quote)));
			builder.Append(")");
			yield return Statement(builder.ToString());
		}

		protected virtual IEnumerable<MigrationStatement> Generate(AlterColumnOperation operation)
		{
			var column = operation.Column;
			var builder = new StringBuilder();
			builder.Append("ALTER TABLE ");
			builder.Append(Name(operation.Table));
			builder.Append(" ALTER COLUMN ");
			builder.Append(Quote(column.Name));
			builder.Append(" ");
			builder.Append(BuildColumnType(column));
			if (column.IsNullable != null && !column.IsNullable.Value)
			{
				builder.Append(" NOT");
			}
			builder.Append(" NULL");
			yield return Statement(builder.ToString());

			if (column.DefaultValue != null || !string.IsNullOrWhiteSpace(column.DefaultValueSql))
			{
				builder.Clear();
				builder.Append("ALTER TABLE ");
				builder.Append(Name(operation.Table));
				builder.Append(" ALTER COLUMN ");
				builder.Append(Quote(column.Name));
				builder.Append(" DROP DEFAULT");
				yield return Statement(builder.ToString());

				builder.Clear();
				builder.Append("ALTER TABLE ");
				builder.Append(Name(operation.Table));
				builder.Append(" ALTER COLUMN ");
				builder.Append(Quote(column.Name));
				builder.Append(" SET DEFAULT ");
				builder.Append(column.DefaultValue != null ? Generate((dynamic)column.DefaultValue) : column.DefaultValueSql);
				yield return Statement(builder.ToString());
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
			var builder = new StringBuilder();
			builder.Append("CREATE ");
			if (operation.IsUnique)
			{
				builder.Append("UNIQUE ");
			}
			builder.Append("INDEX ");
			builder.Append(Quote(operation.Name));
			builder.Append(" ON ");
			builder.Append(Name(operation.Table));
			builder.Append("(");
			builder.Append(JoinColumns(operation.Columns));
			builder.Append(")");
			yield return Statement(builder.ToString());
		}

		protected virtual IEnumerable<MigrationStatement> Generate(CreateProcedureOperation operation)
		{
			throw new NotImplementedException();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(CreateTableOperation operation)
		{
#warning Schemas
			//var databaseName = DatabaseName.Parse(operation.Name);

			//if (!string.IsNullOrWhiteSpace(databaseName.Schema))
			//{
			//	if (!databaseName.Schema.EqualsIgnoreCase("dbo")
			//		&& !_generatedSchemas.Contains(databaseName.Schema))
			//	{
			//		GenerateCreateSchema(databaseName.Schema);

			//		_generatedSchemas.Add(databaseName.Schema);
			//	}
			//}

			var builder = new StringBuilder();
			builder.Append("CREATE TABLE ");
			builder.Append(Name(operation.Name));
			builder.Append(" (");
			builder.AppendLine();
			builder.Append(JoinColumns(operation.Columns.Select(Generate), true));
			builder.Append(")");
			yield return Statement(builder.ToString());

			if (operation.PrimaryKey != null)
			{
				foreach (var item in Generate(operation.PrimaryKey))
					yield return item;
			}
		}

		protected virtual IEnumerable<MigrationStatement> Generate(DropColumnOperation operation)
		{
			var builder = new StringBuilder();
			builder.Append("ALTER TABLE ");
			builder.Append(Name(operation.Table));
			builder.Append(" DROP COLUMN ");
			builder.Append(Quote(operation.Name));
			yield return Statement(builder.ToString());
		}

		protected virtual IEnumerable<MigrationStatement> Generate(DropForeignKeyOperation operation)
		{
			var builder = new StringBuilder();
			builder.Append("ALTER TABLE ");
			builder.Append(Name(operation.DependentTable));
			builder.Append(" DROP CONSTRAINT ");
			builder.Append(Quote(operation.Name));
			yield return Statement(builder.ToString());
		}

		protected virtual IEnumerable<MigrationStatement> Generate(DropIndexOperation operation)
		{
			var builder = new StringBuilder();
			builder.Append("DROP INDEX ");
			builder.Append(Quote(operation.Name));
			builder.Append(" ON ");
			builder.Append(Name(operation.Table));
			yield return Statement(builder.ToString());
		}

		protected virtual IEnumerable<MigrationStatement> Generate(DropPrimaryKeyOperation operation)
		{
#warning Does not work in NuoDB, yet
			var writer = new StringBuilder();
			writer.Append("ALTER TABLE ");
			writer.Append(Name(operation.Table));
			writer.Append(" DROP PRIMARY KEY");
			yield return Statement(writer.ToString());
		}

		protected virtual IEnumerable<MigrationStatement> Generate(DropProcedureOperation operation)
		{
			throw new NotImplementedException();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(DropTableOperation operation)
		{
			var builder = new StringBuilder();
			builder.Append("DROP TABLE ");
			builder.Append(Name(operation.Name));
			yield return Statement(builder.ToString());
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
			throw new NotImplementedException();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(RenameColumnOperation operation)
		{
			var builder = new StringBuilder();
			builder.Append("ALTER TABLE ");
			builder.Append(Quote(operation.Table));
			builder.Append(" RENAME COLUMN ");
			builder.Append(Quote(operation.Name));
			builder.Append(" TO ");
			builder.Append(Quote(operation.NewName));
			yield return Statement(builder.ToString());
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
			var builder = new StringBuilder();
			builder.Append(Quote(column.Name));
			builder.Append(" ");
			builder.Append(BuildColumnType(column));

			if ((column.IsNullable != null)
				&& !column.IsNullable.Value)
			{
				builder.Append(" NOT NULL");
			}

			if (column.DefaultValue != null)
			{
				builder.Append(" DEFAULT ");
				builder.Append(Generate((dynamic)column.DefaultValue));
			}
			else if (!string.IsNullOrWhiteSpace(column.DefaultValueSql))
			{
				builder.Append(" DEFAULT ");
				builder.Append(column.DefaultValueSql);
			}
			else if (column.IsIdentity)
			{
				if (column.Type == PrimitiveTypeKind.Guid && column.DefaultValue == null)
				{
					builder.Append(" DEFAULT " + Guid.NewGuid().ToNuoDbString());
				}
				else
				{
					builder.Append(" GENERATED BY DEFAULT AS IDENTITY");
				}
			}

			return builder.ToString();
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
#warning Not done!
			return defaultValue ? "1" : "0";
		}

		protected virtual string Generate(DateTimeOffset defaultValue)
		{
#warning Not done!
			return "'" + defaultValue.ToString(""/*DateTimeOffsetFormat*/, CultureInfo.InvariantCulture) + "'";
		}

		protected virtual string Generate(Guid defaultValue)
		{
			return SqlGenerator.FormatGuid(defaultValue);
		}

		protected virtual string Generate(string defaultValue)
		{
#warning Not done!
			return "'" + defaultValue + "'";
		}

		protected virtual string Generate(TimeSpan defaultValue)
		{
#warning Not done!
			return "'" + defaultValue + "'";
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

		string BuildColumnType(ColumnModel columnModel)
		{
			if (columnModel.IsTimestamp)
			{
#warning Does this exists in NuoDB?
				return "rowversion";
			}

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

		static string JoinColumns(IEnumerable<string> columns, bool separateLines = false)
		{
			var separator = ", " + (separateLines ? Environment.NewLine : string.Empty);
			return string.Join(separator, columns);
		}

		#endregion
	}
}
