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
			return GenerateStatements(migrationOperations).ToArray();
		}

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
			return Enumerable.Empty<MigrationStatement>();
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

		protected MigrationStatement Statement(string sql, bool suppressTransaction)
		{
			return new MigrationStatement
			{
				Sql = sql,
				SuppressTransaction = suppressTransaction,
				BatchTerminator = string.Empty
			};
		}

		IEnumerable<MigrationStatement> GenerateStatements(IEnumerable<MigrationOperation> operations)
		{
			return operations.Select<dynamic, IEnumerable<MigrationStatement>>(x => Generate(x)).SelectMany(x => x);
		}
	}
}
