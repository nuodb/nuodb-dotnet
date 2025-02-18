// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.EntityFrameworkCore.Migrations;

namespace NuoDb.EntityFrameworkCore.Tests.Migrations
{
    public class NuoDbMigrationBuilderTest
    {
        [Fact]
        public void IsNuoDb_when_using_NuoDb()
        {
            var migrationBuilder = new MigrationBuilder("NuoDb.EntityFrameworkCore.NuoDb");
            Assert.True(migrationBuilder.IsNuoDb());
        }

        [Fact]
        public void Not_IsSqlServer_when_using_different_provider()
        {
            var migrationBuilder = new MigrationBuilder("Microsoft.EntityFrameworkCore.InMemory");
            Assert.False(migrationBuilder.IsNuoDb());
        }
    }
}
