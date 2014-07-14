using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Migrations.Sql;
using System.Linq;
using System.Text;

namespace NuoDb.Data.Client.EntityFramework6
{
	public class NuoDbMigrationSqlGenerator : MigrationSqlGenerator
	{
		public NuoDbMigrationSqlGenerator()
		{ }

		public override IEnumerable<MigrationStatement> Generate(IEnumerable<MigrationOperation> migrationOperations, string providerManifestToken)
		{
			return migrationOperations.Select(Generate);
		}

		protected virtual MigrationStatement Generate(AddColumnOperation operation)
		{
			return null;
		}

		protected virtual MigrationStatement Generate(AddForeignKeyOperation operation)
		{
			return null;
		}

		protected virtual MigrationStatement Generate(AddPrimaryKeyOperation operation)
		{
			return null;
		}

		protected virtual MigrationStatement Generate(AlterColumnOperation operation)
		{
			return null;
		}

		protected virtual MigrationStatement Generate(AlterProcedureOperation operation)
		{
			return null;
		}

		protected virtual MigrationStatement Generate(AlterTableOperation operation)
		{
			return null;
		}

		protected virtual MigrationStatement Generate(CreateIndexOperation operation)
		{
			return null;
		}

		protected virtual MigrationStatement Generate(CreateProcedureOperation operation)
		{
			return null;
		}

		protected virtual MigrationStatement Generate(CreateTableOperation operation)
		{
			return null;
		}

		protected virtual MigrationStatement Generate(DropColumnOperation operation)
		{
			return null;
		}

		protected virtual MigrationStatement Generate(DropForeignKeyOperation operation)
		{
			return null;
		}

		protected virtual MigrationStatement Generate(DropIndexOperation operation)
		{
			return null;
		}

		protected virtual MigrationStatement Generate(DropPrimaryKeyOperation operation)
		{
			return null;
		}

		protected virtual MigrationStatement Generate(DropProcedureOperation operation)
		{
			return null;
		}

		protected virtual MigrationStatement Generate(DropTableOperation operation)
		{
			return null;
		}

		protected virtual MigrationStatement Generate(ForeignKeyOperation operation)
		{
			return null;
		}

		protected virtual MigrationStatement Generate(HistoryOperation operation)
		{
			return null;
		}

		protected virtual MigrationStatement Generate(IndexOperation operation)
		{
			return null;
		}

		protected virtual MigrationStatement Generate(MigrationOperation operation)
		{
			return null;
		}

		protected virtual MigrationStatement Generate(MoveProcedureOperation operation)
		{
			return null;
		}

		protected virtual MigrationStatement Generate(MoveTableOperation operation)
		{
			return null;
		}

		protected virtual MigrationStatement Generate(NotSupportedOperation operation)
		{
			return null;
		}

		protected virtual MigrationStatement Generate(PrimaryKeyOperation operation)
		{
			return null;
		}

		protected virtual MigrationStatement Generate(ProcedureOperation operation)
		{
			return null;
		}

		protected virtual MigrationStatement Generate(PropertyModel operation)
		{
			return null;
		}

		protected virtual MigrationStatement Generate(RenameColumnOperation operation)
		{
			return null;
		}

		protected virtual MigrationStatement Generate(RenameIndexOperation operation)
		{
			return null;
		}

		protected virtual MigrationStatement Generate(RenameProcedureOperation operation)
		{
			return null;
		}

		protected virtual MigrationStatement Generate(RenameTableOperation operation)
		{
			return null;
		}

		protected virtual MigrationStatement Generate(SqlOperation operation)
		{
			return null;
		}

		protected virtual MigrationStatement Generate(UpdateDatabaseOperation operation)
		{
			return null;
		}

		protected MigrationStatement Statement(string sql, bool suppressTransaction = false)
		{
			return new MigrationStatement
			{
				Sql = sql,
				SuppressTransaction = suppressTransaction,
				BatchTerminator = string.Empty
			};
		}
	}
}
