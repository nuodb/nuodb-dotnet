// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
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
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
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
    }
}