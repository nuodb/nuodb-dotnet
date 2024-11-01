// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace NuoDb.EntityFrameworkCore.Tests.Query
{
    public class NorthwindSelectQueryNuoDbTest : NorthwindSelectQueryRelationalTestBase<
        NorthwindQueryNuoDbFixture<NoopModelCustomizer>>
    {
        public NorthwindSelectQueryNuoDbTest(NorthwindQueryNuoDbFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task Correlated_collection_after_distinct_not_containing_original_identifier(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Correlated_collection_after_distinct_not_containing_original_identifier(async));
        }

        public override async Task Correlated_collection_after_distinct_with_complex_projection_containing_original_identifier(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Correlated_collection_after_distinct_with_complex_projection_containing_original_identifier(async));
        }

        public override async Task Collection_projection_selecting_outer_element_followed_by_take(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Collection_projection_selecting_outer_element_followed_by_take(async));
        }

        public override async Task Correlated_collection_after_groupby_with_complex_projection_containing_original_identifier(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Correlated_collection_after_groupby_with_complex_projection_containing_original_identifier(async));
        }

        public override async Task List_from_result_of_single_result_3(bool async)
        {
            //await base.List_from_result_of_single_result_3(async);
        }

        public override async Task Collection_include_over_result_of_single_non_scalar(bool async)
        {
            await base.Collection_include_over_result_of_single_non_scalar(async);
        }

        public override async Task Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault_2(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault_2( async));
        }

        public override async Task Projecting_after_navigation_and_distinct(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Projecting_after_navigation_and_distinct( async));
        }

        public override async Task Projection_when_arithmetic_mixed(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Projection_when_arithmetic_mixed( async));
        }

        public override async Task Select_nested_collection_deep(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Select_nested_collection_deep( async));
        }

        public override async Task Select_nested_collection_deep_distinct_no_identifiers(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Select_nested_collection_deep_distinct_no_identifiers( async));
        }

        public override async Task SelectMany_correlated_with_outer_1(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.SelectMany_correlated_with_outer_1( async));
        }

        public override async Task SelectMany_correlated_with_outer_2(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.SelectMany_correlated_with_outer_2( async));
        }

        public override async Task SelectMany_correlated_with_outer_3(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.SelectMany_correlated_with_outer_3( async));
        }

        public override async Task SelectMany_correlated_with_outer_4(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.SelectMany_correlated_with_outer_4( async));
        }

        public override async Task SelectMany_correlated_with_outer_5(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.SelectMany_correlated_with_outer_5( async));
        }

        public override async Task SelectMany_correlated_with_outer_6(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.SelectMany_correlated_with_outer_6( async));
        }

        public override async Task SelectMany_correlated_with_outer_7(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.SelectMany_correlated_with_outer_7( async));
        }

        public override async Task SelectMany_whose_selector_references_outer_source(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.SelectMany_whose_selector_references_outer_source( async));
        }
        public override async Task SelectMany_with_collection_being_correlated_subquery_which_references_inner_and_outer_entity(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.SelectMany_with_collection_being_correlated_subquery_which_references_inner_and_outer_entity( async));
        }

        public override async Task Take_on_correlated_collection_in_first(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Take_on_correlated_collection_in_first( async));
        }

        public override async Task Take_on_top_level_and_on_collection_projection_with_outer_apply(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Take_on_top_level_and_on_collection_projection_with_outer_apply( async));
        }
        public override Task Member_binding_after_ctor_arguments_fails_with_client_eval(bool async)
        {
            return AssertTranslationFailed(() => base.Member_binding_after_ctor_arguments_fails_with_client_eval(async));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public Task Take_limits_returned_records(bool async)
        {
             return AssertQuery(
                async,
                ss => ss.Set<Order>().Take(3),entryCount:3
                );
        }
    }

}