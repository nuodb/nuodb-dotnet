using Microsoft.EntityFrameworkCore.Update;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using NuoDb.EntityFrameworkCore.NuoDb.Internal;
using NuoDb.EntityFrameworkCore.NuoDb.Storage.Internal;
using NuoDb.EntityFrameworkCore.NuoDb.Update.Internal;
using NuoDb.EntityFrameworkCore.Tests.TestUtilities;

namespace NuoDb.EntityFrameworkCore.Tests.Update
{
    public class NuoDbUpdateSqlGeneratorTest: UpdateSqlGeneratorTestBase
    {
        protected override TestHelpers TestHelpers
            => NuoDbTestHelpers.Instance;
        
        protected override void AppendDeleteOperation_creates_full_delete_command_text_verification(StringBuilder stringBuilder)
        {
            var sql = stringBuilder.ToString();
            AssertBaseline(
                """
DELETE FROM "dbo"."Ducks"
WHERE "Id" = @p0;
SELECT GETUPDATECOUNT() FROM DUAL;
""", sql);
        }

        protected override void AppendDeleteOperation_creates_full_delete_command_text_with_concurrency_check_verification(
            StringBuilder stringBuilder)
        {
            var sql = stringBuilder.ToString();
            AssertBaseline(
                """
DELETE FROM "dbo"."Ducks"
WHERE "Id" = @p0 AND "ConcurrencyToken" IS NULL;
SELECT GETUPDATECOUNT() FROM DUAL;
""", sql);
        }

        protected override void AppendInsertOperation_insert_if_store_generated_columns_exist_verification(StringBuilder stringBuilder)
        {
            var sql = stringBuilder.ToString();
            AssertBaseline(
                """
                INSERT INTO "dbo"."Ducks" ("Name", "Quacks", "ConcurrencyToken")
                VALUES (@p0, @p1, @p2);
                SELECT "Id", "Computed"
                FROM "dbo"."Ducks"
                WHERE GETUPDATECOUNT() = 1 AND "Id" = SCOPE_IDENTITY();
                """,sql);

        }

        public override void AppendInsertOperation_appends_insert_and_select_rowcount_if_no_store_generated_columns_exist_or_conditions_exist()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(false, false);

            CreateSqlGenerator().AppendInsertOperation(stringBuilder, command, 0);


            AssertBaseline(
                """
                INSERT INTO "dbo"."Ducks" ("Id", "Name", "Quacks", "ConcurrencyToken")
                VALUES (@p0, @p1, @p2, @p3);
                SELECT GETUPDATECOUNT() FROM DUAL;
                """, stringBuilder.ToString());
        }

        public override void AppendInsertOperation_insert_if_store_generated_columns_exist()
        {
            base.AppendInsertOperation_insert_if_store_generated_columns_exist();
        }


        protected override void AppendInsertOperation_for_store_generated_columns_but_no_identity_verification(StringBuilder stringBuilder)
        {
            var sql = stringBuilder.ToString();
            AssertBaseline(
                """
                INSERT INTO "dbo"."Ducks" ("Id", "Name", "Quacks", "ConcurrencyToken")
                VALUES (@p0, @p1, @p2, @p3);
                SELECT "Computed"
                FROM "dbo"."Ducks"
                WHERE GETUPDATECOUNT() = 1 AND "Id" = @p0;
                """, sql);
        }

        protected override void AppendInsertOperation_for_only_identity_verification(StringBuilder stringBuilder)
        {
            var sql = stringBuilder.ToString();
            AssertBaseline(
                """
                INSERT INTO "dbo"."Ducks" ("Name", "Quacks", "ConcurrencyToken")
                VALUES (@p0, @p1, @p2);
                SELECT "Id"
                FROM "dbo"."Ducks"
                WHERE GETUPDATECOUNT() = 1 AND "Id" = SCOPE_IDENTITY();
                """, sql);
        }

        protected override void AppendInsertOperation_for_all_store_generated_columns_verification(StringBuilder stringBuilder)
        {
            var sql = stringBuilder.ToString();
            AssertBaseline(
                """
                INSERT INTO "dbo"."Ducks"
                DEFAULT VALUES;
                SELECT "Id", "Computed"
                FROM "dbo"."Ducks"
                WHERE GETUPDATECOUNT() = 1 AND "Id" = SCOPE_IDENTITY();
                """, sql);
        }

        protected override void AppendInsertOperation_for_only_single_identity_columns_verification(StringBuilder stringBuilder)
        {
            var sql = stringBuilder.ToString();
            AssertBaseline(
                """
                INSERT INTO "dbo"."Ducks"
                DEFAULT VALUES;
                SELECT "Id"
                FROM "dbo"."Ducks"
                WHERE GETUPDATECOUNT() = 1 AND "Id" = SCOPE_IDENTITY();
                """, sql);
        }

        protected override void AppendUpdateOperation_if_store_generated_columns_exist_verification(StringBuilder stringBuilder)
        {
            var sql = stringBuilder.ToString();
            AssertBaseline(
                """
                UPDATE "dbo"."Ducks" SET "Name" = @p0, "Quacks" = @p1, "ConcurrencyToken" = @p2
                WHERE "Id" = @p3 AND "ConcurrencyToken" IS NULL;
                SELECT "Computed"
                FROM "dbo"."Ducks"
                WHERE GETUPDATECOUNT() = 1 AND "Id" = @p3;
                """, sql);
        }

        protected override void AppendUpdateOperation_if_store_generated_columns_dont_exist_verification(StringBuilder stringBuilder)
        {
            var sql = stringBuilder.ToString();
            AssertBaseline(
                """
                UPDATE "dbo"."Ducks" SET "Name" = @p0, "Quacks" = @p1, "ConcurrencyToken" = @p2
                WHERE "Id" = @p3;
                SELECT GETUPDATECOUNT() FROM DUAL;
                """, sql);
        }

        protected override void AppendUpdateOperation_appends_where_for_concurrency_token_verification(StringBuilder stringBuilder)
        {
            var sql = stringBuilder.ToString();
            AssertBaseline(
                """
                UPDATE "dbo"."Ducks" SET "Name" = @p0, "Quacks" = @p1, "ConcurrencyToken" = @p2
                WHERE "Id" = @p3 AND "ConcurrencyToken" IS NULL;
                SELECT GETUPDATECOUNT() FROM DUAL;
                """, sql);
        }

        protected override void AppendUpdateOperation_for_computed_property_verification(StringBuilder stringBuilder)
        {
            var sql = stringBuilder.ToString();
            AssertBaseline(
                """
                UPDATE "dbo"."Ducks" SET "Name" = @p0, "Quacks" = @p1, "ConcurrencyToken" = @p2
                WHERE "Id" = @p3;
                SELECT "Computed"
                FROM "dbo"."Ducks"
                WHERE GETUPDATECOUNT() = 1 AND "Id" = @p3;
                """, sql);
        }

        public override void GenerateNextSequenceValueOperation_correctly_handles_schemas()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.GenerateNextSequenceValueOperation_correctly_handles_schemas());
            Assert.Equal(NuoDbStrings.SequencesNotSupported, ex.Message);
        }

        public override void GenerateNextSequenceValueOperation_returns_statement_with_sanitized_sequence()
        {
            var ex = Assert.Throws<NotSupportedException>(
                () => base.GenerateNextSequenceValueOperation_returns_statement_with_sanitized_sequence());
            Assert.Equal(NuoDbStrings.SequencesNotSupported, ex.Message);
        }

        protected override IUpdateSqlGenerator CreateSqlGenerator()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseNuoDb("Server=localhost;Database=demo;User=dba;Password=dba;Schema=USER");

            return new NuoDbUpdateSqlGenerator(
                new UpdateSqlGeneratorDependencies(
                    new NuoDbSqlGenerationHelper(
                        new RelationalSqlGenerationHelperDependencies()),
                    new NuoDbTypeMappingSource(
                        TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                        TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>())));
        }

        private void AssertBaseline(string expected, string actual)
            => Assert.Equal(expected, actual.TrimEnd(), ignoreLineEndingDifferences: true);

        protected override string RowsAffected => "GETUPDATECOUNT()";
    }
}
