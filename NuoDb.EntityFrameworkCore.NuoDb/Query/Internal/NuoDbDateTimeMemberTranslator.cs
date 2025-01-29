// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
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
        private readonly IRelationalTypeMappingSource _typeMappingSource;
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public NuoDbDateTimeMemberTranslator(ISqlExpressionFactory sqlExpressionFactory, IRelationalTypeMappingSource typeMappingSource)
        {
            _sqlExpressionFactory = (NuoDbSqlExpressionFactory)sqlExpressionFactory;
            _typeMappingSource = typeMappingSource;
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
            //Check.NotNull(instance, nameof(instance));
            Check.NotNull(member, nameof(member));
            Check.NotNull(returnType, nameof(returnType));
            Check.NotNull(logger, nameof(logger));

            if (member.DeclaringType == typeof(DateTime))
            {
                var memberName = member.Name;

                if (memberName == nameof(DateTime.DayOfWeek))
                {
                    Check.NotNull(instance, nameof(instance));
                    return _sqlExpressionFactory.ComplexFunctionArgument(
                            new SqlExpression[]
                            {
                                _sqlExpressionFactory.NullableFunction(
                                "DAYOFWEEK",
                                new SqlExpression[]
                                {
                                    instance
                                },
                                instance.Type,
                                instance.TypeMapping,
                                true,
                                new[] {true, false}),
                                _sqlExpressionFactory.Fragment(" - 1") //apply an offset to account for nuodb's day of week being 1 based index
                            }," ", typeof(string));
                }

                if (memberName == nameof(DateTime.Today))
                {
                    return _sqlExpressionFactory.Fragment("CURRENT_DATE");
                }

                if (memberName == nameof(DateTime.Date))
                {
                    Check.NotNull(instance, nameof(instance));
                    return _sqlExpressionFactory.Function(
                        "CAST",
                        new SqlExpression[]
                        {
                            new NuoDbComplexFunctionArgumentExpression(
                                new SqlExpression[]
                                {
                                    instance,
                                    _sqlExpressionFactory.Fragment("AS DATE")
                                },
                                " ",
                                typeof(string),
                                instance.TypeMapping
                            )
                        },
                        true,
                        new[] {true, false},
                        returnType
                    );
                }

                if (memberName == nameof(DateTime.Now))
                {
                    return _sqlExpressionFactory.Function("now", Array.Empty<SqlExpression>(),true,new []{true,false}, typeof(DateTime), _typeMappingSource.FindMapping(typeof(DateTime)));
                }

                if (memberName == nameof(DateTime.UtcNow))
                {
                    return _sqlExpressionFactory.Function("now", Array.Empty<SqlExpression>(),true, new []{true,false},typeof(DateTime),_typeMappingSource.FindMapping(typeof(DateTime)));
                }

                if (_datePartMapping.TryGetValue(memberName, out var datePart))
                {
                    Check.NotNull(instance, nameof(instance));
                    return _sqlExpressionFactory.Convert(
                        NuoDbExpression.DateToStr(
                            _sqlExpressionFactory,
                            typeof(string),
                            datePart,
                            instance),
                        returnType);
                }

                if (memberName == nameof(DateTime.TimeOfDay))
                {
                    Check.NotNull(instance, nameof(instance));
                    return _sqlExpressionFactory.Function(
                        "CAST",
                        new SqlExpression[]
                        {
                            new NuoDbComplexFunctionArgumentExpression(
                                new SqlExpression[]
                                {
                                    instance,
                                    _sqlExpressionFactory.Fragment("AS TIME")
                                },
                                " ",
                                typeof(TimeSpan),
                                instance.TypeMapping
                            )
                        },
                        true,
                        new[]{true,false},
                        returnType
                    );
                }

                // if (memberName == nameof(DateTime.Ticks))
                // {
                //     return _sqlExpressionFactory.Convert(
                //         _sqlExpressionFactory.Multiply(
                //             _sqlExpressionFactory.Subtract(
                //                 _sqlExpressionFactory.Function(
                //                     "julianday",
                //                     new[] { instance! },
                //                     nullable: true,
                //                     argumentsPropagateNullability: new[] { true },
                //                     typeof(double)),
                //                 _sqlExpressionFactory.Constant(1721425.5)), // NB: Result of julianday('0001-01-01 00:00:00')
                //             _sqlExpressionFactory.Constant(TimeSpan.TicksPerDay)),
                //         typeof(long));
                // }
                if (memberName == nameof(DateTime.Subtract))
                {

                }

                if (memberName == nameof(DateTime.Millisecond))
                {
                    Check.NotNull(instance, nameof(instance));
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
            }

            return null;
        }
    }
}
