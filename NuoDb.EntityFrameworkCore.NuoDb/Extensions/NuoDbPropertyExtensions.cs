// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata;
using NuoDb.EntityFrameworkCore.NuoDb.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IProperty" /> for NuoDb metadata.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    /// </remarks>
    public static class NuoDbPropertyExtensions
    {
        /// <summary>
        ///     Returns the SRID to use when creating a column for this property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The SRID to use when creating a column for this property.</returns>
        public static int? GetSrid(this IReadOnlyProperty property)
            => (int?)property[NuoDbAnnotationNames.Srid];

        /// <summary>
        ///     Returns the SRID to use when creating a column for this property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="storeObject">The identifier of the store object.</param>
        /// <returns>The SRID to use when creating a column for this property.</returns>
        public static int? GetSrid(
            this IReadOnlyProperty property,
            in StoreObjectIdentifier storeObject)
        {
            var annotation = property.FindAnnotation(NuoDbAnnotationNames.Srid);
            if (annotation != null)
            {
                return (int?)annotation.Value;
            }

            return property.FindSharedStoreObjectRootProperty(storeObject)?.GetSrid(storeObject);
        }

        /// <summary>
        ///     Sets the SRID to use when creating a column for this property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="value">The SRID.</param>
        public static void SetSrid(this IMutableProperty property, int? value)
            => property.SetOrRemoveAnnotation(NuoDbAnnotationNames.Srid, value);

        /// <summary>
        ///     Sets the SRID to use when creating a column for this property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="value">The SRID.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        public static void SetSrid(this IConventionProperty property, int? value, bool fromDataAnnotation = false)
            => property.SetOrRemoveAnnotation(NuoDbAnnotationNames.Srid, value, fromDataAnnotation);

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the column SRID.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The <see cref="ConfigurationSource" /> for the column SRID.</returns>
        public static ConfigurationSource? GetSridConfigurationSource(this IConventionProperty property)
            => property.FindAnnotation(NuoDbAnnotationNames.Srid)?.GetConfigurationSource();
    }
}
