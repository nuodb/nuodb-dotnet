// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query;
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

    }
}