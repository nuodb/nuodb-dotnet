// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace NuoDb.EntityFrameworkCore.Tests.Query
{
    public class NorthwindAsNoTrackingQueryNuoDbTest : NorthwindAsNoTrackingQueryTestBase<
        NorthwindQueryNuoDbFixture<NoopModelCustomizer>>
    {
        public NorthwindAsNoTrackingQueryNuoDbTest(
            NorthwindQueryNuoDbFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override void Applied_to_multiple_body_clauses()
        {
            // Causes Cross Join which is not supported in nuodb
            Assert.Throws<InvalidOperationException>(() => base.Applied_to_multiple_body_clauses());
        }

        public override void SelectMany_simple()
        {
            // Causes Cross Join which is not supported in nuodb
            Assert.Throws<InvalidOperationException>(() => base.SelectMany_simple());
        }
    }
}