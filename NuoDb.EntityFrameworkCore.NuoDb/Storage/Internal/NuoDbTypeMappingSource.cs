// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic.CompilerServices;

namespace NuoDb.EntityFrameworkCore.NuoDb.Storage.Internal
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
    public class NuoDbTypeMappingSource : RelationalTypeMappingSource
    {
       
        private const string IntegerTypeName = "INTEGER";
        private const string DecimalTypeName = "DECIMAL";
        private const string LongTypeName = "BIGINT";
        private const string ShortTypeName = "SMALLINT";
        private const string RealTypeName = "DOUBLE";
        private const string BlobTypeName = "BLOB";
        private const string TextTypeName = "STRING";
        private const string BooleanTypeName = "BOOLEAN";
        private const string TimestampTypeName = "TIMESTAMP WITHOUT TIMEZONE";

        private static readonly ShortTypeMapping _short = new(ShortTypeName);
        private static readonly DecimalTypeMapping _decimal = new(DecimalTypeName);
        private static readonly IntTypeMapping _integer = new(IntegerTypeName);
        private static readonly LongTypeMapping _long = new(LongTypeName);
        private static readonly DoubleTypeMapping _real = new(RealTypeName);
        private static readonly ByteArrayTypeMapping _blob = new(BlobTypeName);
        private static readonly NuoDbStringTypeMapping _text = new(TextTypeName);
        private static readonly DateTimeTypeMapping _dateTime = new(TimestampTypeName);
        private static readonly BoolTypeMapping _bool = new(BooleanTypeName);

        private readonly Dictionary<Type, RelationalTypeMapping> _clrTypeMappings = new()
        {
            { typeof(string), _text },
            { typeof(byte[]), _blob },
            { typeof(bool), new NuoDbBooleanTypeMapping(BooleanTypeName) },
            { typeof(byte), new ByteTypeMapping(IntegerTypeName) },
            { typeof(char), new CharTypeMapping(TextTypeName) },
            { typeof(int), new IntTypeMapping(IntegerTypeName) },
            { typeof(long), new LongTypeMapping(LongTypeName) },
            { typeof(sbyte), new SByteTypeMapping(IntegerTypeName) },
            { typeof(short), new ShortTypeMapping(ShortTypeName) },
            { typeof(DateTime), new NuoDbDateTimeTypeMapping(TimestampTypeName) },
            { typeof(DateTimeOffset), new NuoDbDateTimeOffsetTypeMapping(TextTypeName) },
            // { typeof(TimeSpan), new TimeSpanTypeMapping(TextTypeName) },
            { typeof(DateOnly), new NuoDbDateOnlyTypeMapping(TextTypeName) },
            // { typeof(TimeOnly), new NuoDbTimeOnlyTypeMapping(TextTypeName) },
            { typeof(decimal), new NuoDbDecimalTypeMapping(DecimalTypeName) },
            { typeof(double), _real },
            { typeof(float), new FloatTypeMapping(RealTypeName) },
            { typeof(Guid), new NuoDbGuidTypeMapping(TextTypeName) }
        };

        private readonly Dictionary<string, RelationalTypeMapping> _storeTypeMappings = new(StringComparer.OrdinalIgnoreCase)
        {
            { IntegerTypeName, _integer },
            { LongTypeName, _long},
            { RealTypeName, _real },
            { BlobTypeName, _blob },
            { TextTypeName, _text },
            { ShortTypeName, _short},
            { DecimalTypeName, _decimal },
            { TimestampTypeName, _dateTime },
            { BooleanTypeName, _bool }
        };

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public NuoDbTypeMappingSource(
            TypeMappingSourceDependencies dependencies,
            RelationalTypeMappingSourceDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)
        {
            var mapping = base.FindMapping(mappingInfo) ?? FindRawMapping(mappingInfo);

            return mapping != null
                && mappingInfo.StoreTypeName != null
                    ? mapping.Clone(mappingInfo.StoreTypeName, null)
                    : mapping;
        }

        private RelationalTypeMapping? FindRawMapping(RelationalTypeMappingInfo mappingInfo)
        {
            var clrType = mappingInfo.ClrType;
            if (clrType != null
                && _clrTypeMappings.TryGetValue(clrType, out var mapping))
            {
                return mapping;
            }

            var storeTypeName = mappingInfo.StoreTypeName;
            if (storeTypeName != null
                && _storeTypeMappings.TryGetValue(storeTypeName, out mapping))
            {
                return mapping;
            }

            if (storeTypeName != null)
            {
                var affinityTypeMapping = _typeRules.Select(r => r(storeTypeName)).FirstOrDefault(r => r != null);

                if (affinityTypeMapping == null)
                {
                    return _blob;
                }

                if (clrType == null
                    || affinityTypeMapping.ClrType.UnwrapNullableType() == clrType)
                {
                    return affinityTypeMapping;
                }
            }

            return null;
        }

        private readonly Func<string, RelationalTypeMapping?>[] _typeRules =
        {
            name => Contains(name, "SMALLINT")
                ? _short
                : null,
            name => Contains(name, "INTEGER")
                ? _integer
                : null,
            name => Contains(name, "BIGINT")
                ? _long
                : null,
            name => Contains(name, "CHAR")
                || Contains(name, "CLOB")
                || Contains(name, "TEXT")
                    ? _text
                    : null,
            name => Contains(name, "DECIMAL")
                    ? _decimal
                    : null,
            name => Contains(name, "BLOB")
                || Contains(name, "BIN")
                    ? _blob
                    : null,
            name => Contains(name, "REAL")
                || Contains(name, "FLOA")
                || Contains(name, "DOUB")
                    ? _real
                    : null
        };

        private static bool Contains(string haystack, string needle)
            => haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
