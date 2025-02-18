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
using NuoDb.EntityFrameworkCore.NuoDb.Storage.Internal;

namespace NuoDb.EntityFrameworkCore.NuoDb.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class NuoDbDateTimeAddTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _addMilliseconds
            = typeof(DateTime).GetRequiredRuntimeMethod(nameof(DateTime.AddMilliseconds), typeof(double));

        private static readonly MethodInfo _addTicks
            = typeof(DateTime).GetRequiredRuntimeMethod(nameof(DateTime.AddTicks), typeof(long));

        private readonly Dictionary<MethodInfo, string> _methodInfoToUnitSuffix = new()
        {
            { typeof(DateTime).GetRequiredRuntimeMethod(nameof(DateTime.AddYears), typeof(int)), "YEAR" },
            { typeof(DateTime).GetRequiredRuntimeMethod(nameof(DateTime.AddMonths), typeof(int)), "MONTH" },
            { typeof(DateTime).GetRequiredRuntimeMethod(nameof(DateTime.AddDays), typeof(double)), "DAY" },
            { typeof(DateTime).GetRequiredRuntimeMethod(nameof(DateTime.AddHours), typeof(double)), "HOUR" },
            { typeof(DateTime).GetRequiredRuntimeMethod(nameof(DateTime.AddMinutes), typeof(double)), "MINUTE" },
            { typeof(DateTime).GetRequiredRuntimeMethod(nameof(DateTime.AddSeconds), typeof(double)), "SECOND" },

            { typeof(DateOnly).GetRequiredRuntimeMethod(nameof(DateOnly.AddYears), typeof(int)), "YEAR" },
            { typeof(DateOnly).GetRequiredRuntimeMethod(nameof(DateOnly.AddMonths), typeof(int)), "MONTH" },
            { typeof(DateOnly).GetRequiredRuntimeMethod(nameof(DateOnly.AddDays), typeof(int)), " DAY" },
        };

        private readonly NuoDbSqlExpressionFactory _sqlExpressionFactory;
        private readonly NuoDbTypeMappingSource _typeMappingSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public NuoDbDateTimeAddTranslator(ISqlExpressionFactory sqlExpressionFactory, NuoDbTypeMappingSource nuodbTypeMappingSource)
        {
            _sqlExpressionFactory = (NuoDbSqlExpressionFactory)sqlExpressionFactory;
            _typeMappingSource = nuodbTypeMappingSource;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlExpression? Translate(
            SqlExpression? instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            Check.NotNull(method, nameof(method));
            Check.NotNull(arguments, nameof(arguments));
            Check.NotNull(logger, nameof(logger));

            return method.DeclaringType == typeof(DateTime)
                ? TranslateDateTime(instance, method, arguments)
                : method.DeclaringType == typeof(DateOnly)
                    ? TranslateDateOnly(instance, method, arguments)
                    : null;
        }

        private SqlExpression? TranslateDateTime(
            SqlExpression? instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments)
        {
            if (instance is SqlConstantExpression instanceConstant)
            {
                instance = instanceConstant.ApplyTypeMapping(_typeMappingSource.FindMapping(typeof(DateTime), "TIMESTAMP"));
            }

            if(_methodInfoToUnitSuffix.TryGetValue(method, out var datePart))
            {
                return _sqlExpressionFactory.NullableFunction(
                        "DATE_ADD",
                        new[]
                        {
                            instance,
                            _sqlExpressionFactory.ComplexFunctionArgument(
                                new SqlExpression[]
                                {
                                    _sqlExpressionFactory.Fragment("INTERVAL"),
                                    _sqlExpressionFactory.Convert(arguments[0], typeof(int)),
                                    _sqlExpressionFactory.Fragment(datePart)
                                },
                                " ",
                                typeof(string))
                        },
                        instance.Type,
                        instance.TypeMapping,
                        true,
                        new[] {true, false});
                // var amount = arguments[0];
                // return NuoDbExpression.DateAdd(_sqlExpressionFactory, method.ReturnType, instance!, unitSuffix, amount,
                //     Array.Empty<SqlExpression>());
            }
            
            return null;
        }

        private SqlExpression? TranslateDateOnly(
            SqlExpression? instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments)
        {
            if (instance is not null && _methodInfoToUnitSuffix.TryGetValue(method, out var unitSuffix))
            {
                return _sqlExpressionFactory.Function(
                    "date",
                    new[]
                    {
                        instance,
                        _sqlExpressionFactory.Add(
                            _sqlExpressionFactory.Convert(arguments[0], typeof(string)),
                            _sqlExpressionFactory.Constant(unitSuffix))
                    },
                    argumentsPropagateNullability: new[] { true, true },
                    nullable: true,
                    returnType: method.ReturnType);
            }

            return null;
        }
    }
}
