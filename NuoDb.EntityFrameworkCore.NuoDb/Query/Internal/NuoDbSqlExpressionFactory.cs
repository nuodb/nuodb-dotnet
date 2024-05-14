using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace NuoDb.EntityFrameworkCore.NuoDb.Query.Internal
{
    public class NuoDbSqlExpressionFactory: SqlExpressionFactory
    {
        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly RelationalTypeMapping _boolTypeMapping;
        private readonly RelationalTypeMapping _doubleTypeMapping;
        
        public NuoDbSqlExpressionFactory(SqlExpressionFactoryDependencies dependencies)
            : base(dependencies)
        {
            _typeMappingSource = dependencies.TypeMappingSource;
            _boolTypeMapping = _typeMappingSource.FindMapping(typeof(bool));
            _doubleTypeMapping = _typeMappingSource.FindMapping(typeof(double));
        }
        public virtual SqlFragmentExpression Fragment(string sql)
        {
            Check.NotNull(sql, nameof(sql));

            return new SqlFragmentExpression(sql);
        }
        public virtual NuoDbComplexFunctionArgumentExpression ComplexFunctionArgument(
            IEnumerable<SqlExpression> argumentParts,
            string delimiter,
            Type argumentType,
            RelationalTypeMapping typeMapping = null)
        {
            var typeMappedArgumentParts = new List<SqlExpression>();

            foreach (var argument in argumentParts)
            {
                typeMappedArgumentParts.Add(ApplyDefaultTypeMapping(argument));
            }

            return (NuoDbComplexFunctionArgumentExpression)ApplyTypeMapping(
                new NuoDbComplexFunctionArgumentExpression(
                    typeMappedArgumentParts,
                    delimiter,
                    argumentType,
                    typeMapping),
                typeMapping);
        }

        public virtual SqlFunctionExpression NullableFunction(
            string name,
            IEnumerable<SqlExpression> arguments,
            Type returnType,
            RelationalTypeMapping typeMapping = null,
            bool onlyNullWhenAnyNullPropagatingArgumentIsNull = true,
            IEnumerable<bool> argumentsPropagateNullability = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(arguments, nameof(arguments));
            Check.NotNull(returnType, nameof(returnType));

            var typeMappedArguments = new List<SqlExpression>();

            foreach (var argument in arguments)
            {
                typeMappedArguments.Add(ApplyDefaultTypeMapping(argument));
            }

            return new SqlFunctionExpression(
                name,
                typeMappedArguments,
                true,
                onlyNullWhenAnyNullPropagatingArgumentIsNull
                    ? (argumentsPropagateNullability ?? typeMappedArguments.Select(x=>true))
                    : typeMappedArguments.Select(x=>true),
                returnType,
                typeMapping);
        }

        public override SqlExpression ApplyTypeMapping(SqlExpression sqlExpression, RelationalTypeMapping typeMapping)
            => sqlExpression is not { TypeMapping: null }
                ? sqlExpression
                : ApplyNewTypeMapping(sqlExpression, typeMapping);

        private SqlExpression ApplyNewTypeMapping(SqlExpression sqlExpression, RelationalTypeMapping typeMapping)
            => sqlExpression switch
            {
                // Customize handling for binary expressions.
                //SqlBinaryExpression e => ApplyTypeMappingOnSqlBinary(e, typeMapping),
                NuoDbComplexFunctionArgumentExpression e => ApplyTypeMappingOnComplexFunctionArgument(e),

                _ => base.ApplyTypeMapping(sqlExpression, typeMapping)
            };

        private SqlBinaryExpression ApplyTypeMappingOnSqlBinary(SqlBinaryExpression sqlBinaryExpression, RelationalTypeMapping typeMapping)
        {
            // The default SqlExpressionFactory behavior is to assume that the two operands have the same type, and so to infer one side's
            // mapping from the other if needed. Here we take care of some heterogeneous operand cases where this doesn't work.

            var left = sqlBinaryExpression.Left;
            var right = sqlBinaryExpression.Right;

            var newSqlBinaryExpression = (SqlBinaryExpression)base.ApplyTypeMapping(sqlBinaryExpression, typeMapping);

            // // Handle the special case, that a JSON value is compared to a string (e.g. when used together with
            // // JSON_EXTRACT()).
            // // The string argument should not be interpreted as a JSON value, which it normally would due to inference
            // // if its type mapping hasn't been explicitly set before, but just as a string.
            // if (newSqlBinaryExpression.Left.TypeMapping is MySqlJsonTypeMapping newLeftTypeMapping &&
            //     newLeftTypeMapping.ClrType == typeof(string) &&
            //     right.TypeMapping is null &&
            //     right.Type == typeof(string))
            // {
            //     newSqlBinaryExpression = new SqlBinaryExpression(
            //         sqlBinaryExpression.OperatorType,
            //         ApplyTypeMapping(left, newLeftTypeMapping),
            //         ApplyTypeMapping(right, _typeMappingSource.FindMapping(right.Type)),
            //         newSqlBinaryExpression.Type,
            //         newSqlBinaryExpression.TypeMapping);
            // }
            // else if (newSqlBinaryExpression.Right.TypeMapping is MySqlJsonTypeMapping newRightTypeMapping &&
            //          newRightTypeMapping.ClrType == typeof(string) &&
            //          left.TypeMapping is null &&
            //          left.Type == typeof(string))
            // {
            //     newSqlBinaryExpression = new SqlBinaryExpression(
            //         sqlBinaryExpression.OperatorType,
            //         ApplyTypeMapping(left, _typeMappingSource.FindMapping(left.Type)),
            //         ApplyTypeMapping(right, newRightTypeMapping),
            //         newSqlBinaryExpression.Type,
            //         newSqlBinaryExpression.TypeMapping);
            // }

            return newSqlBinaryExpression;
        }
        private NuoDbComplexFunctionArgumentExpression ApplyTypeMappingOnComplexFunctionArgument(NuoDbComplexFunctionArgumentExpression complexFunctionArgumentExpression)
        {
            var inferredTypeMapping = ExpressionExtensions.InferTypeMapping(complexFunctionArgumentExpression.ArgumentParts.ToArray())
                                      ?? (complexFunctionArgumentExpression.Type.IsArray
                                          ? _typeMappingSource.FindMapping(
                                              complexFunctionArgumentExpression.Type.GetElementType() ??
                                              complexFunctionArgumentExpression.Type)
                                          : _typeMappingSource.FindMapping(complexFunctionArgumentExpression.Type));

            return new NuoDbComplexFunctionArgumentExpression(
                complexFunctionArgumentExpression.ArgumentParts,
                complexFunctionArgumentExpression.Delimiter,
                complexFunctionArgumentExpression.Type,
                inferredTypeMapping ?? complexFunctionArgumentExpression.TypeMapping);
        }
    }
}
