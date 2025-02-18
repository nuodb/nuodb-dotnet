// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace NuoDb.EntityFrameworkCore.Tests.Query
{
    public class NorthwindChangeTrackingQueryNuoDbTest : NorthwindChangeTrackingQueryTestBase<
        NorthwindQueryNuoDbFixture<NoopModelCustomizer>>
    {
        public NorthwindChangeTrackingQueryNuoDbTest(NorthwindQueryNuoDbFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }

    }
}