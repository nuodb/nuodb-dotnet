// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;
using NuoDb.EntityFrameworkCore.NuoDb.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     NuoDb-specific extension methods for <see cref="PropertyBuilder" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public static class NuoDbPropertyBuilderExtensions
    {
        /// <summary>
        ///     Configures the SRID of the column that the property maps to when targeting NuoDb.
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-spatial">Spatial data</see>, and
        /// </remarks>
        /// <param name="propertyBuilder">The builder for the property being configured.</param>
        /// <param name="srid">The SRID.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder HasSrid(this PropertyBuilder propertyBuilder, int srid)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            propertyBuilder.Metadata.SetSrid(srid);

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the SRID of the column that the property maps to when targeting NuoDb.
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-spatial">Spatial data</see>, and
        /// </remarks>
        /// <param name="propertyBuilder">The builder for the property being configured.</param>
        /// <param name="srid">The SRID.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder<TProperty> HasSrid<TProperty>(
            this PropertyBuilder<TProperty> propertyBuilder,
            int srid)
            => (PropertyBuilder<TProperty>)HasSrid((PropertyBuilder)propertyBuilder, srid);

        /// <summary>
        ///     Configures the SRID of the column that the property maps to when targeting NuoDb.
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-spatial">Spatial data</see>, and
        /// </remarks>
        /// <param name="propertyBuilder">The builder for the property being configured.</param>
        /// <param name="srid">The SRID.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionPropertyBuilder? HasSrid(
            this IConventionPropertyBuilder propertyBuilder,
            int? srid,
            bool fromDataAnnotation = false)
        {
            if (propertyBuilder.CanSetSrid(srid, fromDataAnnotation))
            {
                propertyBuilder.Metadata.SetSrid(srid, fromDataAnnotation);

                return propertyBuilder;
            }

            return null;
        }

        /// <summary>
        ///     Returns a value indicating whether the given value can be set as the SRID for the column.
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-spatial">Spatial data</see>, and
        /// </remarks>
        /// <param name="propertyBuilder">The builder for the property being configured.</param>
        /// <param name="srid">The SRID.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns><see langword="true" /> if the given value can be set as the SRID for the column.</returns>
        public static bool CanSetSrid(
            this IConventionPropertyBuilder propertyBuilder,
            int? srid,
            bool fromDataAnnotation = false)
            => Check.NotNull(propertyBuilder, nameof(propertyBuilder)).CanSetAnnotation(
                NuoDbAnnotationNames.Srid,
                srid,
                fromDataAnnotation);
    }
}
