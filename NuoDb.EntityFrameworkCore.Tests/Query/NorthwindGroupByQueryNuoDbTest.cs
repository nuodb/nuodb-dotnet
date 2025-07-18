﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Numerics;
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
            Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
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

        public override Task Complex_query_with_groupBy_in_subquery4(bool async)
             =>  Assert.ThrowsAsync<InvalidOperationException>(()=> base.Complex_query_with_groupBy_in_subquery4(async));

        [ConditionalTheory(Skip="NuoDB bug, returns unknown function type: 39")]
        [MemberData(nameof(IsAsyncData))]
        public override Task GroupBy_conditional_properties(bool async)
  
        {
            return base.GroupBy_conditional_properties(async);
        }

        [ConditionalTheory(Skip="NuoDB does not support aggregate functions in where clause")]
        [MemberData(nameof(IsAsyncData))]
        public override Task GroupBy_with_aggregate_containing_complex_where(bool async)
        {
            return base.GroupBy_with_aggregate_containing_complex_where(async);
        }

        [ConditionalTheory(Skip="NuoDB does not support apply")]
        [MemberData(nameof(IsAsyncData))]
        public override Task Select_correlated_collection_after_GroupBy_aggregate_when_identifier_changes_to_complex(bool async)
        {
            return base.Select_correlated_collection_after_GroupBy_aggregate_when_identifier_changes_to_complex(async);
        }


        public override  Task GroupBy_aggregate_from_multiple_query_in_same_projection_2(bool async)
        {
            //NuoDb doesnt support this as it expects additional group by criteria
            //await Assert.ThrowsAsync<InvalidOperationException>(()=> base.GroupBy_aggregate_from_multiple_query_in_same_projection_2(async));
            return Task.CompletedTask;
        }
    }
}