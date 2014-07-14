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
			return new[] { Statement(operation.Sql, operation.SuppressTransaction) };
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
					builder.Append(Generate(DateTime.Parse("1900-01-01 00:00:00", CultureInfo.InvariantCulture)));
				}
				else
				{
					builder.Append(Generate((dynamic)column.ClrDefaultValue));
				}
			}

			return new[] { Statement(builder.ToString()) };
		}

		protected virtual IEnumerable<MigrationStatement> Generate(AddForeignKeyOperation operation)
		{
			return Enumerable.Empty<MigrationStatement>();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(AddPrimaryKeyOperation operation)
		{
			return Enumerable.Empty<MigrationStatement>();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(AlterColumnOperation operation)
		{
			return Enumerable.Empty<MigrationStatement>();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(AlterProcedureOperation operation)
		{
			return Enumerable.Empty<MigrationStatement>();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(AlterTableOperation operation)
		{
			return Enumerable.Empty<MigrationStatement>();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(CreateIndexOperation operation)
		{
			return Enumerable.Empty<MigrationStatement>();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(CreateProcedureOperation operation)
		{
			return Enumerable.Empty<MigrationStatement>();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(CreateTableOperation operation)
		{
			return Enumerable.Empty<MigrationStatement>();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(DropColumnOperation operation)
		{
			return Enumerable.Empty<MigrationStatement>();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(DropForeignKeyOperation operation)
		{
			return Enumerable.Empty<MigrationStatement>();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(DropIndexOperation operation)
		{
			return Enumerable.Empty<MigrationStatement>();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(DropPrimaryKeyOperation operation)
		{
			return Enumerable.Empty<MigrationStatement>();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(DropProcedureOperation operation)
		{
			return Enumerable.Empty<MigrationStatement>();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(DropTableOperation operation)
		{
			return Enumerable.Empty<MigrationStatement>();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(HistoryOperation operation)
		{
			return Enumerable.Empty<MigrationStatement>();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(MoveProcedureOperation operation)
		{
			return Enumerable.Empty<MigrationStatement>();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(MoveTableOperation operation)
		{
			return Enumerable.Empty<MigrationStatement>();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(NotSupportedOperation operation)
		{
			return Enumerable.Empty<MigrationStatement>();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(RenameColumnOperation operation)
		{
			return Enumerable.Empty<MigrationStatement>();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(RenameIndexOperation operation)
		{
			return Enumerable.Empty<MigrationStatement>();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(RenameProcedureOperation operation)
		{
			return Enumerable.Empty<MigrationStatement>();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(RenameTableOperation operation)
		{
			return Enumerable.Empty<MigrationStatement>();
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
#warning Not finished.
			var originalStoreTypeName = propertyModel.StoreType;
			var typeUsage = ProviderManifest.GetStoreType(propertyModel.TypeUsage);

			if (string.IsNullOrWhiteSpace(originalStoreTypeName))
			{
				originalStoreTypeName = typeUsage.EdmType.Name;
			}
			else
			{
				var storeTypeUsage = BuildStoreTypeUsage(originalStoreTypeName, propertyModel);

				typeUsage = storeTypeUsage ?? typeUsage;
			}

			var storeTypeName = originalStoreTypeName;

			const string MaxSuffix = "(max)";

			if (storeTypeName.EndsWith(MaxSuffix, StringComparison.Ordinal))
			{
				storeTypeName = Quote(storeTypeName.Substring(0, storeTypeName.Length - MaxSuffix.Length)) + MaxSuffix;
			}
			else
			{
				storeTypeName = Quote(storeTypeName);
			}

			switch (originalStoreTypeName)
			{
				case "decimal":
				case "numeric":
					storeTypeName += "(" +
									 (propertyModel.Precision ?? (byte)typeUsage.Facets[DbProviderManifest.PrecisionFacetName].Value)
									 + ", " + (propertyModel.Scale ?? (byte)typeUsage.Facets[DbProviderManifest.ScaleFacetName].Value) + ")";
					break;
				case "datetime2":
				case "datetimeoffset":
				case "time":
					storeTypeName += "(" + (propertyModel.Precision ?? (byte)typeUsage.Facets[DbProviderManifest.PrecisionFacetName].Value) + ")";
					break;
				case "binary":
				case "varbinary":
				case "nvarchar":
				case "varchar":
				case "char":
				case "nchar":
					storeTypeName += "(" + (propertyModel.MaxLength ?? (int)typeUsage.Facets[DbProviderManifest.MaxLengthFacetName].Value) + ")";
					break;
			}

			return storeTypeName;
		}

		IEnumerable<MigrationStatement> GenerateStatements(IEnumerable<MigrationOperation> operations)
		{
			return operations.Select<dynamic, IEnumerable<MigrationStatement>>(x => Generate(x)).SelectMany(x => x);
		}

		static DbConnection CreateConnection()
		{
			return DbConfiguration.DependencyResolver.GetService<DbProviderFactory>(NuoDbProviderServices.ProviderInvariantName).CreateConnection();
		}

		#endregion


	}
}
