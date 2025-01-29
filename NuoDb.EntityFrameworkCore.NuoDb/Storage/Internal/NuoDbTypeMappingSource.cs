// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Compression;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
       
        private readonly ShortTypeMapping _short
            = new ShortTypeMapping("smallint");

        private readonly LongTypeMapping _long
            = new LongTypeMapping("bigint");

        private readonly IntTypeMapping _int
            = new("integer");


        private readonly NuoDbByteArrayTypeMapping _blob = new NuoDbByteArrayTypeMapping("BLOB");

        private readonly BoolTypeMapping _bool
            = new NuoDbBooleanTypeMapping("boolean");

        private readonly NuoDbStringTypeMapping _fixedLengthUnicodeString
            = new(unicode: true, fixedLength: true);

        private readonly NuoDbStringTypeMapping _variableLengthUnicodeString
            = new(unicode: true);

        private readonly NuoDbStringTypeMapping _variableLengthMaxUnicodeString
            = new("string", unicode: true, storeTypePostfix: StoreTypePostfix.None);

        private readonly NuoDbStringTypeMapping _fixedLengthAnsiString
            = new(fixedLength: true);

        private readonly NuoDbStringTypeMapping _textAnsiString
            = new("string", storeTypePostfix: StoreTypePostfix.None);

        private readonly NuoDbStringTypeMapping _variableLengthAnsiString
            = new();

        private readonly NuoDbStringTypeMapping _variableLengthMaxAnsiString
            = new("string", storeTypePostfix: StoreTypePostfix.None);

        private readonly NuoDbByteArrayTypeMapping _variableLengthBinary
            = new();

        private readonly NuoDbByteArrayTypeMapping _variableLengthMaxBinary
            = new("blob", storeTypePostfix: StoreTypePostfix.None);

        private readonly NuoDbByteArrayTypeMapping _fixedLengthBinary
            = new(fixedLength: true);

        private readonly NuoDbDateTimeTypeMapping _datetime
            = new("TIMESTAMP WITHOUT TIME ZONE", DbType.DateTime);

        private readonly NuoDbTimeOnlyTypeMapping _timeOnly
            = new("TIME", DbType.Time);

        private readonly NuoDbDateTimeOffsetTypeMapping _dateTimeOffset
            = new("TIMESTAMP", DbType.DateTimeOffset);

        private readonly ByteTypeMapping _byte
            = new ByteTypeMapping("binary(1)");
        private readonly DoubleTypeMapping _double
            = new DoubleTypeMapping("double");

        
        // private readonly SqlServerDateTimeOffsetTypeMapping _datetimeoffset
        //     = new("datetimeoffset");
        //
        // private readonly SqlServerDateTimeOffsetTypeMapping _datetimeoffsetAlias
        //     = new("placeholder", DbType.DateTimeOffset, StoreTypePostfix.None);

        private readonly GuidTypeMapping _uniqueidentifier
            = new NuoDbGuidTypeMapping("varchar(40)");

 

        private readonly DecimalTypeMapping _decimal
            = new NuoDbDecimalTypeMapping("decimal");

        private readonly DecimalTypeMapping _decimalAlias
            = new NuoDbDecimalTypeMapping("placeholder", precision: 18, scale: 2, storeTypePostfix: StoreTypePostfix.None);

        private readonly DecimalTypeMapping _decimal182
            = new NuoDbDecimalTypeMapping("decimal(18, 2)", precision: 18, scale: 2);

        private readonly DecimalTypeMapping _money
            = new NuoDbDecimalTypeMapping("money", storeTypePostfix: StoreTypePostfix.None);

        private readonly NuoDbDateOnlyTypeMapping _date
            = new("date", DbType.Date);

        private readonly NuoDbTimeOnlyTypeMapping _time
            = new("time", DbType.Time);


        private readonly NuoDbStringTypeMapping _xml
            = new("xml", unicode: true, storeTypePostfix: StoreTypePostfix.None);

        private readonly Dictionary<Type, RelationalTypeMapping> _clrTypeMappings;

        private readonly Dictionary<Type, RelationalTypeMapping> _clrNoFacetTypeMappings;

        private readonly Dictionary<string, RelationalTypeMapping> _storeTypeMappings;

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
            _clrTypeMappings
                = new Dictionary<Type, RelationalTypeMapping>
                {
                    { typeof(int), _int },
                    { typeof(long), _long },
                    { typeof(Guid), _uniqueidentifier },
                    { typeof(DateOnly), _date},
                    { typeof(DateTime), _datetime},
                    { typeof(TimeOnly), _timeOnly},
                    { typeof(DateTimeOffset), _dateTimeOffset},
                    { typeof(bool), _bool },
                    { typeof(byte), _byte },
                    { typeof(double), _double },
                    //{ typeof(DateTimeOffset), _datetimeoffset },
                    { typeof(short), _short },
                    { typeof(decimal), _decimal182 },
                };

            _clrNoFacetTypeMappings
                = new Dictionary<Type, RelationalTypeMapping>
                {
                   // { typeof(DateTimeOffset), _datetimeoffsetAlias },
                    { typeof(decimal), _decimalAlias }
                };

            _storeTypeMappings
                = new Dictionary<string, RelationalTypeMapping>(StringComparer.OrdinalIgnoreCase)
                {
                    { "bigint", _long },
                    { "blob", _blob },
                    { "binary varying", _variableLengthBinary },
                    { "binary", _fixedLengthBinary },
                    { "boolean", _bool },
                    { "char varying", _variableLengthAnsiString },
                    { "char varying(max)", _variableLengthMaxAnsiString },
                    { "char", _fixedLengthAnsiString },
                    { "character varying", _variableLengthAnsiString },
                    { "character varying(max)", _variableLengthMaxAnsiString },
                    { "character", _fixedLengthAnsiString },
                    { "date", _date },
                    { "time", _timeOnly},
                    { "datetime", _datetime },
                    { "TIMESTAMP", _dateTimeOffset },
                    { "TIMESTAMP WITHOUT TIME ZONE", _datetime },
                    { "decimal", _decimal },
                    { "double", _double },
                    { "float", _double },
                    { "integer", _int },
                    { "money", _money },
                    { "string", _textAnsiString },
                    { "now()", _datetime},
                    { "nchar", _fixedLengthUnicodeString },
                    { "numeric", _decimal },
                    { "nvarchar", _variableLengthUnicodeString },
                    { "smalldatetime", _datetime },
                    { "smallint", _short },
                    { "smallmoney", _money },
                    { "text", _textAnsiString },
                    { "binary(1)", _byte },
                    { "varbinary", _variableLengthBinary },
                    { "varbinary(max)", _variableLengthMaxBinary },
                    { "varchar", _variableLengthAnsiString },
                    { "varchar(max)", _variableLengthMaxAnsiString },
                    { "xml", _xml }
                };
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)
            => base.FindMapping(mappingInfo) ?? FindRawMapping(mappingInfo)?.Clone(mappingInfo);

        private RelationalTypeMapping? FindRawMapping(RelationalTypeMappingInfo mappingInfo)
        {
            var clrType = mappingInfo.ClrType;
            var storeTypeName = mappingInfo.StoreTypeName;

            if (storeTypeName != null)
            {
                var storeTypeNameBase = mappingInfo.StoreTypeNameBase;
                if (storeTypeNameBase!.StartsWith("[", StringComparison.Ordinal)
                    && storeTypeNameBase.EndsWith("]", StringComparison.Ordinal))
                {
                    storeTypeNameBase = storeTypeNameBase.Substring(1, storeTypeNameBase.Length - 2);
                }

                if (clrType == typeof(float)
                    && mappingInfo.Precision != null
                    && mappingInfo.Precision <= 24
                    && (storeTypeNameBase.Equals("float", StringComparison.OrdinalIgnoreCase)
                        || storeTypeNameBase.Equals("double", StringComparison.OrdinalIgnoreCase)))
                {
                    return _double;
                }

                if (_storeTypeMappings.TryGetValue(storeTypeName, out var mapping)
                    || _storeTypeMappings.TryGetValue(storeTypeNameBase, out mapping))
                {
                    return clrType == null
                        || mapping.ClrType == clrType
                            ? mapping
                            : null;
                }

                if (clrType != null
                    && _clrNoFacetTypeMappings.TryGetValue(clrType, out mapping))
                {
                    return mapping;
                }
            }

            if (clrType != null)
            {
                if (_clrTypeMappings.TryGetValue(clrType, out var mapping))
                {
                    return mapping;
                }

                if (clrType == typeof(string))
                {
                    var isAnsi = mappingInfo.IsUnicode == false;
                    var isFixedLength = mappingInfo.IsFixedLength == true;
                    var maxSize = isAnsi ? 8000 : 4000;

                    var size = mappingInfo.Size ?? (mappingInfo.IsKeyOrIndex ? isAnsi ? 900 : 450 : null);
                    if (size > maxSize)
                    {
                        size = isFixedLength ? maxSize : null;
                    }

                    if (size == -1)
                    {
                        size = null;
                    }

                    if (size == null
                        && storeTypeName == null)
                    {
                        return isAnsi
                            ? isFixedLength
                                ? _fixedLengthAnsiString
                                : _variableLengthMaxAnsiString
                            : isFixedLength
                                ? _fixedLengthUnicodeString
                                : _variableLengthMaxUnicodeString;
                    }

                    return new NuoDbStringTypeMapping(
                        unicode: !isAnsi,
                        size: size,
                        fixedLength: isFixedLength,
                        storeTypePostfix: storeTypeName == null ? StoreTypePostfix.Size : StoreTypePostfix.None);
                }

                if (clrType == typeof(byte[]))
                {
                    var isFixedLength = mappingInfo.IsFixedLength == true;

                    var size = mappingInfo.Size ?? (mappingInfo.IsKeyOrIndex ? 900 : null);
                    if (size > 8000)
                    {
                        size = isFixedLength ? 8000 : null;
                    }

                    return size == null
                        ? _variableLengthMaxBinary
                        : new NuoDbByteArrayTypeMapping(
                            size: size,
                            fixedLength: isFixedLength,
                            storeTypePostfix: storeTypeName == null ? StoreTypePostfix.Size : StoreTypePostfix.None);
                }
            }

            return null;
        }

        private static readonly List<string> _nameBasesUsingPrecision = new()
        {
            "decimal",
            "numeric",
            "double",
        };

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override string? ParseStoreTypeName(
            string? storeTypeName,
            ref bool? unicode,
            ref int? size,
            ref int? precision,
            ref int? scale)
        {
            var parsedName = base.ParseStoreTypeName(storeTypeName, ref unicode, ref size, ref precision, ref scale);

            if (size.HasValue
                && storeTypeName != null
                && _nameBasesUsingPrecision.Any(n => storeTypeName.StartsWith(n, StringComparison.OrdinalIgnoreCase)))
            {
                precision = size;
                size = null;
            }

            return parsedName;
        }
    }
}
