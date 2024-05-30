// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace NuoDb.EntityFrameworkCore.Tests.Query
{
    public class NorthwindNavigationsQueryNuoDbTest : NorthwindNavigationsQueryRelationalTestBase<
        NorthwindQueryNuoDbFixture<NoopModelCustomizer>>
    {
        public NorthwindNavigationsQueryNuoDbTest(NorthwindQueryNuoDbFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }

        public override async Task Select_Where_Navigation_Equals_Navigation(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Select_Where_Navigation_Equals_Navigation(async));
        }

        public override async Task Select_Where_Navigation_Scalar_Equals_Navigation_Scalar(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar(async));
        }

        public override async Task Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected(async));
        }

        public override async Task Collection_orderby_nav_prop_count(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Collection_orderby_nav_prop_count(async));
        }
    }
}