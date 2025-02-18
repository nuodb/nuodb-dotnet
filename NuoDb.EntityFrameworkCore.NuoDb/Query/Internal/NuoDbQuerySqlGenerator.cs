// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using NuoDb.EntityFrameworkCore.NuoDb.Internal;

namespace NuoDb.EntityFrameworkCore.NuoDb.Query.Internal
{

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class NuoDbQuerySqlGenerator : QuerySqlGenerator
    {
        private readonly IRelationalCommandBuilder _relationalCommandBuilder;
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public NuoDbQuerySqlGenerator(QuerySqlGeneratorDependencies dependencies)
            : base(dependencies)
        {
           // _relationalCommandBuilder = dependencies.
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override string GetOperator(SqlBinaryExpression binaryExpression)
        {
            Check.NotNull(binaryExpression, nameof(binaryExpression));

            return binaryExpression.OperatorType == ExpressionType.Add
                && binaryExpression.Type == typeof(string)
                    ? " || "
                    : base.GetOperator(binaryExpression);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>

        public virtual Expression VisitNuoDbComplexFunctionArgumentExpression(NuoDbComplexFunctionArgumentExpression nuoDbComplexFunctionArgumentExpression)
        {
            Check.NotNull(nuoDbComplexFunctionArgumentExpression, nameof(nuoDbComplexFunctionArgumentExpression));

            var first = true;
            foreach (var argument in nuoDbComplexFunctionArgumentExpression.ArgumentParts)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    Sql.Append(nuoDbComplexFunctionArgumentExpression.Delimiter);
                }

                Visit(argument);
            }

            return nuoDbComplexFunctionArgumentExpression;
        }

         
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override void GenerateLimitOffset(SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            if (selectExpression.Limit != null
                || selectExpression.Offset != null)
            {
                
                Sql.AppendLine();
                if (selectExpression.Limit != null)
                {
                    Sql.Append("Limit ");
                    Visit(selectExpression.Limit);
                }

                if (selectExpression.Offset != null)
                {
                    Sql.Append(" OFFSET ");
                    Visit(selectExpression.Offset);
                }
            }
        }

       protected override Expression VisitOrdering(OrderingExpression orderingExpression)
        {
            Check.NotNull(orderingExpression, nameof(orderingExpression));

            if (orderingExpression.Expression is SqlConstantExpression
                || orderingExpression.Expression is SqlParameterExpression)
            {
                Sql.Append("(1)");
            }
            else
            {
                Visit(orderingExpression.Expression);
            }

            if (!orderingExpression.IsAscending)
            {
                Sql.Append(" DESC");
            }

            return orderingExpression;
        }

        private SqlUnaryExpression VisitConvert(SqlUnaryExpression sqlUnaryExpression)
        {
        
            Visit(sqlUnaryExpression.Operand);
        
            return sqlUnaryExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override void GenerateSetOperationOperand(SetOperationBase setOperation, SelectExpression operand)
        {
            Check.NotNull(setOperation, nameof(setOperation));
            Check.NotNull(operand, nameof(operand));

            // NuoDb doesn't support parentheses around set operation operands
            Visit(operand);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override void GeneratePseudoFromClause()
        {
            Sql.Append(" FROM DUAL");
        }

        protected override void GenerateOrderings(SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            base.GenerateOrderings(selectExpression);

            // In SQL Server, if an offset is specified, then an ORDER BY clause must also exist.
            // Generate a fake one.
            if (!selectExpression.Orderings.Any() && selectExpression.Offset != null)
            {
                Sql.AppendLine().Append("ORDER BY (1)");
            }
        }


        protected override Expression VisitRowNumber(RowNumberExpression rowNumberExpression)
        {
            Check.NotNull(rowNumberExpression, nameof(rowNumberExpression));

            // rewrite default ordering expression for row number expressions
            if (rowNumberExpression.Orderings.Count == 1 && rowNumberExpression.Orderings.Any(x =>
                    x.Expression is SqlFragmentExpression fragment && fragment.Sql == "(SELECT 1)"))
            {
                var orderExpression = rowNumberExpression.Orderings[0];
                var newOrderings = new List<OrderingExpression>
                    { new OrderingExpression(new SqlFragmentExpression("(0)"),orderExpression.IsAscending) }.AsReadOnly();
                var updatedExpression = rowNumberExpression.Update(rowNumberExpression.Partitions, newOrderings);
                return base.VisitRowNumber(updatedExpression);
            }

            return base.VisitRowNumber(rowNumberExpression);
        }

       

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override void CheckComposableSql(string sql)
        {
            //we override this intentionally so as to allow use of complex argument expressions
            //(allowing atypical function calls like DATE_ADD(date, 2 interval day) and POSITION('blah' in c.CompanyName) ) 
        }
    }
}
