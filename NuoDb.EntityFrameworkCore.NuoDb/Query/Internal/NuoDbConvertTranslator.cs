using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace NuoDb.EntityFrameworkCore.NuoDb.Query.Internal
{
    public class NuoDbConvertTranslator: IMethodCallTranslator
    {
        private static readonly Dictionary<string, string> _typeMapping = new()
        {
            [nameof(Convert.ToBoolean)] = "boolean",
            [nameof(Convert.ToByte)] = "smallint",
            [nameof(Convert.ToDecimal)] = "decimal",
            [nameof(Convert.ToDouble)] = "double",
            [nameof(Convert.ToInt16)] = "smallint",
            [nameof(Convert.ToInt32)] = "int",
            [nameof(Convert.ToInt64)] = "bigint",
            [nameof(Convert.ToString)] = "string"
        };

        private static readonly List<Type> _supportedTypes = new()
        {
            typeof(bool),
            typeof(byte),
            typeof(DateTime),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(int),
            typeof(long),
            typeof(short),
            typeof(string)
        };

        private static readonly IEnumerable<MethodInfo> _supportedMethods
            = _typeMapping.Keys
                .SelectMany(
                    t => typeof(Convert).GetTypeInfo().GetDeclaredMethods(t)
                        .Where(
                            m => m.GetParameters().Length == 1
                                 && _supportedTypes.Contains(m.GetParameters().First().ParameterType)));

        private readonly NuoDbSqlExpressionFactory _sqlExpressionFactory;

         public NuoDbConvertTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            Check.NotNull(method, nameof(method));
            Check.NotNull(arguments, nameof(arguments));
            Check.NotNull(logger, nameof(logger));

            return _supportedMethods.Contains(method)
                ? _sqlExpressionFactory.Function(
                    "CAST",
                    new[]
                    {
                        _sqlExpressionFactory.ComplexFunctionArgument(
                            new []
                            {
                                arguments[0],
                                _sqlExpressionFactory.Fragment("as"),
                                _sqlExpressionFactory.Fragment(_typeMapping[method.Name])
                            },
                            " ",
                            typeof(string)
                            )
                    },
                    nullable: true,
                    argumentsPropagateNullability: new[] { false, true },
                    method.ReturnType)
                : null;
        }
    }
}
