using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using NuoDb.EntityFrameworkCore.NuoDb.Metadata.Internal;
using NuoDb.EntityFrameworkCore.Tests.TestUtilities;

namespace NuoDb.EntityFrameworkCore.Tests.Migrations
{
    public class NuoDbMigrationsSqlGeneratorTest: MigrationsSqlGeneratorTestBase
    {
        public NuoDbMigrationsSqlGeneratorTest() : base(
            NuoDbTestHelpers.Instance)
        {

        }


        [ConditionalFact]
        public virtual void It_lifts_foreign_key_additions()
        {
            Generate(
                new CreateTableOperation
                {
                    Name = "Pie",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            ClrType = typeof(int),
                            Name = "FlavorId",
                            ColumnType = "INT",
                            Table = "Pie"
                        }
                    }
                },
                new AddForeignKeyOperation
                {
                    Table = "Pie",
                    PrincipalTable = "Flavor",
                    Columns = new[] { "FlavorId" },
                    PrincipalColumns = new[] { "Id" }
                });

            AssertSql(
                @"CREATE TABLE IF NOT EXISTS ""Pie"" (
    ""FlavorId"" INT NOT NULL,
    FOREIGN KEY (""FlavorId"") REFERENCES ""Flavor"" (""Id"")
);
");
        }

        
        [ConditionalFact]
        public virtual void DefaultValue_formats_literal_correctly()
        {
            Generate(
                new CreateTableOperation
                {
                    Name = "History",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Name = "Event",
                            Table = "History",
                            ClrType = typeof(string),
                            ColumnType = "TEXT",
                            DefaultValue = new DateTime(2015, 4, 12, 17, 5, 0)
                        }
                    }
                });

            AssertSql(
                @"CREATE TABLE IF NOT EXISTS ""History"" (
    ""Event"" TEXT NOT NULL DEFAULT '2015-04-12 17:05:00'
);
");
        }


        public override void DefaultValue_with_line_breaks(bool isUnicode)
        {
            base.DefaultValue_with_line_breaks(isUnicode);
            var expectedSql = @"CREATE TABLE IF NOT EXISTS ""dbo"".""TestLineBreaks"" (
    ""TestDefaultValue"" string NOT NULL DEFAULT ((CHAR(13) || (CHAR(10) || 'Various Line')) || (CHAR(13) || ('Breaks' || CHAR(10))))
);
";
            AssertSql(expectedSql);
        }

        public override void DefaultValue_with_line_breaks_2(bool isUnicode)
        {
            base.DefaultValue_with_line_breaks_2(isUnicode);

            AssertSql(
                @"CREATE TABLE IF NOT EXISTS ""dbo"".""TestLineBreaks"" (
    ""TestDefaultValue"" string NOT NULL DEFAULT ((((((((('0' || (CHAR(13) || CHAR(10))) || (('1' || CHAR(13)) || (CHAR(10) || '2'))) || ((CHAR(13) || (CHAR(10) || '3')) || ((CHAR(13) || CHAR(10)) || ('4' || CHAR(13))))) || (((CHAR(10) || ('5' || CHAR(13))) || ((CHAR(10) || '6') || (CHAR(13) || CHAR(10)))) || (('7' || (CHAR(13) || CHAR(10))) || (('8' || CHAR(13)) || (CHAR(10) || '9'))))) || ((((CHAR(13) || (CHAR(10) || '10')) || ((CHAR(13) || CHAR(10)) || ('11' || CHAR(13)))) || ((CHAR(10) || ('12' || CHAR(13))) || ((CHAR(10) || '13') || (CHAR(13) || CHAR(10))))) || ((('14' || (CHAR(13) || CHAR(10))) || (('15' || CHAR(13)) || (CHAR(10) || '16'))) || ((CHAR(13) || (CHAR(10) || '17')) || ((CHAR(13) || CHAR(10)) || ('18' || CHAR(13))))))) || (((((CHAR(10) || ('19' || CHAR(13))) || ((CHAR(10) || '20') || (CHAR(13) || CHAR(10)))) || (('21' || (CHAR(13) || CHAR(10))) || (('22' || CHAR(13)) || (CHAR(10) || '23')))) || (((CHAR(13) || (CHAR(10) || '24')) || ((CHAR(13) || CHAR(10)) || ('25' || CHAR(13)))) || ((CHAR(10) || ('26' || CHAR(13))) || ((CHAR(10) || '27') || (CHAR(13) || CHAR(10)))))) || (((('28' || (CHAR(13) || CHAR(10))) || (('29' || CHAR(13)) || (CHAR(10) || '30'))) || ((CHAR(13) || (CHAR(10) || '31')) || ((CHAR(13) || CHAR(10)) || ('32' || CHAR(13))))) || (((CHAR(10) || ('33' || CHAR(13))) || ((CHAR(10) || '34') || (CHAR(13) || CHAR(10)))) || (('35' || (CHAR(13) || CHAR(10))) || (('36' || CHAR(13)) || (CHAR(10) || '37'))))))) || ((((((CHAR(13) || (CHAR(10) || '38')) || ((CHAR(13) || CHAR(10)) || ('39' || CHAR(13)))) || ((CHAR(10) || ('40' || CHAR(13))) || ((CHAR(10) || '41') || (CHAR(13) || CHAR(10))))) || ((('42' || (CHAR(13) || CHAR(10))) || (('43' || CHAR(13)) || (CHAR(10) || '44'))) || ((CHAR(13) || (CHAR(10) || '45')) || ((CHAR(13) || CHAR(10)) || ('46' || CHAR(13)))))) || ((((CHAR(10) || ('47' || CHAR(13))) || ((CHAR(10) || '48') || (CHAR(13) || CHAR(10)))) || (('49' || (CHAR(13) || CHAR(10))) || (('50' || CHAR(13)) || (CHAR(10) || '51')))) || (((CHAR(13) || (CHAR(10) || '52')) || ((CHAR(13) || CHAR(10)) || ('53' || CHAR(13)))) || ((CHAR(10) || ('54' || CHAR(13))) || ((CHAR(10) || '55') || (CHAR(13) || CHAR(10))))))) || ((((('56' || (CHAR(13) || CHAR(10))) || (('57' || CHAR(13)) || (CHAR(10) || '58'))) || ((CHAR(13) || (CHAR(10) || '59')) || ((CHAR(13) || CHAR(10)) || ('60' || CHAR(13))))) || (((CHAR(10) || ('61' || CHAR(13))) || ((CHAR(10) || '62') || (CHAR(13) || CHAR(10)))) || (('63' || (CHAR(13) || CHAR(10))) || (('64' || CHAR(13)) || (CHAR(10) || '65'))))) || ((((CHAR(13) || (CHAR(10) || '66')) || ((CHAR(13) || CHAR(10)) || ('67' || CHAR(13)))) || ((CHAR(10) || ('68' || CHAR(13))) || ((CHAR(10) || '69') || (CHAR(13) || CHAR(10))))) || ((('70' || (CHAR(13) || CHAR(10))) || (('71' || CHAR(13)) || (CHAR(10) || '72'))) || (((CHAR(13) || CHAR(10)) || ('73' || CHAR(13))) || ((CHAR(10) || '74') || (CHAR(13) || CHAR(10))))))))) || ((((((('75' || (CHAR(13) || CHAR(10))) || (('76' || CHAR(13)) || (CHAR(10) || '77'))) || ((CHAR(13) || (CHAR(10) || '78')) || ((CHAR(13) || CHAR(10)) || ('79' || CHAR(13))))) || (((CHAR(10) || ('80' || CHAR(13))) || ((CHAR(10) || '81') || (CHAR(13) || CHAR(10)))) || (('82' || (CHAR(13) || CHAR(10))) || (('83' || CHAR(13)) || (CHAR(10) || '84'))))) || ((((CHAR(13) || (CHAR(10) || '85')) || ((CHAR(13) || CHAR(10)) || ('86' || CHAR(13)))) || ((CHAR(10) || ('87' || CHAR(13))) || ((CHAR(10) || '88') || (CHAR(13) || CHAR(10))))) || ((('89' || (CHAR(13) || CHAR(10))) || (('90' || CHAR(13)) || (CHAR(10) || '91'))) || ((CHAR(13) || (CHAR(10) || '92')) || ((CHAR(13) || CHAR(10)) || ('93' || CHAR(13))))))) || (((((CHAR(10) || ('94' || CHAR(13))) || ((CHAR(10) || '95') || (CHAR(13) || CHAR(10)))) || (('96' || (CHAR(13) || CHAR(10))) || (('97' || CHAR(13)) || (CHAR(10) || '98')))) || (((CHAR(13) || (CHAR(10) || '99')) || ((CHAR(13) || CHAR(10)) || ('100' || CHAR(13)))) || ((CHAR(10) || ('101' || CHAR(13))) || ((CHAR(10) || '102') || (CHAR(13) || CHAR(10)))))) || (((('103' || (CHAR(13) || CHAR(10))) || (('104' || CHAR(13)) || (CHAR(10) || '105'))) || ((CHAR(13) || (CHAR(10) || '106')) || ((CHAR(13) || CHAR(10)) || ('107' || CHAR(13))))) || (((CHAR(10) || ('108' || CHAR(13))) || ((CHAR(10) || '109') || (CHAR(13) || CHAR(10)))) || (('110' || (CHAR(13) || CHAR(10))) || (('111' || CHAR(13)) || (CHAR(10) || '112'))))))) || ((((((CHAR(13) || (CHAR(10) || '113')) || ((CHAR(13) || CHAR(10)) || ('114' || CHAR(13)))) || ((CHAR(10) || ('115' || CHAR(13))) || ((CHAR(10) || '116') || (CHAR(13) || CHAR(10))))) || ((('117' || (CHAR(13) || CHAR(10))) || (('118' || CHAR(13)) || (CHAR(10) || '119'))) || ((CHAR(13) || (CHAR(10) || '120')) || ((CHAR(13) || CHAR(10)) || ('121' || CHAR(13)))))) || ((((CHAR(10) || ('122' || CHAR(13))) || ((CHAR(10) || '123') || (CHAR(13) || CHAR(10)))) || (('124' || (CHAR(13) || CHAR(10))) || (('125' || CHAR(13)) || (CHAR(10) || '126')))) || (((CHAR(13) || (CHAR(10) || '127')) || ((CHAR(13) || CHAR(10)) || ('128' || CHAR(13)))) || ((CHAR(10) || ('129' || CHAR(13))) || ((CHAR(10) || '130') || (CHAR(13) || CHAR(10))))))) || ((((('131' || (CHAR(13) || CHAR(10))) || (('132' || CHAR(13)) || (CHAR(10) || '133'))) || ((CHAR(13) || (CHAR(10) || '134')) || ((CHAR(13) || CHAR(10)) || ('135' || CHAR(13))))) || (((CHAR(10) || ('136' || CHAR(13))) || ((CHAR(10) || '137') || (CHAR(13) || CHAR(10)))) || (('138' || (CHAR(13) || CHAR(10))) || (('139' || CHAR(13)) || (CHAR(10) || '140'))))) || ((((CHAR(13) || (CHAR(10) || '141')) || ((CHAR(13) || CHAR(10)) || ('142' || CHAR(13)))) || ((CHAR(10) || ('143' || CHAR(13))) || ((CHAR(10) || '144') || (CHAR(13) || CHAR(10))))) || ((('145' || (CHAR(13) || CHAR(10))) || (('146' || CHAR(13)) || (CHAR(10) || '147'))) || (((CHAR(13) || CHAR(10)) || ('148' || CHAR(13))) || ((CHAR(10) || '149') || (CHAR(13) || CHAR(10)))))))))) || (((((((('150' || (CHAR(13) || CHAR(10))) || (('151' || CHAR(13)) || (CHAR(10) || '152'))) || ((CHAR(13) || (CHAR(10) || '153')) || ((CHAR(13) || CHAR(10)) || ('154' || CHAR(13))))) || (((CHAR(10) || ('155' || CHAR(13))) || ((CHAR(10) || '156') || (CHAR(13) || CHAR(10)))) || (('157' || (CHAR(13) || CHAR(10))) || (('158' || CHAR(13)) || (CHAR(10) || '159'))))) || ((((CHAR(13) || (CHAR(10) || '160')) || ((CHAR(13) || CHAR(10)) || ('161' || CHAR(13)))) || ((CHAR(10) || ('162' || CHAR(13))) || ((CHAR(10) || '163') || (CHAR(13) || CHAR(10))))) || ((('164' || (CHAR(13) || CHAR(10))) || (('165' || CHAR(13)) || (CHAR(10) || '166'))) || ((CHAR(13) || (CHAR(10) || '167')) || ((CHAR(13) || CHAR(10)) || ('168' || CHAR(13))))))) || (((((CHAR(10) || ('169' || CHAR(13))) || ((CHAR(10) || '170') || (CHAR(13) || CHAR(10)))) || (('171' || (CHAR(13) || CHAR(10))) || (('172' || CHAR(13)) || (CHAR(10) || '173')))) || (((CHAR(13) || (CHAR(10) || '174')) || ((CHAR(13) || CHAR(10)) || ('175' || CHAR(13)))) || ((CHAR(10) || ('176' || CHAR(13))) || ((CHAR(10) || '177') || (CHAR(13) || CHAR(10)))))) || (((('178' || (CHAR(13) || CHAR(10))) || (('179' || CHAR(13)) || (CHAR(10) || '180'))) || ((CHAR(13) || (CHAR(10) || '181')) || ((CHAR(13) || CHAR(10)) || ('182' || CHAR(13))))) || (((CHAR(10) || ('183' || CHAR(13))) || ((CHAR(10) || '184') || (CHAR(13) || CHAR(10)))) || (('185' || (CHAR(13) || CHAR(10))) || (('186' || CHAR(13)) || (CHAR(10) || '187'))))))) || ((((((CHAR(13) || (CHAR(10) || '188')) || ((CHAR(13) || CHAR(10)) || ('189' || CHAR(13)))) || ((CHAR(10) || ('190' || CHAR(13))) || ((CHAR(10) || '191') || (CHAR(13) || CHAR(10))))) || ((('192' || (CHAR(13) || CHAR(10))) || (('193' || CHAR(13)) || (CHAR(10) || '194'))) || ((CHAR(13) || (CHAR(10) || '195')) || ((CHAR(13) || CHAR(10)) || ('196' || CHAR(13)))))) || ((((CHAR(10) || ('197' || CHAR(13))) || ((CHAR(10) || '198') || (CHAR(13) || CHAR(10)))) || (('199' || (CHAR(13) || CHAR(10))) || (('200' || CHAR(13)) || (CHAR(10) || '201')))) || (((CHAR(13) || (CHAR(10) || '202')) || ((CHAR(13) || CHAR(10)) || ('203' || CHAR(13)))) || ((CHAR(10) || ('204' || CHAR(13))) || ((CHAR(10) || '205') || (CHAR(13) || CHAR(10))))))) || ((((('206' || (CHAR(13) || CHAR(10))) || (('207' || CHAR(13)) || (CHAR(10) || '208'))) || ((CHAR(13) || (CHAR(10) || '209')) || ((CHAR(13) || CHAR(10)) || ('210' || CHAR(13))))) || (((CHAR(10) || ('211' || CHAR(13))) || ((CHAR(10) || '212') || (CHAR(13) || CHAR(10)))) || (('213' || (CHAR(13) || CHAR(10))) || (('214' || CHAR(13)) || (CHAR(10) || '215'))))) || ((((CHAR(13) || (CHAR(10) || '216')) || ((CHAR(13) || CHAR(10)) || ('217' || CHAR(13)))) || ((CHAR(10) || ('218' || CHAR(13))) || ((CHAR(10) || '219') || (CHAR(13) || CHAR(10))))) || ((('220' || (CHAR(13) || CHAR(10))) || (('221' || CHAR(13)) || (CHAR(10) || '222'))) || (((CHAR(13) || CHAR(10)) || ('223' || CHAR(13))) || ((CHAR(10) || '224') || (CHAR(13) || CHAR(10))))))))) || ((((((('225' || (CHAR(13) || CHAR(10))) || (('226' || CHAR(13)) || (CHAR(10) || '227'))) || ((CHAR(13) || (CHAR(10) || '228')) || ((CHAR(13) || CHAR(10)) || ('229' || CHAR(13))))) || (((CHAR(10) || ('230' || CHAR(13))) || ((CHAR(10) || '231') || (CHAR(13) || CHAR(10)))) || (('232' || (CHAR(13) || CHAR(10))) || (('233' || CHAR(13)) || (CHAR(10) || '234'))))) || ((((CHAR(13) || (CHAR(10) || '235')) || ((CHAR(13) || CHAR(10)) || ('236' || CHAR(13)))) || ((CHAR(10) || ('237' || CHAR(13))) || ((CHAR(10) || '238') || (CHAR(13) || CHAR(10))))) || ((('239' || (CHAR(13) || CHAR(10))) || (('240' || CHAR(13)) || (CHAR(10) || '241'))) || ((CHAR(13) || (CHAR(10) || '242')) || ((CHAR(13) || CHAR(10)) || ('243' || CHAR(13))))))) || (((((CHAR(10) || ('244' || CHAR(13))) || ((CHAR(10) || '245') || (CHAR(13) || CHAR(10)))) || (('246' || (CHAR(13) || CHAR(10))) || (('247' || CHAR(13)) || (CHAR(10) || '248')))) || (((CHAR(13) || (CHAR(10) || '249')) || ((CHAR(13) || CHAR(10)) || ('250' || CHAR(13)))) || ((CHAR(10) || ('251' || CHAR(13))) || ((CHAR(10) || '252') || (CHAR(13) || CHAR(10)))))) || (((('253' || (CHAR(13) || CHAR(10))) || (('254' || CHAR(13)) || (CHAR(10) || '255'))) || ((CHAR(13) || (CHAR(10) || '256')) || ((CHAR(13) || CHAR(10)) || ('257' || CHAR(13))))) || (((CHAR(10) || ('258' || CHAR(13))) || ((CHAR(10) || '259') || (CHAR(13) || CHAR(10)))) || (('260' || (CHAR(13) || CHAR(10))) || (('261' || CHAR(13)) || (CHAR(10) || '262'))))))) || ((((((CHAR(13) || (CHAR(10) || '263')) || ((CHAR(13) || CHAR(10)) || ('264' || CHAR(13)))) || ((CHAR(10) || ('265' || CHAR(13))) || ((CHAR(10) || '266') || (CHAR(13) || CHAR(10))))) || ((('267' || (CHAR(13) || CHAR(10))) || (('268' || CHAR(13)) || (CHAR(10) || '269'))) || ((CHAR(13) || (CHAR(10) || '270')) || ((CHAR(13) || CHAR(10)) || ('271' || CHAR(13)))))) || ((((CHAR(10) || ('272' || CHAR(13))) || ((CHAR(10) || '273') || (CHAR(13) || CHAR(10)))) || (('274' || (CHAR(13) || CHAR(10))) || (('275' || CHAR(13)) || (CHAR(10) || '276')))) || (((CHAR(13) || (CHAR(10) || '277')) || ((CHAR(13) || CHAR(10)) || ('278' || CHAR(13)))) || ((CHAR(10) || ('279' || CHAR(13))) || ((CHAR(10) || '280') || (CHAR(13) || CHAR(10))))))) || ((((('281' || (CHAR(13) || CHAR(10))) || (('282' || CHAR(13)) || (CHAR(10) || '283'))) || ((CHAR(13) || (CHAR(10) || '284')) || ((CHAR(13) || CHAR(10)) || ('285' || CHAR(13))))) || (((CHAR(10) || ('286' || CHAR(13))) || ((CHAR(10) || '287') || (CHAR(13) || CHAR(10)))) || (('288' || (CHAR(13) || CHAR(10))) || (('289' || CHAR(13)) || (CHAR(10) || '290'))))) || ((((CHAR(13) || (CHAR(10) || '291')) || ((CHAR(13) || CHAR(10)) || ('292' || CHAR(13)))) || ((CHAR(10) || ('293' || CHAR(13))) || ((CHAR(10) || '294') || (CHAR(13) || CHAR(10))))) || ((('295' || (CHAR(13) || CHAR(10))) || (('296' || CHAR(13)) || (CHAR(10) || '297'))) || (((CHAR(13) || CHAR(10)) || ('298' || CHAR(13))) || ((CHAR(10) || '299') || (CHAR(13) || CHAR(10)))))))))))
);
");
        }


        [ConditionalTheory]
        [InlineData(true, null)]
        [InlineData(false, "PK_Id")]
        public void CreateTableOperation_with_annotations(bool autoincrement, string pkName)
        {
            var addIdColumn = new AddColumnOperation
            {
                Name = "Id",
                Table = "People",
                ClrType = typeof(long),
                ColumnType = "INTEGER",
                IsNullable = false
            };
            if (autoincrement)
            {
                addIdColumn.AddAnnotation(NuoDbAnnotationNames.Autoincrement, true);
            }

            Generate(
                new CreateTableOperation
                {
                    Name = "People",
                    Columns =
                    {
                        addIdColumn,
                        new AddColumnOperation
                        {
                            Name = "EmployerId",
                            Table = "People",
                            ClrType = typeof(int),
                            ColumnType = "int",
                            IsNullable = true
                        },
                        new AddColumnOperation
                        {
                            Name = "SSN",
                            Table = "People",
                            ClrType = typeof(string),
                            ColumnType = "char(11)",
                            IsNullable = true
                        }
                    },
                    PrimaryKey = new AddPrimaryKeyOperation { Name = pkName, Columns = new[] { "Id" } },
                    UniqueConstraints = { new AddUniqueConstraintOperation { Columns = new[] { "SSN" } } },
                    ForeignKeys =
                    {
                        new AddForeignKeyOperation
                        {
                            Columns = new[] { "EmployerId" },
                            PrincipalTable = "Companies",
                            PrincipalColumns = new[] { "Id" }
                        }
                    }
                });

            if (autoincrement)
            {
              AssertSql(@"CREATE TABLE IF NOT EXISTS ""People"" (
    ""Id"" INTEGER NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    ""EmployerId"" int NULL,
    ""SSN"" char(11) NULL,
    PRIMARY KEY (""Id""),
    UNIQUE (""SSN""),
    FOREIGN KEY (""EmployerId"") REFERENCES ""Companies"" (""Id"")
);
");
            }
            else
            {
                AssertSql(@"CREATE TABLE IF NOT EXISTS ""People"" (
    ""Id"" INTEGER NOT NULL,
    ""EmployerId"" int NULL,
    ""SSN"" char(11) NULL,
    CONSTRAINT ""PK_Id"" PRIMARY KEY (""Id""),
    UNIQUE (""SSN""),
    FOREIGN KEY (""EmployerId"") REFERENCES ""Companies"" (""Id"")
);
");
            }

    
        }

        public override void AddColumnOperation_without_column_type()
        {
            base.AddColumnOperation_without_column_type();

            AssertSql(
                @"ALTER TABLE ""People"" ADD ""Alias"" string NOT NULL;
");
        }

        public override void AddColumnOperation_with_maxLength_overridden()
        {
            base.AddColumnOperation_with_maxLength_overridden();

            AssertSql(
                @"ALTER TABLE ""Person"" ADD ""Name"" varchar(32) NULL;
");
        }

        public override void AddColumnOperation_with_unicode_no_model()
        {
            base.AddColumnOperation_with_unicode_no_model();

            AssertSql(
                @"ALTER TABLE ""Person"" ADD ""Name"" string NULL;
");
        }
        public override void AddColumnOperation_with_fixed_length_no_model()
        {
            base.AddColumnOperation_with_fixed_length_no_model();

            AssertSql(
                @"ALTER TABLE ""Person"" ADD ""Name"" char(100) NULL;
");
        }

        public override void AddColumnOperation_with_maxLength_no_model()
        {
            base.AddColumnOperation_with_maxLength_no_model();

            AssertSql(
                @"ALTER TABLE ""Person"" ADD ""Name"" varchar(30) NULL;
");
        }

        public override void AddColumnOperation_with_precision_and_scale_overridden()
        {
            base.AddColumnOperation_with_precision_and_scale_overridden();

            AssertSql(
                @"ALTER TABLE ""Person"" ADD ""Pi"" decimal(15,10) NOT NULL;
");
        }

        public override void AddColumnOperation_with_precision_and_scale_no_model()
        {
            base.AddColumnOperation_with_precision_and_scale_no_model();

            AssertSql(
                @"ALTER TABLE ""Person"" ADD ""Pi"" decimal(20,7) NOT NULL;
");
        }

        public override void AddColumnOperation_with_unicode_overridden()
        {
            base.AddColumnOperation_with_unicode_overridden();

            AssertSql(
                @"ALTER TABLE ""Person"" ADD ""Name"" string NULL;
");
        }

        public override void AlterColumnOperation_without_column_type()
        {
            base.AlterColumnOperation_without_column_type();

            AssertSql(
                @"ALTER TABLE ""People"" MODIFY COLUMN ""LuckyNumber"" integer NOT NULL;
");
        }

        public override void AddForeignKeyOperation_without_principal_columns()
        {
            base.AddForeignKeyOperation_without_principal_columns();

            AssertSql(
                @"ALTER TABLE ""People"" ADD FOREIGN KEY (""SpouseId"") REFERENCES ""People"";
");
        }

        public override void InsertDataOperation_all_args_spatial()
        {
            Assert.Throws<InvalidOperationException>( () => base.InsertDataOperation_all_args_spatial());
            //base.InsertDataOperation_all_args_spatial();
        }


        public override void DeleteDataOperation_all_args()
        {
            base.DeleteDataOperation_all_args();
        }

        public override void DeleteDataOperation_all_args_composite()
        {
            base.DeleteDataOperation_all_args_composite();
        }

        public override void DeleteDataOperation_required_args()
        {
            base.DeleteDataOperation_required_args();
        }

        public override void DeleteDataOperation_required_args_composite()
        {
            base.DeleteDataOperation_required_args_composite();
        }

        public override void InsertDataOperation_required_args()
        {
            base.InsertDataOperation_required_args();
        }

        public override void InsertDataOperation_required_args_composite()
        {
            base.InsertDataOperation_required_args_composite();
        }

        public override void InsertDataOperation_required_args_multiple_rows()
        {
            base.InsertDataOperation_required_args_multiple_rows();
        }

        public override void InsertDataOperation_throws_for_unsupported_column_types()
        {
            base.InsertDataOperation_throws_for_unsupported_column_types();
        }

        public override void RenameTableOperation()
        {
            base.RenameTableOperation();
        }

        public override void RenameTableOperation_legacy()
        {
            base.RenameTableOperation_legacy();
        }

        public override void UpdateDataOperation_all_args()
        {
            base.UpdateDataOperation_all_args();
        }

        public override void UpdateDataOperation_all_args_composite()
        {
            base.UpdateDataOperation_all_args_composite();
        }

        public override void UpdateDataOperation_all_args_composite_multi()
        {
            base.UpdateDataOperation_all_args_composite_multi();
        }

        public override void UpdateDataOperation_all_args_multi()
        {
            base.UpdateDataOperation_all_args_multi();
        }

        public override void UpdateDataOperation_required_args()
        {
            base.UpdateDataOperation_required_args();
        }

        public override void UpdateDataOperation_required_args_composite()
        {
            base.UpdateDataOperation_required_args_composite();
        }

        public override void UpdateDataOperation_required_args_composite_multi()
        {
            base.UpdateDataOperation_required_args_composite_multi();
        }

        public override void UpdateDataOperation_required_args_multi()
        {
            base.UpdateDataOperation_required_args_multi();
        }

        public override void UpdateDataOperation_required_args_multiple_rows()
        {
            base.UpdateDataOperation_required_args_multiple_rows();
        }

        public override void SqlOperation()
        {
            base.SqlOperation();
        }

        protected override string GetGeometryCollectionStoreType()
        {
            throw new InvalidOperationException("NuoDb does not currently support spatial types!");
        }
    }
}
