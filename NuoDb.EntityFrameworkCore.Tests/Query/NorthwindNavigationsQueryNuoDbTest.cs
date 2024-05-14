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
    }
}