// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using NuoDb.EntityFrameworkCore.NuoDb.Infrastructure.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Migrations
{
    /// <summary>
    ///     NuoDb specific extension methods for <see cref="MigrationBuilder" />.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public static class NuoDbMigrationBuilderExtensions
    {
        /// <summary>
        ///     <para>
        ///         Returns <see langword="true" /> if the database provider currently in use is the NuoDb provider.
        ///     </para>
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="migrationBuilder">
        ///     The migrationBuilder from the parameters on <see cref="Migration.Up(MigrationBuilder)" /> or
        ///     <see cref="Migration.Down(MigrationBuilder)" />.
        /// </param>
        /// <returns><see langword="true" /> if NuoDb is being used; <see langword="false" /> otherwise.</returns>
        public static bool IsNuoDb(this MigrationBuilder migrationBuilder)
            => string.Equals(
                migrationBuilder.ActiveProvider,
                typeof(NuoDbOptionsExtension).Assembly.GetName().Name,
                StringComparison.Ordinal);
    }
}
