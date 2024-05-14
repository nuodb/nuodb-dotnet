// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query;
using Xunit.Abstractions;

namespace NuoDb.EntityFrameworkCore.Tests.Query
{
    public class NorthwindQueryFiltersQueryNuoDbTest : NorthwindQueryFiltersQueryTestBase<
        NorthwindQueryNuoDbFixture<NorthwindQueryFiltersCustomizer>>
    {
        public NorthwindQueryFiltersQueryNuoDbTest(
            NorthwindQueryNuoDbFixture<NorthwindQueryFiltersCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
            //fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }
    }
}