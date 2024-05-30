// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
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
    public class NuoDbMathTranslator : IMethodCallTranslator
    {
        private static readonly Dictionary<MethodInfo, string> _supportedMethods = new()
        {
            { typeof(Math).GetRequiredMethod(nameof(Math.Abs), typeof(double)), "abs" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Abs), typeof(float)), "abs" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Abs), typeof(int)), "abs" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Abs), typeof(long)), "abs" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Abs), typeof(sbyte)), "abs" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Abs), typeof(short)), "abs" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Abs), typeof(decimal)), "abs" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Acos), typeof(double)), "acos" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Asin), typeof(double)), "asin" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Atan), typeof(double)), "atan" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Tan), typeof(double)), "tan" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Sin), typeof(double)), "sin" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Sqrt), typeof(double)), "sqrt" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Atan2), typeof(double), typeof(double)), "atan2" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Pow), typeof(double), typeof(double)), "power" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Ceiling), typeof(double)), "CEIL" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Ceiling), typeof(decimal)), "CEIL" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Floor), typeof(double)), "floor" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Floor), typeof(decimal)), "floor" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Cos), typeof(double)), "cos" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Round), typeof(double)), "round" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Round), typeof(decimal)), "round" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Round), typeof(decimal), typeof(int)), "round" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Round), typeof(double), typeof(int)), "round" },

            { typeof(MathF).GetRequiredMethod(nameof(MathF.Abs), typeof(float)), "abs" },
            { typeof(MathF).GetRequiredMethod(nameof(MathF.Max), typeof(float), typeof(float)), "max" },
            { typeof(MathF).GetRequiredMethod(nameof(MathF.Acos), typeof(float)), "acos" },
            { typeof(MathF).GetRequiredMethod(nameof(MathF.Cos), typeof(float)), "cos" },
            { typeof(MathF).GetRequiredMethod(nameof(MathF.Asin), typeof(float)), "asin" },
            { typeof(MathF).GetRequiredMethod(nameof(MathF.Sin), typeof(float)), "sin" },
            { typeof(MathF).GetRequiredMethod(nameof(MathF.Atan), typeof(float)), "atan" },
            { typeof(MathF).GetRequiredMethod(nameof(MathF.Tan), typeof(float)), "tan" },
            { typeof(MathF).GetRequiredMethod(nameof(MathF.Sqrt), typeof(float)), "sqrt" },
            { typeof(MathF).GetRequiredMethod(nameof(MathF.Ceiling), typeof(float)), "ceil" },
            { typeof(MathF).GetRequiredMethod(nameof(MathF.Floor), typeof(float)), "floor" },
            { typeof(MathF).GetRequiredMethod(nameof(MathF.Round), typeof(float)), "round" },
            { typeof(MathF).GetRequiredMethod(nameof(MathF.Round), typeof(float), typeof(int)), "round" },
            { typeof(MathF).GetRequiredMethod(nameof(MathF.Atan2), typeof(float), typeof(float)), "atan2" },
            { typeof(MathF).GetRequiredMethod(nameof(MathF.Pow), typeof(float), typeof(float)), "power" },

        };

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public NuoDbMathTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
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

            if (_supportedMethods.TryGetValue(method, out var sqlFunctionName))
            {
                RelationalTypeMapping? typeMapping;
                List<SqlExpression>? newArguments = null;
                if (sqlFunctionName == "max" || sqlFunctionName == "max")
                {
                    typeMapping = ExpressionExtensions.InferTypeMapping(arguments![0]!, arguments[1]!);
                    newArguments = new List<SqlExpression>
                    {
                        _sqlExpressionFactory.ApplyTypeMapping(arguments[0], typeMapping),
                        _sqlExpressionFactory.ApplyTypeMapping(arguments[1], typeMapping)
                    };
                }
                else
                {
                    typeMapping = arguments[0].TypeMapping;
                }

                // nuo db expects these arguments in inverse order, so rebuild arguments list
                if (sqlFunctionName == "atan2")
                {
                    typeMapping = ExpressionExtensions.InferTypeMapping(arguments![0]!, arguments[1]!);
                    newArguments = new List<SqlExpression>
                    {
                        _sqlExpressionFactory.ApplyTypeMapping(arguments[1], typeMapping),
                        _sqlExpressionFactory.ApplyTypeMapping(arguments[0], typeMapping)
                    };
                }

                var finalArguments = newArguments ?? arguments;

                return _sqlExpressionFactory.Function(
                    sqlFunctionName,
                    finalArguments,
                    nullable: true,
                    argumentsPropagateNullability: finalArguments.Select(a => true).ToList(),
                    method.ReturnType,
                    typeMapping);
            }

            return null;
        }
    }
}
