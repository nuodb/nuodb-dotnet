// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using NuoDb.Data.Client;

namespace NuoDb.EntityFrameworkCore.NuoDb.Storage.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class NuoDbStringTypeMapping : StringTypeMapping
    {
        private const int UnicodeMax = 4000;
        private const int AnsiMax = 8000;

        private readonly DbType? _dbType;
        private readonly int _maxSpecificSize;
        private readonly int _maxSize;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public NuoDbStringTypeMapping(
            string? storeType = null,
            bool unicode = false,
            int? size = null,
            bool fixedLength = false,
            DbType? dbType = null,
            StoreTypePostfix? storeTypePostfix = null)
            : this(
                new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(typeof(string)),
                    storeType ?? GetStoreName(unicode, fixedLength),
                    storeTypePostfix ?? StoreTypePostfix.Size,
                    GetDbType(unicode, fixedLength),
                    unicode,
                    size,
                    fixedLength),
                dbType)
        {
        }

        private static string GetStoreName(bool unicode, bool fixedLength)
            => unicode
                ? fixedLength ? "char" : "varchar"
                : fixedLength
                    ? "char"
                    : "varchar";

        private static DbType? GetDbType(bool unicode, bool fixedLength)
            => unicode
                ? (fixedLength
                    ? System.Data.DbType.StringFixedLength
                    : System.Data.DbType.String)
                : (fixedLength
                    ? System.Data.DbType.AnsiStringFixedLength
                    : System.Data.DbType.AnsiString);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected NuoDbStringTypeMapping(RelationalTypeMappingParameters parameters, DbType? dbType)
            : base(parameters)
        {
            if (parameters.Unicode)
            {
                _maxSpecificSize = parameters.Size.HasValue && parameters.Size <= UnicodeMax
                    ? parameters.Size.Value
                    : UnicodeMax;
                _maxSize = UnicodeMax;
            }
            else
            {
                _maxSpecificSize = parameters.Size.HasValue && parameters.Size <= AnsiMax
                    ? parameters.Size.Value
                    : AnsiMax;
                _maxSize = AnsiMax;
            }

            _dbType = dbType;
        }

        /// <summary>
        ///     Creates a copy of this mapping.
        /// </summary>
        /// <param name="parameters">The parameters for this mapping.</param>
        /// <returns>The newly created mapping.</returns>
        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NuoDbStringTypeMapping(parameters, _dbType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override void ConfigureParameter(DbParameter parameter)
        {
            var value = parameter.Value;
            var length = (value as string)?.Length;

            if (_dbType.HasValue
                && parameter is NuoDbParameter sqlParameter) // To avoid crashing wrapping providers
            {
                sqlParameter.DbType = _dbType.Value;
            }

            if ((value == null
                 || value == DBNull.Value)
                || (IsFixedLength
                    && length == _maxSpecificSize
                    && Size.HasValue))
            {
                // A fixed-length parameter where the value matches the length can remain a fixed-length parameter
                // because SQLClient will not do any padding or truncating.
                parameter.Size = _maxSpecificSize;
            }
            else
            {
                if (IsFixedLength)
                {
                    // Force the parameter type to be not fixed length to avoid SQLClient truncation and padding.
                    parameter.DbType = IsUnicode ? System.Data.DbType.String : System.Data.DbType.AnsiString;
                }

                // For strings and byte arrays, set the max length to the size facet if specified, or
                // 8000 bytes if no size facet specified, if the data will fit so as to avoid query cache
                // fragmentation by setting lots of different Size values otherwise set to the max bounded length
                // if the value will fit, otherwise set to -1 (unbounded) to avoid SQL client size inference.
                if (length != null
                    && length <= _maxSpecificSize)
                {
                    parameter.Size = _maxSpecificSize;
                }
                else if (length != null
                         && length <= _maxSize)
                {
                    parameter.Size = _maxSize;
                }
                else
                {
                    parameter.Size = -1;
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var stringValue = (string)value;
            var builder = new StringBuilder();

            var start = 0;
            int i;
            int length;
            var openApostrophe = false;
            var lengths = new List<int>();
            var startIndexes = new List<int> { 0 };
            for (i = 0; i < stringValue.Length; i++)
            {
                var lineFeed = stringValue[i] == '\n';
                var carriageReturn = stringValue[i] == '\r';
                var apostrophe = stringValue[i] == '\'';
                if (lineFeed || carriageReturn || apostrophe)
                {
                    length = i - start;
                    if (length != 0)
                    {
                        if (!openApostrophe)
                        {
                            if (builder.Length != 0)
                            {
                                lengths.Add(builder.Length - startIndexes[^1]);
                                startIndexes.Add(builder.Length);
                            }

                            builder.Append('\'');
                            openApostrophe = true;
                        }

                        builder.Append(stringValue.AsSpan().Slice(start, length));
                    }

                    if (lineFeed || carriageReturn)
                    {
                        if (openApostrophe)
                        {
                            builder.Append('\'');
                            openApostrophe = false;
                        }

                        if (builder.Length != 0)
                        {
                            lengths.Add(builder.Length - startIndexes[^1]);
                            startIndexes.Add(builder.Length);
                        }

                        builder
                            .Append("CHAR(")
                            .Append(lineFeed ? "10" : "13")
                            .Append(')');
                    }
                    else if (apostrophe)
                    {
                        if (!openApostrophe)
                        {
                            if (builder.Length != 0)
                            {
                                lengths.Add(builder.Length - startIndexes[^1]);
                                startIndexes.Add(builder.Length);
                            }

                            builder.Append('\'');
                            openApostrophe = true;
                        }

                        builder.Append("''");
                    }

                    start = i + 1;
                }
            }

            length = i - start;
            if (length != 0)
            {
                if (!openApostrophe)
                {
                    if (builder.Length != 0)
                    {
                        lengths.Add(builder.Length - startIndexes[^1]);
                        startIndexes.Add(builder.Length);
                    }

                    builder.Append('\'');
                    openApostrophe = true;
                }

                builder.Append(stringValue.AsSpan().Slice(start, length));
            }

            if (openApostrophe)
            {
                builder.Append('\'');
            }

            if (builder.Length != 0)
            {
                lengths.Add(builder.Length - startIndexes[^1]);
            }

            if (lengths.Count == 0
                && builder.Length == 0)
            {
                return "''";
            }

            var newBuilder = new StringBuilder();
            GenerateBalancedTree(0, lengths.Count);

            return newBuilder.ToString();

            void GenerateBalancedTree(int start, int end)
            {
                var count = end - start;
                if (count < 1)
                {
                    return;
                }

                if (count == 1)
                {
                    newBuilder.Append(builder, startIndexes[start], lengths[start]);
                    return;
                }

                var mid = start + count / 2;
                newBuilder.Append('(');
                GenerateBalancedTree(start, mid);
                newBuilder.Append(" || ");
                GenerateBalancedTree(mid, end);
                newBuilder.Append(')');
            }
        }
    }
}
