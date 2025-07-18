// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using NuoDb.EntityFrameworkCore.NuoDb.Storage.Internal;

namespace NuoDb.EntityFrameworkCore.NuoDb.Metadata.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class NuoDbAnnotationProvider : RelationalAnnotationProvider
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public NuoDbAnnotationProvider(RelationalAnnotationProviderDependencies dependencies)
            : base(dependencies)
        {
        }

        // /// <summary>
        // ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        // ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        // ///     any release. You should only use it directly in your code with extreme caution and knowing that
        // ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        // /// </summary>
        // public override IEnumerable<IAnnotation> For(IRelationalModel model, bool designTime)
        // {
        //     if (model.Tables.SelectMany(t => t.Columns).Any(
        //         c => NuoDbTypeMappingSource.IsSpatialiteType(c.StoreType)))
        //     {
        //         yield return new Annotation(NuoDbAnnotationNames.InitSpatialMetaData, true);
        //     }
        // }

        public override IEnumerable<IAnnotation> For(ITable table, bool designTime)
        {
            return base.For(table, designTime);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override IEnumerable<IAnnotation> For(IColumn column, bool designTime)
        {
            // Model validation ensures that these facets are the same on all mapped properties
            var property = column.PropertyMappings.First().Property;
            // Only return auto increment for integer single column primary key
            var primaryKey = property.DeclaringType.ContainingEntityType.FindPrimaryKey();
            if (primaryKey is { Properties.Count: 1 }
                && primaryKey.Properties[0] == property
                && property.ValueGenerated == ValueGenerated.OnAdd
                && property.ClrType.UnwrapNullableType().IsInteger()
                && !HasConverter(property))
            {
                yield return new Annotation(NuoDbAnnotationNames.Autoincrement, true);
            }
        }

        private static bool HasConverter(IProperty property)
            => (property.GetValueConverter() ?? property.FindTypeMapping()?.Converter) != null;
    }
}
