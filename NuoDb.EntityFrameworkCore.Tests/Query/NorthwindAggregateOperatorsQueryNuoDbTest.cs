// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using NuoDb.EntityFrameworkCore.NuoDb.Internal;
using Xunit.Abstractions;

namespace NuoDb.EntityFrameworkCore.Tests.Query
{
    public class NorthwindAggregateOperatorsQueryNuoDbTest : NorthwindAggregateOperatorsQueryRelationalTestBase<
        NorthwindQueryNuoDbFixture<NoopModelCustomizer>>
    {
        public NorthwindAggregateOperatorsQueryNuoDbTest(
            NorthwindQueryNuoDbFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            ClearLog();
            Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task Average_on_nav_subquery_in_projection(bool isAsync)
        {
            using (var ctx = this.CreateContext())
            {
                var results = ctx.Set<Customer>()
                    .OrderBy(c => c.CustomerID)
                    .Select(c => new { Ave = (decimal?)c.Orders.Average(o => o.OrderID) }).ToList();
            }
            await base.Average_on_nav_subquery_in_projection(isAsync);
        }
        
        [ConditionalTheory(Skip = "Contains over subquery is not supported)")]
#pragma warning disable xUnit1003
        public override Task Contains_over_entityType_with_null_should_rewrite_to_identity_equality_subquery_complex(bool async)
#pragma warning restore xUnit1003
        {
            return base.Contains_over_entityType_with_null_should_rewrite_to_identity_equality_subquery_complex(async);
        }

        public override async Task Multiple_collection_navigation_with_FirstOrDefault_chained(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                base.Multiple_collection_navigation_with_FirstOrDefault_chained(async));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Average_over_max_subquery_is_client_eval(bool async)
            => await AssertAverage(
                async,
                ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(3),
                selector: c => (decimal)c.Orders.Average(o => 5 + o.OrderDetails.Max(od => od.ProductID)),
                asserter: (arg1, arg2) => Assert.True(Math.Abs(arg1 - arg2) < 0.01m));

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Average_over_nested_subquery_is_client_eval(bool async)
            => await AssertAverage(
                async,
                ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(3),
                selector: c => (decimal)c.Orders.Average(o => 5 + o.OrderDetails.Average(od => od.ProductID)),
                asserter: (arg1, arg2) => Assert.True(Math.Abs(arg1 - arg2) < 0.01m));

        [ConditionalTheory] // #32374
        [MemberData(nameof(IsAsyncData))]
        public override async Task Contains_inside_Average_without_GroupBy(bool async)
        {
            var cities = new[] { "London", "Berlin" };

            await AssertAverage(
                async,
                ss => ss.Set<Customer>(),
                selector: c => cities.Contains(c.City) ? 1.0 : 0.0,
                asserter: (d, d1) =>
                {
                    Assert.True(Math.Abs(d - d1) < 0.0000001);
                });
        }

        [ConditionalTheory(Skip="NuoDb does not support local enumerable inline")]
        [MemberData(nameof(IsAsyncData))]
        public override Task Contains_with_local_enumerable_inline(bool async)
        {
            return base.Contains_with_local_enumerable_inline(async);
        }

        [ConditionalTheory(Skip="NuoDb does not support local enumerable inline")]
        [MemberData(nameof(IsAsyncData))]
        public override Task Contains_with_local_enumerable_inline_closure_mix(bool async)
        {
            return base.Contains_with_local_enumerable_inline_closure_mix(async);
        }


        public override async Task Contains_with_local_tuple_array_closure(bool async)
            => await AssertTranslationFailed(() => base.Contains_with_local_tuple_array_closure(async));

        public override async Task Contains_with_local_anonymous_type_array_closure(bool async)
            // Aggregates. Issue #15937.
            => await AssertTranslationFailed(() => base.Contains_with_local_anonymous_type_array_closure(async));
    }
}