// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace NuoDb.EntityFrameworkCore.Tests.Query
{
    public class NorthwindSetOperationsQueryNuoDbTest : NorthwindSetOperationsQueryRelationalTestBase<
        NorthwindQueryNuoDbFixture<NoopModelCustomizer>>
    {
        public NorthwindSetOperationsQueryNuoDbTest(
            NorthwindQueryNuoDbFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task Except(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=>base.Except(async)) ;
        }
        public override async Task Except_non_entity(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=>base.Except_non_entity(async)) ;
        }

        public override async Task Except_simple_followed_by_projecting_constant(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=>base.Except_simple_followed_by_projecting_constant(async)) ;
        }

        public override async Task Intersect(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=>base.Intersect(async)) ;
        }

        public override async Task Intersect_nested(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=>base.Intersect_nested(async)) ;
        }

        public override async Task Intersect_non_entity(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=>base.Intersect_non_entity(async)) ;
        }

        public override async Task Select_Except_reference_projection(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=>base.Select_Except_reference_projection(async)) ;
        }

        public override async Task Except_nested(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=>base.Except_nested(async)) ;
        }

        public override async Task Union_Intersect(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=>base.Union_Intersect(async)) ;
        }

        public override async Task Union_Select_scalar(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=>base.Union_Select_scalar(async)) ;
        }

    }
}
