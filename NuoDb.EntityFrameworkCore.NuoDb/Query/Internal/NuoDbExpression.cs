// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace NuoDb.EntityFrameworkCore.NuoDb.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class NuoDbExpression
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static SqlFunctionExpression DateToStr(
            ISqlExpressionFactory sqlExpressionFactory,
            Type returnType,
            string format,
            SqlExpression timestring,
            IEnumerable<SqlExpression>? modifiers = null,
            RelationalTypeMapping? typeMapping = null)
        {
            modifiers ??= Enumerable.Empty<SqlExpression>();

            // If the inner call is another strftime then shortcut a double call
            if (timestring is SqlFunctionExpression rtrimFunction
                && rtrimFunction.Name == "rtrim"
                && rtrimFunction.Arguments!.Count == 2
                && rtrimFunction.Arguments[0] is SqlFunctionExpression rtrimFunction2
                && rtrimFunction2.Name == "rtrim"
                && rtrimFunction2.Arguments!.Count == 2
                && rtrimFunction2.Arguments[0] is SqlFunctionExpression strftimeFunction
                && strftimeFunction.Name == "strftime"
                && strftimeFunction.Arguments!.Count > 1)
            {
                // Use its timestring parameter directly in place of ours
                timestring = strftimeFunction.Arguments[1];

                // Prepend its modifier arguments (if any) to the current call
                modifiers = strftimeFunction.Arguments.Skip(2).Concat(modifiers);
            }

            var finalArguments = new[] { timestring, sqlExpressionFactory.Constant(format) }.Concat(modifiers);

            return sqlExpressionFactory.Function(
                "DATE_TO_STR",
                finalArguments,
                nullable: true,
                argumentsPropagateNullability: finalArguments.Select(a => true),
                returnType,
                typeMapping);
        }
        
        // public static SqlFunctionExpression DateAdd(
        //     ISqlExpressionFactory sqlExpressionFactory,
        //     Type returnType,
        //     SqlExpression timeSource,
        //     string unit, 
        //     SqlExpression amount,
        //     IEnumerable<SqlExpression>? modifiers,
        //     RelationalTypeMapping? typeMapping = null
        // )
        // {
        //     var finalArguments = new[] { timeSource, sqlExpressionFactory.Constant($"INTERVAL {amount} {unit}") };
        //
        //     return sqlExpressionFactory.Function(
        //         "DATE_ADD",
        //         finalArguments,
        //         nullable: true,
        //         argumentsPropagateNullability: finalArguments.Select(a => true),
        //         returnType,
        //         typeMapping
        //     );
        // }
    }

    // public static SqlFunctionExpression Position(
    //     ISqlExpressionFactory sqlExpressionFactory,
    //     Type returnType,
    //     SqlExpression value,
    //     SqlExpression set,
    //     IEnumerable<SqlExpression>? modifiers = null,
    //     RelationalTypeMapping? typeMapping = null)
    // {
    //     var finalArguments = new List<SqlExpression>().Concat(modifiers);
    //     return sqlExpressionFactory.Function(
    //         "position",
    //         finalArguments,
    //         nullable: false,
    //         argumentsPropagateNullability:finalArguments.Select(a => true),
    //         returnType, typeMapping
    //     );
    // }

}
