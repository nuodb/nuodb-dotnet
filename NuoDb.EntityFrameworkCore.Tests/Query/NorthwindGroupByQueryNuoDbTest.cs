// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace NuoDb.EntityFrameworkCore.Tests.Query
{
    public class NorthwindGroupByQueryNuoDbTest : NorthwindGroupByQueryRelationalTestBase<
        NorthwindQueryNuoDbFixture<NoopModelCustomizer>>
    {
        // ReSharper disable once UnusedParameter.Local
        public NorthwindGroupByQueryNuoDbTest(NorthwindQueryNuoDbFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task AsEnumerable_in_subquery_for_GroupBy(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.AsEnumerable_in_subquery_for_GroupBy(async));
        }

        public override async Task Complex_query_with_groupBy_in_subquery1(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Complex_query_with_groupBy_in_subquery1(async));
        }

        public override async Task Complex_query_with_groupBy_in_subquery2(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Complex_query_with_groupBy_in_subquery2(async));
        }
        public override async Task Complex_query_with_groupBy_in_subquery3(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Complex_query_with_groupBy_in_subquery3(async));
        }

        public override async Task GroupBy_aggregate_from_multiple_query_in_same_projection(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.GroupBy_aggregate_from_multiple_query_in_same_projection(async));
        }

        // public override async Task GroupBy_aggregate_from_multiple_query_in_same_projection_2(bool async)
        // {
        //     await Assert.ThrowsAsync<InvalidOperationException>(()=> base.GroupBy_aggregate_from_multiple_query_in_same_projection_2(async));
        // }

        public override async Task GroupBy_aggregate_from_multiple_query_in_same_projection_3(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.GroupBy_aggregate_from_multiple_query_in_same_projection_3(async));
        }

        public override async Task Complex_query_with_group_by_in_subquery5(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Complex_query_with_group_by_in_subquery5(async));
        }

        public override async Task GroupBy_aggregate_without_selectMany_selecting_first(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.GroupBy_aggregate_without_selectMany_selecting_first(async));
        }

        public override async Task Select_nested_collection_with_groupby(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Select_nested_collection_with_groupby(async));
        }

        public override async Task Select_uncorrelated_collection_with_groupby_multiple_collections_work(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Select_uncorrelated_collection_with_groupby_multiple_collections_work(async));
        }

        public override async Task Select_uncorrelated_collection_with_groupby_when_outer_is_distinct(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Select_uncorrelated_collection_with_groupby_when_outer_is_distinct(async));
        }
        public override async Task Select_uncorrelated_collection_with_groupby_works(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Select_uncorrelated_collection_with_groupby_works(async));
        }

        public override async Task GroupBy_aggregate_from_multiple_query_in_same_projection_2(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.GroupBy_aggregate_from_multiple_query_in_same_projection_2(async));
        }
    }
}