// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace NuoDb.EntityFrameworkCore.NuoDb.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class NuoDbDateTimeMemberTranslator : IMemberTranslator
    {
        private static readonly Dictionary<string, string> _datePartMapping
            = new()
            {
                { nameof(DateTime.Year), "yyyy" },
                { nameof(DateTime.Month), "MM" },
                { nameof(DateTime.DayOfYear), "D" },
                { nameof(DateTime.Day), "dd" },
                { nameof(DateTime.Hour), "HH" },
                { nameof(DateTime.Minute), "mm" },
                { nameof(DateTime.Second), "ss" },
                { nameof(DateTime.DayOfWeek), "e" }
            };

        private readonly NuoDbSqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public NuoDbDateTimeMemberTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = (NuoDbSqlExpressionFactory)sqlExpressionFactory;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlExpression? Translate(
            SqlExpression? instance,
            MemberInfo member,
            Type returnType,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            Check.NotNull(member, nameof(member));
            Check.NotNull(returnType, nameof(returnType));
            Check.NotNull(logger, nameof(logger));

            if (member.DeclaringType == typeof(DateTime))
            {
                var memberName = member.Name;

                if (_datePartMapping.TryGetValue(memberName, out var datePart))
                {
                    return _sqlExpressionFactory.Convert(
                        NuoDbExpression.DateToStr(
                            _sqlExpressionFactory,
                            typeof(string),
                            datePart,
                            instance!),
                        returnType);
                }

                if (memberName == nameof(DateTime.Ticks))
                {
                    return _sqlExpressionFactory.Convert(
                        _sqlExpressionFactory.Multiply(
                            _sqlExpressionFactory.Subtract(
                                _sqlExpressionFactory.Function(
                                    "julianday",
                                    new[] { instance! },
                                    nullable: true,
                                    argumentsPropagateNullability: new[] { true },
                                    typeof(double)),
                                _sqlExpressionFactory.Constant(1721425.5)), // NB: Result of julianday('0001-01-01 00:00:00')
                            _sqlExpressionFactory.Constant(TimeSpan.TicksPerDay)),
                        typeof(long));
                }

                if (memberName == nameof(DateTime.Millisecond))
                {
                    return _sqlExpressionFactory.Modulo(
                        _sqlExpressionFactory.Multiply(
                            _sqlExpressionFactory.Convert(
                                NuoDbExpression.DateToStr(
                                    _sqlExpressionFactory,
                                    typeof(string),
                                    "SSS",
                                    instance!),
                                typeof(double)),
                            _sqlExpressionFactory.Constant(1000)),
                        _sqlExpressionFactory.Constant(1000));
                }

                var format = "yyyy-MM-dd HH:mm:ss.SSS";
                SqlExpression timestring;
                var modifiers = new List<SqlExpression>();

                switch (memberName)
                {
                    case nameof(DateTime.Now):
                        return _sqlExpressionFactory.Function("now", Array.Empty<SqlExpression>(),typeof(DateTime));
                    case nameof(DateTime.UtcNow):
                        return _sqlExpressionFactory.Function("now", Array.Empty<SqlExpression>(),typeof(DateTime));

                    case nameof(DateTime.Date):
                        format = "yyyy-MM-dd";
                        timestring = instance!;
                        return _sqlExpressionFactory.ComplexFunctionArgument(
                            new SqlExpression[]
                            {
                                NuoDbExpression.DateToStr(
                                    _sqlExpressionFactory,
                                    returnType,
                                    format,
                                    timestring,
                                    modifiers),
                                _sqlExpressionFactory.Fragment("||' 00:00:00'"),
                            },
                            " ",
                            typeof(string));
                       

                    case nameof(DateTime.Today):
                        timestring = _sqlExpressionFactory.Constant("now");
                        modifiers.Add(_sqlExpressionFactory.Constant("localtime"));
                        modifiers.Add(_sqlExpressionFactory.Constant("start of day"));
                        break;

                    case nameof(DateTime.TimeOfDay):
                        format = "HH:mm:ss.SSS";
                        timestring = instance!;
                        break;

                    default:
                        return null;
                }

                Check.DebugAssert(timestring != null, "timestring is null");

                return
                    NuoDbExpression.DateToStr(
                        _sqlExpressionFactory,
                        returnType,
                        format,
                        timestring,
                        modifiers);
                    // _sqlExpressionFactory.Function(
                    // "rtrim",
                    // new SqlExpression[]
                    // {
                    //     _sqlExpressionFactory.Function(
                    //         "rtrim",
                    //         new SqlExpression[]
                    //         {
                    //             NuoDbExpression.DateToStr(
                    //                 _sqlExpressionFactory,
                    //                 returnType,
                    //                 format,
                    //                 timestring,
                    //                 modifiers),
                    //             _sqlExpressionFactory.Constant("0")
                    //         },
                    //         nullable: true,
                    //         argumentsPropagateNullability: new[] { true, false },
                    //         returnType),
                    //     _sqlExpressionFactory.Constant(".")
                    // },
                    // nullable: true,
                    // argumentsPropagateNullability: new[] { true, false },
                    // returnType);
            }

            return null;
        }
    }
}
