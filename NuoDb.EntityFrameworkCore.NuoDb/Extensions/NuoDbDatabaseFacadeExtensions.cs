// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure;
using NuoDb.EntityFrameworkCore.NuoDb.Infrastructure.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     NuoDb specific extension methods for <see cref="DbContext.Database" />.
    /// </summary>
    public static class NuoDbDatabaseFacadeExtensions
    {
        /// <summary>
        ///     <para>
        ///         Returns <see langword="true" /> if the database provider currently in use is the NuoDb provider.
        ///     </para>
        ///     <para>
        ///         This method can only be used after the <see cref="DbContext" /> has been configured because
        ///         it is only then that the provider is known. This means that this method cannot be used
        ///         in <see cref="DbContext.OnConfiguring" /> because this is where application code sets the
        ///         provider to use as part of configuring the context.
        ///     </para>
        /// </summary>
        /// </remarks>
        /// <param name="database">The facade from <see cref="DbContext.Database" />.</param>
        /// <returns><see langword="true" /> if NuoDb is being used; <see langword="false" /> otherwise.</returns>
        public static bool IsNuoDb(this DatabaseFacade database)
            => database.ProviderName == typeof(NuoDbOptionsExtension).Assembly.GetName().Name;
    }
}
