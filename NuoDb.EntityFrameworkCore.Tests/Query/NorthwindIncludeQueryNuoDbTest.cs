// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace NuoDb.EntityFrameworkCore.Tests.Query
{
    public class NorthwindIncludeQueryNuoDbTest : NorthwindIncludeQueryRelationalTestBase<
        NorthwindQueryNuoDbFixture<NoopModelCustomizer>>
    {
        public NorthwindIncludeQueryNuoDbTest(NorthwindQueryNuoDbFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
        }

        public override async Task Filtered_include_with_multiple_ordering(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Filtered_include_with_multiple_ordering(async));
        }

        public override async Task Include_collection_on_additional_from_clause(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Include_collection_on_additional_from_clause(async));
        }

        public override async Task Include_collection_on_additional_from_clause_with_filter(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Include_collection_on_additional_from_clause_with_filter(async));
        }

        public override async Task Include_collection_on_additional_from_clause2(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Include_collection_on_additional_from_clause2(async));
        }

        public override async Task Include_collection_order_by_collection_column(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Include_collection_order_by_collection_column(async));
        }

        public override async Task Include_collection_order_by_subquery(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Include_collection_order_by_subquery(async));
        }

        public override async Task Include_collection_SelectMany_GroupBy_Select(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Include_collection_SelectMany_GroupBy_Select(async));
        }

        public override async Task Include_collection_with_cross_apply_with_filter(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Include_collection_with_cross_apply_with_filter(async));
        }

        public override async Task Include_collection_with_cross_join_clause_with_filter(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Include_collection_with_cross_join_clause_with_filter(async));
        }

        public override async Task Include_collection_with_outer_apply_with_filter(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Include_collection_with_outer_apply_with_filter(async));
        }

        public override async Task Include_collection_with_outer_apply_with_filter_non_equality(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Include_collection_with_outer_apply_with_filter_non_equality(async));
        }

        public override async Task Include_duplicate_collection(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Include_duplicate_collection(async));
        }

        public override async Task Include_duplicate_collection_result_operator(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Include_duplicate_collection_result_operator(async));
        }

        public override async Task Include_duplicate_collection_result_operator2(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Include_duplicate_collection_result_operator2(async));
        }

        public override async Task Include_duplicate_reference(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Include_duplicate_reference(async));
        }

        public override async Task Include_duplicate_reference2(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Include_duplicate_reference2(async));
        }

        public override async Task Include_duplicate_reference3(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Include_duplicate_reference3(async));
        }

        public override async Task Include_reference_SelectMany_GroupBy_Select(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Include_reference_SelectMany_GroupBy_Select(async));
        }

        public override async Task SelectMany_Include_collection_GroupBy_Select(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.SelectMany_Include_collection_GroupBy_Select(async));
        }

        public override async Task SelectMany_Include_reference_GroupBy_Select(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.SelectMany_Include_reference_GroupBy_Select(async));
        }
        public override async Task Then_include_collection_order_by_collection_column(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Then_include_collection_order_by_collection_column(async));
        }

    }
}