﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using NuoDb.Data.Client;
using Xunit.Abstractions;

namespace NuoDb.EntityFrameworkCore.Tests.Query
{
    public class NorthwindFunctionsQueryNuoDbTest : NorthwindFunctionsQueryRelationalTestBase<
        NorthwindQueryNuoDbFixture<NoopModelCustomizer>>
    {
        public NorthwindFunctionsQueryNuoDbTest(
            NorthwindQueryNuoDbFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task Trim_with_char_array_argument_in_predicate(bool async)
        {
            //Nuodb Doesnt support trim with a character array, just a string
            //return base.Trim_with_char_array_argument_in_predicate(async);
        }
        public override Task Where_guid_newguid(bool async)
            => AssertTranslationFailed(() => base.Where_guid_newguid(async));

        public override Task Where_math_log(bool async)
            => AssertTranslationFailed(() => base.Where_math_log(async));

        public override Task Where_math_log_new_base(bool async)
            => AssertTranslationFailed(() => base.Where_math_log_new_base(async));

        public override Task Where_math_log10(bool async)
            => AssertTranslationFailed(() => base.Where_math_log10(async));

        public override Task Where_math_min(bool async)
            => AssertTranslationFailed(() => base.Where_math_min(async));

        public override Task Where_math_max(bool async)
            => AssertTranslationFailed(() => base.Where_math_max(async));

        public override Task Where_math_exp(bool async)
            => AssertTranslationFailed(() => base.Where_math_exp(async));

        public override Task Where_math_sign(bool async)
            => AssertTranslationFailed(() => base.Where_math_sign(async));

        public override Task Where_mathf_exp(bool async)
            => AssertTranslationFailed(() => base.Where_mathf_exp(async));
        
        public override Task Where_mathf_log(bool async)
            => AssertTranslationFailed(() => base.Where_mathf_log(async));

        public override Task Where_mathf_log_new_base(bool async)
            => AssertTranslationFailed(() => base.Where_mathf_log_new_base(async));

        public override Task Where_mathf_log10(bool async)
            => AssertTranslationFailed(() => base.Where_mathf_log10(async));

        public override Task Where_mathf_sign(bool async)
            => AssertTranslationFailed(() => base.Where_mathf_sign(async));

        public override Task Projecting_Math_Truncate_and_ordering_by_it_twice(bool async)
            => AssertTranslationFailed(() => base.Projecting_Math_Truncate_and_ordering_by_it_twice(async));

        public override Task Projecting_Math_Truncate_and_ordering_by_it_twice2(bool async)
            => AssertTranslationFailed(() => base.Projecting_Math_Truncate_and_ordering_by_it_twice2(async));

        public override Task Projecting_Math_Truncate_and_ordering_by_it_twice3(bool async)
            => AssertTranslationFailed(() => base.Projecting_Math_Truncate_and_ordering_by_it_twice3(async));

        public override Task Where_math_truncate(bool async)
            => AssertTranslationFailed(() => base.Where_math_truncate(async));

        public override Task Where_mathf_truncate(bool async)
            => AssertTranslationFailed(() => base.Where_mathf_truncate(async));
        public override Task Indexof_with_constant_starting_position(bool async)
            => AssertTranslationFailed(() => base.Indexof_with_constant_starting_position(async));
        public override Task Indexof_with_parameter_starting_position(bool async)
            => AssertTranslationFailed(() => base.Indexof_with_parameter_starting_position(async));

        public override Task Sum_over_truncate_works_correctly_in_projection(bool async)
            => AssertTranslationFailed(() => base.Sum_over_truncate_works_correctly_in_projection(async));

        public override Task Sum_over_truncate_works_correctly_in_projection_2(bool async)
            => AssertTranslationFailed(() => base.Sum_over_truncate_works_correctly_in_projection_2(async));

        public override Task Where_DateOnly_FromDateTime(bool async)
            => AssertTranslationFailed(() => base.Where_DateOnly_FromDateTime(async));

        public override Task Where_math_degrees(bool async)
            => AssertTranslationFailed(() => base.Where_math_degrees(async));

        public override Task Where_mathf_degrees(bool async)
            => AssertTranslationFailed(() => base.Where_mathf_degrees(async));

        public override Task Where_mathf_radians(bool async)
            => AssertTranslationFailed(() => base.Where_mathf_radians(async));

        public override Task Where_math_radians(bool async)
            => AssertTranslationFailed(() => base.Where_math_radians(async));




        public override Task Datetime_subtraction_TotalDays(bool async)
        {
            return AssertTranslationFailed(() => base.Datetime_subtraction_TotalDays(async));
        }

        [ConditionalFact]
        public async Task DateWhereCriteria()
        {
            var sql =
                "SELECT count(*) \r\nFROM \"Orders\" AS \"o\"\r\nWHERE \"o\".\"OrderDate\" = @indate";
            var dateParam = new NuoDbParameter();
            dateParam.DbType = DbType.DateTime;
            dateParam.ParameterName = "@indate";

            var date = new DateTime(1998, 5, 4);
            dateParam.Value = date;
            using (var ctx = this.CreateContext())
            {
                var cmd = ctx.Database.GetDbConnection().CreateCommand();
                cmd.CommandText = sql;
                cmd.Parameters.Add(dateParam);
                var res = (long)Convert.ChangeType(cmd.ExecuteScalar(), typeof(long));
                Assert.Equal(3, res);
            }
        }
    }
}