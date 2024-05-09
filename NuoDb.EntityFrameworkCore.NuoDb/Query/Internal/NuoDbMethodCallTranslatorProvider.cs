// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query;

namespace NuoDb.EntityFrameworkCore.NuoDb.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class NuoDbMethodCallTranslatorProvider : RelationalMethodCallTranslatorProvider
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public NuoDbMethodCallTranslatorProvider(RelationalMethodCallTranslatorProviderDependencies dependencies)
            : base(dependencies)
        {
            var sqlExpressionFactory = dependencies.SqlExpressionFactory;

            AddTranslators(
                new IMethodCallTranslator[]
                {
                    new NuoDbByteArrayMethodTranslator(sqlExpressionFactory, dependencies.RelationalTypeMappingSource),
                    new NuoDbCharMethodTranslator(sqlExpressionFactory),
                    new NuoDbDateTimeAddTranslator(sqlExpressionFactory),
                    new NuoDbGlobMethodTranslator(sqlExpressionFactory),
                    new NuoDbHexMethodTranslator(sqlExpressionFactory),
                    new NuoDbMathTranslator(sqlExpressionFactory),
                    new NuoDbObjectToStringTranslator(sqlExpressionFactory),
                    new NuoDbRandomTranslator(sqlExpressionFactory),
                    new NuoDbRegexMethodTranslator(sqlExpressionFactory),
                    new NuoDbStringMethodTranslator(sqlExpressionFactory),
                    new NuoDbSubstrMethodTranslator(sqlExpressionFactory)
                });
        }
    }
}
