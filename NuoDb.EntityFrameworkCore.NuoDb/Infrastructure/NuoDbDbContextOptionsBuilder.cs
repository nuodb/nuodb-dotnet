// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using NuoDb.EntityFrameworkCore.NuoDb.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Allows NuoDb specific configuration to be performed on <see cref="DbContextOptions" />.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from a call to
    ///         <see
    ///             cref="NuoDbDbContextOptionsBuilderExtensions.UseNuoDb" />
    ///         and it is not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    /// </remarks>
    public class NuoDbDbContextOptionsBuilder : RelationalDbContextOptionsBuilder<NuoDbDbContextOptionsBuilder, NuoDbOptionsExtension>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NuoDbDbContextOptionsBuilder" /> class.
        /// </summary>
        /// <param name="optionsBuilder">The options builder.</param>
        public NuoDbDbContextOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
            : base(optionsBuilder)
        {
        }
    }
}
