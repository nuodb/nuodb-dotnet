using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;
using NuoDb.Data.Client;
using NuoDb.EntityFrameworkCore.NuoDb.Storage.Internal;

namespace NuoDb.EntityFrameworkCore.Tests.Storage
{
    public class NuoDbTypeMappingTest: RelationalTypeMappingTest
    {
        protected override DbCommand CreateTestCommand()
            => new NuoDbCommand();

        [ConditionalTheory]
        [InlineData(typeof(NuoDbDateTimeOffsetTypeMapping), typeof(DateTimeOffset))]
        [InlineData(typeof(NuoDbDateTimeTypeMapping), typeof(DateTime))]
        [InlineData(typeof(NuoDbDecimalTypeMapping), typeof(decimal))]
        [InlineData(typeof(NuoDbGuidTypeMapping), typeof(Guid))]
        public override void Create_and_clone_with_converter(Type mappingType, Type type)
        {
            base.Create_and_clone_with_converter(mappingType, type);
        }

        [ConditionalTheory]
        [InlineData("string", typeof(string))]
        [InlineData("Integer", typeof(int))]
        [InlineData("Blob", typeof(byte[]))]
        [InlineData("numeric", typeof(decimal))]
        [InlineData("double", typeof(double))]
        [InlineData("SMALLINT", typeof(short))]
        [InlineData("VARCHAR(255)", typeof(string))]
        [InlineData("nchar(55)", typeof(string))]
        [InlineData("TIMESTAMP WITHOUT TIMEZONE", typeof(DateTime))]
        [InlineData("decimal(10,4)", typeof(decimal))]
        [InlineData("boolean", typeof(bool))]
        public void It_maps_strings_to_not_null_types(string typeName, Type type)
        {
            Assert.Equal(type, CreateTypeMapper().FindMapping(typeName)?.ClrType);
        }

        private static IRelationalTypeMappingSource CreateTypeMapper()
            => TestServiceFactory.Instance.Create<NuoDbTypeMappingSource>();

        public static RelationalTypeMapping GetMapping(
            Type type)
            => CreateTypeMapper().FindMapping(type);

        public override void DateTimeOffset_literal_generated_correctly()
        {
            Test_GenerateSqlLiteral_helper(
                GetMapping(typeof(DateTimeOffset)),
                new DateTimeOffset(2015, 3, 12, 13, 36, 37, 371, new TimeSpan(-7, 0, 0)),
                "'2015-03-12 13:36:37.371-07:00'");
        }

        public override void DateTime_literal_generated_correctly()
        {
            Test_GenerateSqlLiteral_helper(
                GetMapping(typeof(DateTime)),
                new DateTime(2015, 3, 12, 13, 36, 37, 371, DateTimeKind.Utc),
                "'2015-03-12 13:36:37.371'");
        }

        [ConditionalFact]
        public override void DateOnly_literal_generated_correctly()
        {
            Test_GenerateSqlLiteral_helper(
                GetMapping(typeof(DateOnly)),
                new DateOnly(2015, 3, 12),
                "'2015-03-12'");
        }

        [ConditionalFact]
        public override void TimeOnly_literal_generated_correctly()
        {
            Test_GenerateSqlLiteral_helper(
                GetMapping(typeof(TimeOnly)),
                new TimeOnly(13, 10, 15),
                "'13:10:15'");
        }

        [ConditionalFact]
        public override void TimeOnly_literal_generated_correctly_with_milliseconds()
        {
            Test_GenerateSqlLiteral_helper(
                GetMapping(typeof(TimeOnly)),
                new TimeOnly(13, 10, 15, 500),
                "'13:10:15.5000000'");
        }

        public override void Decimal_literal_generated_correctly()
        {
            var typeMapping = new NuoDbDecimalTypeMapping("DECIMAL");

            Test_GenerateSqlLiteral_helper(typeMapping, decimal.MinValue, "-79228162514264337593543950335.0");
            Test_GenerateSqlLiteral_helper(typeMapping, decimal.MaxValue, "79228162514264337593543950335.0");
        }

        public override void Guid_literal_generated_correctly()
        {
            Test_GenerateSqlLiteral_helper(
                GetMapping(typeof(Guid)),
                new Guid("c6f43a9e-91e1-45ef-a320-832ea23b7292"),
                "'C6F43A9E-91E1-45EF-A320-832EA23B7292'");
        }

        public override void ULong_literal_generated_correctly()
        {
            var typeMapping = new ULongTypeMapping("BIGINT");

            Test_GenerateSqlLiteral_helper(typeMapping, ulong.MinValue, "0");
            Test_GenerateSqlLiteral_helper(typeMapping, ulong.MaxValue, "18446744073709551615");
        }

        protected override DbContextOptions ContextOptions { get; }
            = new DbContextOptionsBuilder()
                .UseInternalServiceProvider(new ServiceCollection().AddEntityFrameworkNuoDb().BuildServiceProvider(validateScopes: true))
                .UseNuoDb("dummy string").Options;
    }
}
